namespace TasksApi.Interfaces
{
    public interface ITokenService
    {
        Task<Tuple<string, string>> GenerateTokens(int userId);
    }
}
