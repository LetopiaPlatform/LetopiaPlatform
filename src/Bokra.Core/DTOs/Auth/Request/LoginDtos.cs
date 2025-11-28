namespace Bokra.Core.DTOs.Auth.Request;

public record LoginRequest
(
    string Email,
    string Password
);
