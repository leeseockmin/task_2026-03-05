using BackEnd.Application.DTOs.Employee;
using BackEnd.Application.Interfaces.Employee;
using BackEnd.Infrastructure.DataBase;
using DB.Data.AccountDB;
using Dapper;

namespace BackEnd.Infrastructure.Persistence.Read
{
    public class EmployeeQueryRepository : IEmployeeQueryRepository
    {
        private readonly DataBaseManager _dbManager;
        private readonly ILogger<EmployeeQueryRepository> _logger;

		public EmployeeQueryRepository(DataBaseManager dbManager, ILogger<EmployeeQueryRepository> logger)
        {
            _dbManager = dbManager;
            _logger = logger;
        }

        public async Task<List<EmployeeDto>> GetListAsync(int page, int pageSize)
        {
            return await _dbManager.ExecuteAsync(DataBaseManager.DBType.Read, async connection =>
            {
                
                const string sql = $@"
                    SELECT {nameof(Employee.employeeId)}, {nameof(Employee.name)}, {nameof(Employee.email)}, {nameof(Employee.tel)}, {nameof(Employee.joined)}
                    FROM Employee
                    ORDER BY {nameof(Employee.joined)} ASC
                    LIMIT @PageSize OFFSET @Offset
                    ";

                var result = await connection.QueryAsync<EmployeeDto>(
                    sql,
                    new { PageSize = pageSize, Offset = (page - 1) * pageSize });

                return result.ToList();
            });
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _dbManager.ExecuteAsync(DataBaseManager.DBType.Read, async connection =>
            {
                const string sql = @"SELECT COUNT(*) FROM Employee";
                return await connection.ExecuteScalarAsync<int>(sql);
            });
        }

        public async Task<EmployeeDto?> GetByNameAsync(string name)
        {
            return await _dbManager.ExecuteAsync(DataBaseManager.DBType.Read, async connection =>
            {
                const string sql = $@"
                    SELECT {nameof(Employee.employeeId)}, {nameof(Employee.name)}, {nameof(Employee.email)}, {nameof(Employee.tel)}, {nameof(Employee.joined)}
                    FROM Employee
                    WHERE {nameof(Employee.name)} = @{nameof(Employee.name)}
                    LIMIT 1
                    ";

                return await connection.QueryFirstOrDefaultAsync<EmployeeDto>(sql, new { name });
            });
        }
    }
}
