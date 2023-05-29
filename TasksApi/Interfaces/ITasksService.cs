using TasksApi.Responses;

namespace TasksApi.Interfaces
{
    public interface ITasksService
    {
        Task<GetTasksResponse> GetTasks(int userId);

        Task<TaskResponse> GetTask(int id);

        Task<SaveTaskResponse> SaveTask(Entities.Task taskEntity);

        Task<SaveTaskResponse> UpdateTask(Entities.Task taskEntity);

        Task<DeleteTaskResponse> DeleteTask(int taskId);
    }
}
