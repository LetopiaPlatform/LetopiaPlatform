using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.DTOs.Project.Request;
using LetopiaPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LetopiaPlatform.API.Controllers;

[ApiController]
public class ProjectController : BaseController
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    // GET: api/projects/discover
    [HttpGet(Router.Projects.Discover)]
    [AllowAnonymous]
    public async Task<IActionResult> DiscoverFilter([FromQuery] ProjectFilterDto filter)
    {
        HttpContext.AddBusinessContext("action", "discover_projects");

        var result = await _projectService.GetDiscoverAsync(filter, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // GET: api/projects/{id}
    [HttpGet(Router.Projects.GetDetailsById)]
    [AllowAnonymous]
    public async Task<IActionResult> GetDetails([FromRoute] Guid id)
    {
        HttpContext.AddBusinessContext("action", "get_project_details");
        HttpContext.AddBusinessContext("project_id", id.ToString());

        var result = await _projectService.GetDetailsAsync(id, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // POST: api/projects/create
    [HttpPost(Router.Projects.Create)]
    [AllowAnonymous]
    public async Task<IActionResult> CreateProject([FromForm] CreateProjectRequestDto request)
    {
        HttpContext.AddBusinessContext("action", "create_project");
        HttpContext.AddBusinessContext("owner_id", request.OwnerId.ToString());


        var result = await _projectService.CreateAsync(request, HttpContext.RequestAborted);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetDetails), new { id = result.Value }, result);
        }

        return HandleResult(result);
    }

    // PUT: api/v1/projects/{id}
    [HttpPut(Router.Projects.Update)]
    [Authorize]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromForm] UpdateProjectRequestDto request)
    {
        if (id != request.Id) return BadRequest("Project ID mismatch.");

        HttpContext.AddBusinessContext("action", "update_project");
        HttpContext.AddBusinessContext("project_id", id.ToString());

        var result = await _projectService.UpdateAsync(id, request, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // DELETE: api/v1/projects/{id}
    [HttpDelete(Router.Projects.Delete)]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        HttpContext.AddBusinessContext("action", "delete_project");
        HttpContext.AddBusinessContext("project_id", id.ToString());

        var result = await _projectService.DeleteAsync(id, HttpContext.RequestAborted);
        return HandleResult(result);
    }

}
