using BackEnd.Application.Interfaces.Employee;
using BackEnd.Infrastructure.DataBase;
using BackEnd.Infrastructure.Persistence.Read;
using BackEnd.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Design;
using DB.Data.AccountDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// Serilog — 애플리케이션 로그 (파일 출력)
// =============================================
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "Logs/app-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
});

// =============================================
// Web API 기본 서비스
// =============================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =============================================
// MediatR — CQRS 핸들러 등록
// =============================================
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));


//마이그레이션 기능에게 하나만 먼저 추가
builder.Services.AddDbContext<AccountDBContext>(option =>
	option.UseMySQL(builder.Configuration.GetConnectionString("WirteConnection")!));

var wirteConnection = builder.Configuration.GetConnectionString("WirteConnection");
builder.Services.AddKeyedSingleton<IDbContextFactory<AccountDBContext>>("Write", (sp, key) => {
	var options = new DbContextOptionsBuilder<AccountDBContext>()
		.UseMySQL(wirteConnection!)
		.Options;
	return new DbContextFactory<AccountDBContext>(sp, options, new DbContextFactorySource<AccountDBContext>());
});

// Slave 설정
var readConnection = builder.Configuration.GetConnectionString("ReadConnection");
builder.Services.AddKeyedSingleton<IDbContextFactory<AccountDBContext>>("Read", (sp, key) => {
	var options = new DbContextOptionsBuilder<AccountDBContext>()
		.UseMySQL(readConnection!)
		.Options;
	return new DbContextFactory<AccountDBContext>(sp, options, new DbContextFactorySource<AccountDBContext>());
});


builder.Services.AddSingleton<DataBaseManager>();

RepositoryServiceRegistration.AddRepositories(builder.Services);

// =============================================
// HTTP Pipeline
// =============================================
var app = builder.Build();

// DataBaseManager 초기화 확인
var dbManager = app.Services.GetService<DataBaseManager>();
if (dbManager is null)
{
    throw new InvalidOperationException("DataBaseManager 초기화에 실패했습니다.");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
