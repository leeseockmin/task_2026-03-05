using BackEnd.Application.Interfaces.Employee;
using DB.Data.AccountDB;
using BackEnd.Infrastructure.DataBase;
using Dapper;
using System.Text;
using System.Data.Common;

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

        public async Task<int> BulkInsertAsync(List<Employee> employees)
        {
            if (employees.Count == 0)
            {
                return 0;
            }

            int affectedCount = 0;
            DateTime updatedAt = DateTime.UtcNow;

            await _dbManager.ExecuteTransactionAsync(DataBaseManager.DBType.Write, async (connection, transaction) =>
            {
                var sqlBuilder = new StringBuilder();
                sqlBuilder.Append($@"INSERT INTO Employee ({nameof(Employee.name)}, {nameof(Employee.email)}, {nameof(Employee.tel)}, {nameof(Employee.joined)}, {nameof(Employee.createdAt)}) VALUES ");

                var parameters = new DynamicParameters();
                for (int i = 0; i < employees.Count; i++)
                {
                    if (i > 0)
                    {
                        sqlBuilder.Append(", ");
                    }

                    sqlBuilder.Append($"(@{nameof(Employee.name)}{i}, @{nameof(Employee.email)}{i}, @{nameof(Employee.tel)}{i}, @{nameof(Employee.joined)}{i}, @{nameof(Employee.createdAt)}{i})");

                    parameters.Add($"{nameof(Employee.name)}{i}", employees[i].name);
                    parameters.Add($"{nameof(Employee.email)}{i}", employees[i].email);
                    parameters.Add($"{nameof(Employee.tel)}{i}", employees[i].tel);
                    parameters.Add($"{nameof(Employee.joined)}{i}", employees[i].joined);
                    parameters.Add($"{nameof(Employee.createdAt)}{i}", employees[i].createdAt);
                }

                // email UNIQUE KEY 기준 — 중복 시 name / tel / joined / updatedAt 갱신
                // createdAt 은 최초 등록 시각이므로 갱신하지 않습니다.
                sqlBuilder.Append($@" ON DUPLICATE KEY UPDATE
                    {nameof(Employee.name)}      = VALUES({nameof(Employee.name)}),
                    {nameof(Employee.tel)}       = VALUES({nameof(Employee.tel)}),
                    {nameof(Employee.joined)}    = VALUES({nameof(Employee.joined)}),
                    {nameof(Employee.updatedAt)} = @updatedAt");

                parameters.Add("updatedAt", updatedAt);

                var sql = sqlBuilder.ToString();
                affectedCount = await connection.ExecuteAsync(sql, parameters, transaction: transaction);

                return true; // INSERT(1) / UPDATE(2) / 변경없음(0) 모두 유효
            });

            return affectedCount;
        }
    }
}
