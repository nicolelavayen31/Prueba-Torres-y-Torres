$ErrorActionPreference = 'Stop'

$authBaseUrl = if ($env:AUTH_API_URL) { $env:AUTH_API_URL } else { 'http://localhost:5087/api/auth' }
$businessBaseUrl = if ($env:BUSINESS_API_URL) { $env:BUSINESS_API_URL } else { 'http://localhost:8000/api' }
$sqlServer = if ($env:SQLCMD_SERVER) { $env:SQLCMD_SERVER } else { '.\SQLEXPRESS06' }
$accountEmail = "flow-$([guid]::NewGuid().ToString('N'))@example.test"
$firstCustomerEmail = "first-$([guid]::NewGuid().ToString('N'))@example.test"
$secondCustomerEmail = "second-$([guid]::NewGuid().ToString('N'))@example.test"

function Assert-Flow([bool]$condition, [string]$message) {
    if (-not $condition) {
        throw $message
    }
}

function Get-AuthHeaders([string]$token) {
    return @{ Authorization = "Bearer $token"; Accept = 'application/json' }
}

try {
    $registration = Invoke-RestMethod -Method Post -Uri "$authBaseUrl/register" `
        -ContentType 'application/json' `
        -Body (@{ email = $accountEmail; displayName = 'Cross Service Test'; password = 'SecurePassword123' } | ConvertTo-Json) `
        -ErrorAction Stop
    Assert-Flow ($registration.succeeded -and $registration.data.accountId -gt 0) 'AuthService registration failed.'

    $login = Invoke-RestMethod -Method Post -Uri "$authBaseUrl/sign-in" `
        -ContentType 'application/json' `
        -Body (@{ email = $accountEmail; password = 'SecurePassword123' } | ConvertTo-Json) `
        -ErrorAction Stop
    Assert-Flow ($login.succeeded -and $login.data.accessToken) 'AuthService login failed.'
    $headers = Get-AuthHeaders $login.data.accessToken

    $profile = Invoke-RestMethod -Method Get -Uri "$authBaseUrl/me" -Headers $headers -ErrorAction Stop
    Assert-Flow ($profile.data.email -eq $accountEmail) 'Protected AuthService profile failed.'

    $customerList = Invoke-RestMethod -Method Get -Uri "$businessBaseUrl/customers" -Headers $headers -ErrorAction Stop
    Assert-Flow $customerList.success 'Laravel rejected the .NET JWT.'

    $firstCustomer = Invoke-RestMethod -Method Post -Uri "$businessBaseUrl/customers" `
        -Headers $headers -ContentType 'application/json' `
        -Body (@{ name = 'Cross Service One'; email = $firstCustomerEmail; phone = '555-0101'; address = 'Test address one' } | ConvertTo-Json) `
        -ErrorAction Stop
    $secondCustomer = Invoke-RestMethod -Method Post -Uri "$businessBaseUrl/customers" `
        -Headers $headers -ContentType 'application/json' `
        -Body (@{ name = 'Cross Service Two'; email = $secondCustomerEmail; phone = '555-0102'; address = 'Test address two' } | ConvertTo-Json) `
        -ErrorAction Stop
    $firstCustomerId = [int]$firstCustomer.data.id
    $secondCustomerId = [int]$secondCustomer.data.id
    Assert-Flow ($firstCustomerId -gt 0 -and $secondCustomerId -gt 0) 'Laravel customer creation failed.'

    $createdOrder = Invoke-RestMethod -Method Post -Uri "$businessBaseUrl/orders" `
        -Headers $headers -ContentType 'application/json' `
        -Body (@{
            customer_id = $firstCustomerId
            notes = 'Initial order detail'
            items = @(
                @{ description = 'Initial item A'; quantity = 2; unit_price = 10.00 },
                @{ description = 'Initial item B'; quantity = 1; unit_price = 5.00 }
            )
        } | ConvertTo-Json -Depth 6) `
        -ErrorAction Stop
    $orderId = [int]$createdOrder.data.id
    Assert-Flow ($orderId -gt 0 -and $createdOrder.data.items.Count -eq 2) 'Laravel order creation failed.'

    $updatedOrder = Invoke-RestMethod -Method Put -Uri "$businessBaseUrl/orders/$orderId" `
        -Headers $headers -ContentType 'application/json' `
        -Body (@{
            customer_id = $secondCustomerId
            notes = 'Updated order detail'
            items = @(
                @{ description = 'Replacement item A'; quantity = 3; unit_price = 12.50 },
                @{ description = 'Replacement item B'; quantity = 1; unit_price = 5.00 }
            )
        } | ConvertTo-Json -Depth 6) `
        -ErrorAction Stop
    Assert-Flow ($updatedOrder.data.customer_id -eq $secondCustomerId) 'Order customer reassignment failed.'
    Assert-Flow ($updatedOrder.data.notes -eq 'Updated order detail') 'Order notes update failed.'
    Assert-Flow ($updatedOrder.data.items.Count -eq 2) 'Order item replacement failed.'
    Assert-Flow ([decimal]$updatedOrder.data.total -eq [decimal]42.50) 'Order total trigger did not recalculate the replacement items.'

    $filteredOrders = Invoke-RestMethod -Method Get `
        -Uri "$businessBaseUrl/orders?status=Pending&customer_id=$secondCustomerId" `
        -Headers $headers -ErrorAction Stop
    $matchingOrders = @($filteredOrders.data.items | Where-Object { $_.id -eq $orderId })
    Assert-Flow ($matchingOrders.Count -gt 0) 'Order filtering by status/customer failed.'

    $dashboard = Invoke-RestMethod -Method Get -Uri "$businessBaseUrl/dashboard/stats" -Headers $headers -ErrorAction Stop
    $activityByDay = Invoke-RestMethod -Method Get -Uri "$businessBaseUrl/dashboard/orders-by-day" -Headers $headers -ErrorAction Stop
    $activityByMonth = Invoke-RestMethod -Method Get -Uri "$businessBaseUrl/dashboard/orders-by-month" -Headers $headers -ErrorAction Stop
    Assert-Flow ($dashboard.success -and $activityByDay.success -and $activityByMonth.success) 'Dashboard endpoints failed.'

    $completedOrder = Invoke-RestMethod -Method Patch -Uri "$businessBaseUrl/orders/$orderId/complete" `
        -Headers $headers -ContentType 'application/json' -Body '{}' -ErrorAction Stop
    Assert-Flow ($completedOrder.data.status -eq 'Completed') 'Order completion failed.'

    $refresh = Invoke-RestMethod -Method Post -Uri "$authBaseUrl/refresh-token" `
        -ContentType 'application/json' `
        -Body (@{ refreshToken = $login.data.refreshToken } | ConvertTo-Json) `
        -ErrorAction Stop
    Assert-Flow ($refresh.succeeded -and $refresh.data.refreshToken -ne $login.data.refreshToken) 'Refresh-token rotation failed.'

    $reuseRejected = $false
    try {
        Invoke-RestMethod -Method Post -Uri "$authBaseUrl/refresh-token" `
            -ContentType 'application/json' `
            -Body (@{ refreshToken = $login.data.refreshToken } | ConvertTo-Json) `
            -ErrorAction Stop | Out-Null
    }
    catch {
        $reuseRejected = [int]$_.Exception.Response.StatusCode -eq 401
    }
    Assert-Flow $reuseRejected 'A previously consumed refresh token was accepted again.'

    $revoked = Invoke-RestMethod -Method Post -Uri "$authBaseUrl/revoke-token" `
        -Headers $headers -ContentType 'application/json' `
        -Body (@{ refreshToken = $refresh.data.refreshToken } | ConvertTo-Json) `
        -ErrorAction Stop
    Assert-Flow $revoked.succeeded 'Refresh-token revocation failed.'

    [pscustomobject]@{
        RegistrationAndLogin = 'PASS'
        DotNetJwtAcceptedByLaravel = 'PASS'
        CustomerCrudAndReassignment = 'PASS'
        OrderCreateAndFullItemEdit = 'PASS'
        SQLTotalTrigger = 'PASS'
        StatusFiltersAndCompletion = 'PASS'
        DailyMonthlyDashboard = 'PASS'
        RefreshRotationAndRevocation = 'PASS'
    } | Format-List
}
finally {
    if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
        $cleanupQuery = "USE CustomerOrdersDB; DELETE FROM dbo.Customers WHERE Email IN (N'$firstCustomerEmail', N'$secondCustomerEmail'); DELETE FROM dbo.Users WHERE Email = N'$accountEmail';"
        sqlcmd -S $sqlServer -E -b -Q $cleanupQuery | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Warning 'Automatic cleanup could not complete; inspect the two cross-service-test emails in CustomerOrdersDB.'
        }
    }
}
