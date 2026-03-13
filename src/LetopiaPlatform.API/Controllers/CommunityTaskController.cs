using System.Security.Claims;
using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.DTOs.CommunityTask.Request;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LetopiaPlatform.API.Controllers;
//[Route("api/[controller]")]
[ApiController]
public class CommunityTaskController : BaseController
{
    private readonly ICommunityTaskService _taskService;

    public CommunityTaskController(ICommunityTaskService taskService)
    {
        _taskService = taskService;
    }

    private Guid GetUserIdFromToken()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }

    // GET: api/communities/{communityId}/tasks?status=1
    [HttpGet(Router.CommunityTask.GetAll)]
    [Authorize]
    public async Task<IActionResult> GetAll([FromRoute] Guid communityId, [FromQuery] CommunityTaskStatus status = CommunityTaskStatus.All)
    {
        var userId = GetUserIdFromToken();
        HttpContext.AddBusinessContext("action", "get_filtered_tasks");
        HttpContext.AddBusinessContext("community_id", communityId.ToString());
        HttpContext.AddBusinessContext("status_filter", status.ToString());

        var result = await _taskService.GetTasksAsync(communityId, userId, status, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // GET: api/communities/{communityId}/tasks/today-progress
    [HttpGet(Router.CommunityTask.GetTodayProgress)]
    [Authorize]
    public async Task<IActionResult> GetTodayProgress([FromRoute] Guid communityId)
    {
        var userId = GetUserIdFromToken();
        HttpContext.AddBusinessContext("action", "get_today_progress");
        HttpContext.AddBusinessContext("community_id", communityId.ToString());

        var result = await _taskService.GetTodayProgressAsync(communityId, userId, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // POST: api/communities/{communityId}/tasks
    [HttpPost(Router.CommunityTask.Create)]
    [Authorize]
    public async Task<IActionResult> Create([FromRoute] Guid communityId, [FromBody] CreateTaskRequestDto request)
    {
        var userId = GetUserIdFromToken();
        HttpContext.AddBusinessContext("action", "create_task");
        HttpContext.AddBusinessContext("community_id", communityId.ToString());

        var result = await _taskService.CreateAsync(communityId, userId, request, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // PUT: api/communities/tasks/{id}
    [HttpPut(Router.CommunityTask.Update)]
    [Authorize]
    public async Task<IActionResult> Update([FromRoute] Guid communityTaskid, [FromBody] UpdateTaskRequestDto request)
    {
        var userId = GetUserIdFromToken();
        HttpContext.AddBusinessContext("action", "update_task");
        HttpContext.AddBusinessContext("task_id", communityTaskid.ToString());

        var result = await _taskService.UpdateAsync(communityTaskid, userId, request, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // DELETE: api/communities/tasks/{id}
    [HttpDelete(Router.CommunityTask.Delete)]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] Guid communityTaskid)
    {
        var userId = GetUserIdFromToken();
        HttpContext.AddBusinessContext("action", "delete_task");
        HttpContext.AddBusinessContext("task_id", communityTaskid.ToString());

        var result = await _taskService.DeleteAsync(communityTaskid, userId, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // PATCH: api/communities/tasks/{id}/toggle
    [HttpPatch(Router.CommunityTask.Toggle)]
    [Authorize]
    public async Task<IActionResult> ToggleCompletion([FromRoute] Guid taskid)
    {
        var userId = GetUserIdFromToken();
        HttpContext.AddBusinessContext("action", "toggle_task_completion");
        HttpContext.AddBusinessContext("task_id", taskid.ToString());

        var result = await _taskService.ToggleCompletionAsync(taskid, userId, HttpContext.RequestAborted);
        return HandleResult(result);
    }
}
