using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Reflection;

namespace Diplom_CRM.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var projectDirectory = Path.GetDirectoryName(assemblyLocation)
                ?? throw new InvalidOperationException("Не удалось определить директорию проекта.");

            var configPath = Path.Combine(projectDirectory, "appsettings.Development.json");

            if (!File.Exists(configPath))
            {
                var parentDirectory = Directory.GetParent(projectDirectory)?.FullName;
                if (parentDirectory != null)
                {
                    configPath = Path.Combine(parentDirectory, "appsettings.Development.json");
                }
            }

            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Не найден файл конфигурации: {configPath}");

            var basePath = Path.GetDirectoryName(configPath)
                ?? throw new InvalidOperationException($"Не удалось получить директорию для пути: {configPath}");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(Path.GetFileName(configPath))
                .Build();

            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Строка подключения 'Default' не найдена в конфигурации.");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}