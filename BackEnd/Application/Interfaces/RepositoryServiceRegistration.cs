using BackEnd.Application.Interfaces.Employee;
using BackEnd.Infrastructure.Persistence.Read;
using BackEnd.Infrastructure.Repositories;       

namespace Microsoft.Extensions.DependencyInjection // 권장: DI 네임스페이스를 맞추면 쓰기 편합니다.
{
	public static class RepositoryServiceRegistration
	{
		public static IServiceCollection AddRepositories(this IServiceCollection services)
		{
			// Query Repositories
			services.AddScoped<IEmployeeQueryRepository, EmployeeQueryRepository>();

			// Command Repositories
			services.AddScoped<IEmployeeCommandRepository, EmployeeCommandRepository>();

			// 앞으로 추가될 리포지토리들도 여기에 작성
			// services.AddScoped<IDepartmentRepository, DepartmentRepository>();

			return services;
		}
	}
}