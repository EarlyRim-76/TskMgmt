using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TskMgmt.Api.Interface;

namespace TskMgmt.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly IGenericService<Entities.Comment> _genericService;

        public CommentController(IGenericService<Entities.Comment> genericService)
        {
            _genericService = genericService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProject()
        {
            try
            {
                var projects = await _genericService.GetAllAsync();
                if (projects == null || !projects.Any())
                    return NotFound("No current data is added ");
                return Ok(projects);
            }
            catch (Exception e)
            {
                return Problem(detail: $"Message {e.Message}", statusCode: 500, title: "Error");
            }
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetOne(Guid id)
        {
            try
            {
                var task = await _genericService.GetByIdAsync(id);

                if (task == null)
                    return NotFound("No record found");

                return Ok(task);
            }
            catch (Exception e)
            {
                return Problem(detail: $"Message {e.Message}", statusCode: 500, title: "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Entities.Comment comment)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Error object is not valid");


                var res = await _genericService.CreateAsync(comment);

                if (res is null)
                    return NotFound("Data is not found");

                return Ok(res);
            }
            catch (Exception e)
            {
                return Problem(detail: $"Message {e.Message}", statusCode: 500, title: "Error");
            }

        }


        [HttpPut]
        public async Task<IActionResult> Update(Entities.Comment comment)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Error object is not valid");

                var existingComment = await _genericService.GetByIdAsync(comment.Id);

                if (existingComment is null)
                    return NotFound("No records found");

                existingComment.Body = comment.Body;
                existingComment.UpdatedAt = DateTime.UtcNow;

                var res = await _genericService.UpdateAsync(existingComment);

                return Ok(res);
            }
            catch (Exception e)
            {
                return Problem(detail: $"Message {e.Message}", statusCode: 500, title: "Error");
            }
        }




        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var task = await _genericService.GetByIdAsync(id);

                if (task is null)
                    return NotFound("No record found");

                bool isDeleted = await _genericService.DeleteAsync(task.Id);

                if (!isDeleted)
                    return NotFound("Failed to delete the record");

                return NoContent();

            }
            catch (Exception e)
            {
                return Problem(detail: $"Message {e.Message}", statusCode: 500, title: "Error");
            }
        }


    }
}
