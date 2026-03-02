using System.Security.Claims;
using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.DTOs.ProjectMember.Request;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LetopiaPlatform.API.Controllers;
//[Route("api/[controller]")]
[ApiController]
public class ProjectMemberController : BaseController
{
    private readonly IProjectMemberService _memberService;
    public ProjectMemberController(IProjectMemberService memberService) => _memberService = memberService;


    private Guid GetUserIdFromToken()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }

    [HttpPost(Router.ProjectMembers.Join)]
    [Authorize]
    public async Task<IActionResult> JoinProject([FromRoute] string projectTitle)
    {
        var userId = GetUserIdFromToken();


        var request = new ProjectMemberRequestDto(userId, projectTitle);

        var result = await _memberService.JoinProjectAsync(request, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    [HttpPost(Router.ProjectMembers.Leave)]
    [Authorize]
    public async Task<IActionResult> LeaveProject([FromRoute] string projectTitle)
    {
        var userId = GetUserIdFromToken();
        var request = new ProjectMemberRequestDto(userId, projectTitle);

        var result = await _memberService.LeaveProjectAsync(request, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    [HttpGet(Router.ProjectMembers.GetMembers)]
    public async Task<IActionResult> GetProjectMembers([FromRoute] Guid projectId)
    {
        HttpContext.AddBusinessContext("action", "get_project_members");

        var result = await _memberService.GetProjectMembersAsync(projectId, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    [HttpGet(Router.ProjectMembers.MyProjects)]
    [Authorize]
    public async Task<IActionResult> GetMyProjects()
    {

        var userId = GetUserIdFromToken();

        var result = await _memberService.GetMyProjectsAsync(userId, HttpContext.RequestAborted);
        return HandleResult(result);
    }
}
