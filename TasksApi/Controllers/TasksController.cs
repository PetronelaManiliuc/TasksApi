using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TasksApi.Interfaces;
using TasksApi.Requests;
using TasksApi.Responses;

namespace TasksApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : BaseApiController
    {
        private readonly ITasksService tasksService;
        public TasksController(ITasksService tasksService)
        {
            this.tasksService = tasksService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetTasks()
        {
            var tasksResponse = await tasksService.GetTasks(UserId);
            if (!tasksResponse.Success)
            {

                return UnprocessableEntity(tasksResponse);
            }

            return Ok(tasksResponse.Tasks.ConvertAll(x => new TaskResponse { Id = x.Id, IsCompleted = x.IsCompleted, Name = x.Name, Ts = x.Ts }));
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var taskResponse = await tasksService.GetTask(id);
            if (taskResponse != null)
            {

                return UnprocessableEntity(taskResponse);
            }

            return Ok(taskResponse);
        }

        [HttpPost, Route("add")]
        public async Task<IActionResult> Post(TaskRequest taskRequest)
        {

            var task = new Entities.Task { Id = taskRequest.Id, IsCompleted = taskRequest.IsCompleted, Name = taskRequest.Name, Ts = taskRequest.Ts, UserId = UserId };

            SaveTaskResponse saveTaskResponse = await tasksService.SaveTask(task);

            if (!saveTaskResponse.Success)
            {
                return UnprocessableEntity(saveTaskResponse);
            }

            return Ok(new TaskResponse { Id = saveTaskResponse.Task.Id, IsCompleted = saveTaskResponse.Task.IsCompleted, Name = saveTaskResponse.Task.Name, Ts = saveTaskResponse.Task.Ts });
        }

        [HttpPut, Route("update")]
        public async Task<IActionResult> Put(TaskRequest taskRequest)
        {
            var task = new Entities.Task { Id = taskRequest.Id, IsCompleted = taskRequest.IsCompleted, Name = taskRequest.Name, Ts = taskRequest.Ts, UserId = UserId };
            SaveTaskResponse saveTaskResponse = await tasksService.UpdateTask(task);
            if (!saveTaskResponse.Success)
            {
                return UnprocessableEntity(saveTaskResponse);
            }

            return Ok(new TaskResponse { Id = saveTaskResponse.Task.Id, IsCompleted = saveTaskResponse.Task.IsCompleted, Name = saveTaskResponse.Task.Name, Ts = saveTaskResponse.Task.Ts });
        }

        [HttpDelete, Route("delete")]
        public async Task<IActionResult> Delete(int taskId)
        {
            var task = await tasksService.DeleteTask(taskId);
            if (!task.Success)
            {
                return UnprocessableEntity(task);
            }

            return Ok(task);
        }
    }
}
