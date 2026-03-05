
namespace BackEnd.Application.Interfaces.Employee
{
    public interface IEmployeeCommandRepository
    {
        Task<int> InsertAsync(DB.Data.AccountDB.Employee employee);
        Task<int> UpdateAsync(DB.Data.AccountDB.Employee employee);
        Task<int> DeleteAsync(int employeeId);
    }
}
