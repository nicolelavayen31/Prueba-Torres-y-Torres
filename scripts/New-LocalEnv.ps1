$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$environmentPath = Join-Path $repositoryRoot '.env'

if (Test-Path -LiteralPath $environmentPath) {
    throw 'A .env file already exists. It was not changed.'
}

function New-Base64UrlValue([int]$byteCount) {
    $bytes = [byte[]]::new($byteCount)
    $generator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $generator.GetBytes($bytes)
    }
    finally {
        $generator.Dispose()
    }

    return [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

$saPassword = 'Aa1!' + (New-Base64UrlValue 48)
$jwtSecret = New-Base64UrlValue 48
$applicationKeyBytes = [byte[]]::new(32)
$applicationKeyGenerator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
try {
    $applicationKeyGenerator.GetBytes($applicationKeyBytes)
}
finally {
    $applicationKeyGenerator.Dispose()
}

$applicationKey = 'base64:' + [Convert]::ToBase64String($applicationKeyBytes)

$environmentContent = @"
MSSQL_SA_PASSWORD=$saPassword
AUTH_JWT_SIGNING_KEY=$jwtSecret
LARAVEL_APP_KEY=$applicationKey
SQL_PORT=14330
"@

[System.IO.File]::WriteAllText(
    $environmentPath,
    $environmentContent.Trim() + [Environment]::NewLine,
    [System.Text.UTF8Encoding]::new($false))

Write-Output 'Created a private local .env with random development credentials.'