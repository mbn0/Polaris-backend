using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace backend.Data
{
    public class PolarisDbContextFactory : IDesignTimeDbContextFactory<PolarisDbContext>
    {
        public PolarisDbContext CreateDbContext(string[] args)
        {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Read connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Configure the context
            var optionsBuilder = new DbContextOptionsBuilder<PolarisDbContext>();
            optionsBuilder.UseSqlServer(connectionString); // or UseNpgsql / UseSqlite if applicable

            return new PolarisDbContext(optionsBuilder.Options);
        }
    }
}

