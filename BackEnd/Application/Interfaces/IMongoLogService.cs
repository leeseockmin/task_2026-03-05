namespace BackEnd.Application.Interfaces
{
    public interface IMongoLogService
    {
        Task LogAsync<T>(string tableName, string action, T payload);
    }
}
