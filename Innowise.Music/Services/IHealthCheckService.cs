/*
 * @file: IHealthCheckService.cs
 * @description: Interface for checking backend services health.
 * @dependencies: None
 * @created: 2026-04-22
 */
namespace Innowise.Music.Services;

public interface IHealthCheckService
{
    Task<bool> CheckIdentityServerHealthAsync();
}
