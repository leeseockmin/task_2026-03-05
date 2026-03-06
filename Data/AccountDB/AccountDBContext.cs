using Microsoft.EntityFrameworkCore;

namespace DB.Data.AccountDB
{
    public class AccountDBContext : DbContext
    {
        public AccountDBContext(DbContextOptions<AccountDBContext> options) : base(options)
        {
        }

        public DbSet<Employee> employee { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8mb4");

            var configureMethod = typeof(IModelCreateEntity).GetMethod("CreateModel");

            var entityTypes = typeof(AccountDBContext).Assembly.GetTypes()
                .Where(t => typeof(IModelCreateEntity).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in entityTypes)
            {
                var instance = Activator.CreateInstance(type) as IModelCreateEntity;
                configureMethod?.Invoke(instance, new object[] { modelBuilder });
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
