using DB.Data.AccountDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Data.Common;
using System.Transactions;

namespace BackEnd.Infrastructure.DataBase
{
    public class DataBaseManager
    {
        public enum DBType
        {
            None = 0,
            Write = 1,
            Read = 2
        }

		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<DataBaseManager> _logger;

		public DataBaseManager(ILogger<DataBaseManager> logger, IServiceProvider serviceProvider)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
		}

		private async Task<DbContext> GetDBContextAsync(DBType dbType)
        {
			IDbContextFactory<AccountDBContext>? factory = null;

			switch (dbType)
			{
				case DBType.Write:
					factory = _serviceProvider.GetRequiredKeyedService<IDbContextFactory<AccountDBContext>>("Write");
					break;

				case DBType.Read:
					factory = _serviceProvider.GetRequiredKeyedService<IDbContextFactory<AccountDBContext>>("Read");
					break;

			}

            if(factory == null)
            {
                return null;
            }

			return await factory.CreateDbContextAsync();
		}

        /// <summary>단일 DB 실행 (반환값 없음)</summary>
        public async Task ExecuteAsync(DBType dbType, Func<DbConnection, Task> func)
        {
            await using var context = await GetDBContextAsync(dbType);
            try
            {
                await context.Database.OpenConnectionAsync();
                await func(context.Database.GetDbConnection());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DB 실행 오류. DBType: {DBType}", dbType);
                throw;
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }

        /// <summary>단일 DB 실행 (반환값 있음)</summary>
        public async Task<T> ExecuteAsync<T>(DBType dbType, Func<DbConnection, Task<T>> func)
        {
            await using var context = await GetDBContextAsync(dbType);
            try
            {
                await context.Database.OpenConnectionAsync();
                return await func(context.Database.GetDbConnection());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DB 실행 오류. DBType: {DBType}", dbType);
                throw;
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }

        /// <summary>
        /// 두 DB 동시 실행. dbType 순서와 func 파라미터 순서가 동일해야 합니다.
        /// </summary>
        public async Task ExecuteAsync(DBType dbType1, DBType dbType2, Func<DbConnection, DbConnection, Task> func)
        {
            await using var context1 = await GetDBContextAsync(dbType1);
            await using var context2 = await GetDBContextAsync(dbType2);
            try
            {
                await context1.Database.OpenConnectionAsync();
                await context2.Database.OpenConnectionAsync();
                await func(context1.Database.GetDbConnection(), context2.Database.GetDbConnection());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DB 실행 오류. DBType1: {DBType1}, DBType2: {DBType2}", dbType1, dbType2);
                throw;
            }
            finally
            {
                await context1.Database.CloseConnectionAsync();
                await context2.Database.CloseConnectionAsync();
            }
        }

        /// <summary>단일 DB 트랜잭션 실행. func가 true를 반환해야 커밋됩니다.</summary>
        public virtual async Task ExecuteTransactionAsync(DBType dbType, Func<DbConnection, Task<bool>> func)
        {
            await using var context = await GetDBContextAsync(dbType);
            try
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                await context.Database.OpenConnectionAsync();

                bool isSuccess = await func(context.Database.GetDbConnection());
                if (isSuccess)
                {
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "트랜잭션 오류. DBType: {DBType}", dbType);
                throw;
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }

        /// <summary>
        /// 두 DB 트랜잭션 실행. dbType 순서와 func 파라미터 순서가 동일해야 합니다.
        /// func가 true를 반환해야 커밋됩니다.
        /// </summary>
        public virtual async Task ExecuteTransactionAsync(DBType dbType1, DBType dbType2, Func<DbConnection, DbConnection, Task<bool>> func)
        {
            await using var context1 = await GetDBContextAsync(dbType1);
            await using var context2 = await GetDBContextAsync(dbType2);
            try
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                await context1.Database.OpenConnectionAsync();
                await context2.Database.OpenConnectionAsync();

                bool isSuccess = await func(context1.Database.GetDbConnection(), context2.Database.GetDbConnection());
                if (isSuccess)
                {
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "트랜잭션 오류. DBType1: {DBType1}, DBType2: {DBType2}", dbType1, dbType2);
                throw;
            }
            finally
            {
                await context1.Database.CloseConnectionAsync();
                await context2.Database.CloseConnectionAsync();
            }
        }
    }
}
