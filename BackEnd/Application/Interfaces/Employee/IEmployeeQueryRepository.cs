using BackEnd.Application.DTOs.Employee;

namespace BackEnd.Application.Interfaces.Employee
{
    public interface IEmployeeQueryRepository
    {
        Task<IReadOnlyList<EmployeeDto>> GetListAsync(int page, int pageSize);
        Task<int> GetTotalCountAsync();
        Task<EmployeeDto?> GetByNameAsync(string name);
    }
}
