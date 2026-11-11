using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TskMgmt.Api.Interface;
using TskMgmt.Api.Service;

namespace TskMgmt.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class TaskController : ControllerBase
    {

        private readonly IGenericService<Entities.Task> _genericService;
        
        public TaskController(IGenericService<Entities.Task> genericService)
        {
            _genericService = genericService;
        }


        [HttpGet]
        public async  Task<IActionResult> GetAllTask()
        {
            try
            {
                var tasks = await _genericService.GetAllAsync();

                if(tasks == null || !tasks.Any())
                    return NotFound("No current data is added ");

                return Ok(tasks);


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

                if(task == null)
                    return NotFound("No record found");

                return Ok(task);
            }
            catch(Exception e)
            {
                return Problem(detail:$"Message {e.Message}", statusCode: 500, title: "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Entities.Task task)
        {
            try 
            { 
                if (!ModelState.IsValid)
                    return BadRequest("Error object is not valid");


                var res = await _genericService.CreateAsync(task);

                if (res is null)
                    return NotFound("Data is not found");

                return Ok(res);
             }
            catch(Exception e)
            {
                return Problem(detail:$"Message {e.Message}", statusCode: 500, title: "Error");
            }

        }


        [HttpPut]
        public async Task<IActionResult> Update(Entities.Task task)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Error object is not valid");

                var existingTask = await _genericService.GetByIdAsync(task.Id);

                if(existingTask is null)
                    return NotFound("No records found");

                existingTask.Title = task.Title;
                existingTask.Description = task.Description;
                existingTask.Status = task.Status;
                existingTask.Priority = task.Priority;
                existingTask.DueDate = task.DueDate;
                existingTask.EstimatedHours = task.EstimatedHours;
                existingTask.UpdatedAt = DateTime.UtcNow;

                var res = await _genericService.UpdateAsync(existingTask);

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
                
                if(task is null) 
                    return NotFound("No record found");

                bool isDeleted = await _genericService.DeleteAsync(task.Id);

                if(!isDeleted)
                    return NotFound("Failed to delete the record");

                return NoContent();

            }
            catch(Exception e)
            {
                return Problem(detail: $"Message {e.Message}", statusCode: 500, title: "Error");
            }
        }




        //Add Patch if needed only

    }
}
