using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuthService.Application.DTOs.Request;
using AuthService.Application.DTOs.Response;
using AuthService.Application.Interfaces;
using AuthService.Infrastructure.Persistence;
using AuthService.Shared.Wrappers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace AuthService.IntegrationTest;

public sealed class AuthenticationFlowTests : IClassFixture<AuthApiFactory>
{
    private readonly HttpClient _client;

    public AuthenticationFlowTests(AuthApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RegistrationLoginAndProtectedProfile_WorkTogether()
    {
        var email = $"account-{Guid.NewGuid():N}@example.test";
        var registration = await _client.PostAsJsonAsync("/api/auth/register", new RegisterAccountRequest
        {
            Email = email,
            DisplayName = "Sample Account",
            Password = "SecurePassword123"
        });

        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);
        var registeredSession = await registration.Content.ReadFromJsonAsync<ApiResponse<AuthSessionDto>>();
        Assert.NotNull(registeredSession?.Data?.RefreshToken);

        var duplicate = await _client.PostAsJsonAsync("/api/auth/register", new RegisterAccountRequest
        {
            Email = email,
            DisplayName = "Sample Account",
            Password = "SecurePassword123"
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/auth/sign-in", new SignInRequest
        {
            Email = email,
            Password = "SecurePassword123"
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var session = await login.Content.ReadFromJsonAsync<ApiResponse<AuthSessionDto>>();
        Assert.NotNull(session?.Data?.AccessToken);

        using var profileRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        profileRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session!.Data!.AccessToken);
        var profileResponse = await _client.SendAsync(profileRequest);
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);

        var profile = await profileResponse.Content.ReadFromJsonAsync<ApiResponse<CurrentAccountDto>>();
        Assert.Equal(email, profile?.Data?.Email);

        var refresh = await _client.PostAsJsonAsync("/api/auth/refresh-token", new RefreshSessionRequest
        {
            RefreshToken = registeredSession!.Data!.RefreshToken
        });
        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);

        var reusedRefreshToken = await _client.PostAsJsonAsync("/api/auth/refresh-token", new RefreshSessionRequest
        {
            RefreshToken = registeredSession.Data.RefreshToken
        });
        Assert.Equal(HttpStatusCode.Unauthorized, reusedRefreshToken.StatusCode);

        using var revokeRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/revoke-token")
        {
            Content = JsonContent.Create(new RevokeSessionRequest
            {
                RefreshToken = session!.Data!.RefreshToken
            })
        };
        revokeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Data.AccessToken);
        var revokeResponse = await _client.SendAsync(revokeRequest);
        Assert.Equal(HttpStatusCode.OK, revokeResponse.StatusCode);

        var revokedRefreshToken = await _client.PostAsJsonAsync("/api/auth/refresh-token", new RefreshSessionRequest
        {
            RefreshToken = session.Data.RefreshToken
        });
        Assert.Equal(HttpStatusCode.Unauthorized, revokedRefreshToken.StatusCode);

        var failedLogin = await _client.PostAsJsonAsync("/api/auth/sign-in", new SignInRequest
        {
            Email = email,
            Password = "IncorrectPassword123"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, failedLogin.StatusCode);
    }
}

public sealed class AuthApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Jwt:Issuer"] = "AuthService.Tests",
                ["Authentication:Jwt:Audience"] = "AuthService.Tests.Client",
                ["Authentication:Jwt:SigningKey"] = "AuthService-integration-tests-signing-key-2026"
            });
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAccountDirectory>();
            services.RemoveAll<IAccountRepository>();
            services.RemoveAll<IRefreshSessionStore>();
            services.AddSingleton<InMemoryAccountStore>();
            services.AddSingleton<IAccountDirectory>(provider => provider.GetRequiredService<InMemoryAccountStore>());
            services.AddSingleton<IAccountRepository>(provider => provider.GetRequiredService<InMemoryAccountStore>());
            services.AddSingleton<IRefreshSessionStore, InMemoryRefreshSessionStore>();
        });
    }
}