using Microsoft.EntityFrameworkCore;
using TasksApi.Entities;
using TasksApi.Interfaces;
using TasksApi.Requests;
using TasksApi.Responses;

namespace TasksApi.Services
{
    public class TasksService : ITasksService
    {
        private readonly TasksDbContext dbContext;

        public TasksService(TasksDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<TaskResponse> GetTask(int id)
        {

            Entities.Task task = await dbContext.Tasks.Where(x => x.Id == id).FirstOrDefaultAsync();

            return new TaskResponse()
            {
                Id = task.Id,
                IsCompleted = task.IsCompleted,
                Name = task.Name,
                Ts = task.Ts
            };
        }

        public async Task<GetTasksResponse> GetTasks(int userId)
        {
            List<Entities.Task> tasks = await dbContext.Tasks.Where(x => x.UserId == userId).ToListAsync();

            return new GetTasksResponse()
            {
                Success = true,
                Tasks = tasks
            };
        }

        public async Task<SaveTaskResponse> SaveTask(Entities.Task taskEntity)
        {
            if (taskEntity.Id == 0)
            {
                await dbContext.Tasks.AddAsync(taskEntity);
            }

            var taskResponse = await dbContext.SaveChangesAsync();

            if (taskResponse > 0)
            {
                return new SaveTaskResponse { Task = taskEntity, Success = true };
            }

            return new SaveTaskResponse { Success = false, Error = "Unable to save the task.", ErrorCode = "T01" };
        }

        public async Task<SaveTaskResponse> UpdateTask(Entities.Task taskEntity)
        {

            dbContext.Tasks.Update(taskEntity);

            var taskResponse = await dbContext.SaveChangesAsync();

            if (taskResponse > 0)
            {
                return new SaveTaskResponse { Task = taskEntity, Success = true };
            }

            return new SaveTaskResponse { Success = false, Error = "Unable to update the task.", ErrorCode = "T02" };
        }

        public async Task<DeleteTaskResponse> DeleteTask(int taskId)
        {
            Entities.Task? task = await dbContext.Tasks.FindAsync(taskId);
            if (task != null)
            {
                dbContext.Tasks.Remove(task);
                var taskResponse = await dbContext.SaveChangesAsync();
                if (taskResponse > 0)
                {
                    return new DeleteTaskResponse { Success = true, Error = "Task deleted with success!" };
                }
            }

            return new DeleteTaskResponse { Success = false, Error = "Unable to delete the task.", ErrorCode = "T03" };
        }
    }
}
