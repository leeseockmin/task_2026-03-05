using BackEnd.Application.Interfaces;
using BackEnd.Application.Interfaces.Employee;
using BackEnd.Infrastructure.DataBase;
using BackEnd.Infrastructure.Logging;
using BackEnd.Infrastructure.Persistence.Read;
using BackEnd.Infrastructure.Repositories;
using DB.Data.AccountDB;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));


//마이그레이션 기능에게 하나만 먼저 추가
builder.Services.AddDbContext<AccountDBContext>(option =>
	option.UseMySql(
		builder.Configuration.GetConnectionString("WriteConnection")!,
		ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("WriteConnection")!)));

var writeConnection = builder.Configuration.GetConnectionString("WriteConnection");
builder.Services.AddKeyedSingleton<IDbContextFactory<AccountDBContext>>("Write", (sp, key) => {
	var options = new DbContextOptionsBuilder<AccountDBContext>()
		.UseMySql(writeConnection!, ServerVersion.AutoDetect(writeConnection!))
		.Options;
	return new SimpleDbContextFactory<AccountDBContext>(options);
});

// Slave 설정
var readConnection = builder.Configuration.GetConnectionString("ReadConnection");
builder.Services.AddKeyedSingleton<IDbContextFactory<AccountDBContext>>("Read", (sp, key) => {
	var options = new DbContextOptionsBuilder<AccountDBContext>()
		.UseMySql(readConnection!, ServerVersion.AutoDetect(readConnection!))
		.Options;
	return new SimpleDbContextFactory<AccountDBContext>(options);
});


builder.Services.AddSingleton<DataBaseManager>();

// MongoDB
builder.Services.AddSingleton<MongoDB.Driver.IMongoClient>(_ =>
    new MongoDB.Driver.MongoClient(builder.Configuration.GetConnectionString("MongoDB")));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<MongoDB.Driver.IMongoClient>()
      .GetDatabase(builder.Configuration["MongoDB:DatabaseName"]));
builder.Services.AddSingleton<IMongoLogService, MongoLogService>();

RepositoryServiceRegistration.AddRepositories(builder.Services);


var app = builder.Build();

// DataBaseManager 초기화 확인
var dbManager = app.Services.GetService<DataBaseManager>();
if (dbManager is null)
{
    throw new InvalidOperationException("DataBaseManager 초기화에 실패했습니다.");
}

// 최신 마이그레이션이 적용되지 않은 경우 서버 시작을 중단합니다.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AccountDBContext>();
    var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
    if (pendingMigrations.Count > 0)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AccountDBContext>>();
        logger.LogError($"미적용 마이그레이션이 있습니다: {string.Join(", ", pendingMigrations)}");
        throw new InvalidOperationException(
            $"미적용 마이그레이션이 있습니다. 서버를 시작하기 전에 마이그레이션을 적용하세요.\n" +
            $"미적용 목록: {string.Join(", ", pendingMigrations)}");
    }
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
