using AuthService.Application.DTOs.Response;
using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IAccessTokenIssuer
{
    IssuedAccessToken IssueFor(Account account);
}