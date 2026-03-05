using BackEnd.Application.Interfaces.Employee;
using DB.Data.AccountDB;
using BackEnd.Infrastructure.DataBase;
using Dapper;

namespace BackEnd.Infrastructure.Repositories
{
    public class EmployeeCommandRepository : IEmployeeCommandRepository
    {
        private readonly DataBaseManager _dbManager;
        private readonly ILogger<EmployeeCommandRepository> _logger;

        public EmployeeCommandRepository(DataBaseManager dbManager, ILogger<EmployeeCommandRepository> logger)
        {
            _dbManager = dbManager;
            _logger = logger;
        }

        public async Task<int> InsertAsync(Employee employee)
        {
            return await _dbManager.ExecuteAsync(DataBaseManager.DBType.Write, async connection =>
            {
                const string sql = $@"
                    INSERT INTO Employee ({nameof(Employee.name)}, {nameof(Employee.email)}, {nameof(Employee.tel)}, {nameof(Employee.joined)}, {nameof(Employee.createdAt)})
                    VALUES (@{nameof(Employee.name)}, @{nameof(Employee.email)}, @{nameof(Employee.tel)}, @{nameof(Employee.joined)}, @{nameof(Employee.createdAt)});
                    SELECT LAST_INSERT_ID();
                    ";

                return await connection.QuerySingleAsync<int>(sql, new
                {
                    employee.name,
                    employee.email,
                    employee.tel,
                    employee.joined,
                    employee.createdAt
                });
            });
        }

        public async Task<int> UpdateAsync(Employee employee)
        {

            return await _dbManager.ExecuteAsync(DataBaseManager.DBType.Write, async connection =>
            {
                const string sql = $@"
                    UPDATE Employee
                    SET {nameof(Employee.name)} = @{nameof(Employee.name)},
                        {nameof(Employee.email)} = @{nameof(Employee.email)},
                        {nameof(Employee.tel)} = @{nameof(Employee.tel)},
                        {nameof(Employee.joined)} = @{nameof(Employee.joined)},
                        {nameof(Employee.updatedAt)} = @{nameof(Employee.updatedAt)}
                    WHERE {nameof(Employee.employeeId)} = @{nameof(Employee.employeeId)}
                    ";

                return await connection.ExecuteAsync(sql, new
                {
                    employee.employeeId,
                    employee.name,
                    employee.email,
                    employee.tel,
                    employee.joined,
                    employee.updatedAt
                });
            });
        }

        public async Task<int> DeleteAsync(int employeeId)
        {

            return await _dbManager.ExecuteAsync(DataBaseManager.DBType.Write, async connection =>
            {
                const string sql = $"DELETE FROM Employee WHERE {nameof(Employee.employeeId)} = @{nameof(Employee.employeeId)}";
                return await connection.ExecuteAsync(sql, new { employeeId });
            });
        }
    }
}
