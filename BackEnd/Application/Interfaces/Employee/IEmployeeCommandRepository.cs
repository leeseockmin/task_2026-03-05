
namespace BackEnd.Application.Interfaces.Employee
{
    public interface IEmployeeCommandRepository
    {
        Task<int> BulkInsertAsync(List<DB.Data.AccountDB.Employee> employees);
    }
}
