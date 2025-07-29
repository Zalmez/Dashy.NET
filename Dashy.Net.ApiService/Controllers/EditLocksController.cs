using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Dashy.Net.Shared.Models;
using Dashy.Net.ApiService.Services;

namespace Dashy.Net.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EditLocksController : ControllerBase
{
    private readonly ApiEditLockService _editLockService;
    private readonly ILogger<EditLocksController> _logger;

    public EditLocksController(
        ApiEditLockService editLockService,
        ILogger<EditLocksController> logger)
    {
        _editLockService = editLockService;
        _logger = logger;
    }

    [HttpGet("{dashboardId}")]
    public ActionResult<EditLockDto?> GetEditLock(int dashboardId)
    {
        var lockInfo = _editLockService.GetCurrentLock(dashboardId);
        var lockDto = _editLockService.ToDto(lockInfo);

        if (lockDto == null)
        {
            return Ok((EditLockDto?)null);
        }

        return Ok(lockDto);
    }

    [HttpPost("acquire")]
    public ActionResult<EditLockResponse> AcquireEditLock([FromBody] AcquireEditLockDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous User";

        bool lockAcquired;

        if (request.ForceAcquire)
        {
            lockAcquired = _editLockService.ForceAcquireLock(request.DashboardId, userId, userName);
        }
        else
        {
            lockAcquired = _editLockService.TryAcquireLock(request.DashboardId, userId, userName);
        }

        var currentLock = _editLockService.GetCurrentLock(request.DashboardId);
        var response = new EditLockResponse
        {
            Success = lockAcquired,
            CurrentLock = _editLockService.ToDto(currentLock)
        };

        if (!lockAcquired && currentLock != null)
        {
            response.ErrorMessage = $"Dashboard is currently being edited by {currentLock.UserName}";
        }

        return Ok(response);
    }

    [HttpPost("{dashboardId}/release")]
    public IActionResult ReleaseEditLock(int dashboardId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

        var released = _editLockService.ReleaseLock(dashboardId, userId);

        if (released)
        {
            _logger.LogInformation("Edit lock released for dashboard {DashboardId} by user {UserId}", dashboardId, userId);
            return Ok();
        }
        else
        {
            _logger.LogWarning("Failed to release edit lock for dashboard {DashboardId} by user {UserId} - lock not held by user", dashboardId, userId);
            return BadRequest("Lock not held by current user or lock does not exist");
        }
    }

    [HttpPost("{dashboardId}/activity")]
    public IActionResult UpdateActivity(int dashboardId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

        _editLockService.UpdateActivity(dashboardId, userId);

        return Ok();
    }
}
