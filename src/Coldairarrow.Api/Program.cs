using Coldairarrow.Util;
using Colder.Logging.Serilog;
using EFCore.Sharding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Coldairarrow.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureLoggingDefaults()
                .UseIdHelper()
                .UseCache()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddFxServices();
                    services.AddAutoMapper();
                    services.AddEFCoreSharding(config =>
                    {
                        config.SetEntityAssemblies(GlobalAssemblies.AllAssemblies);

                        var baseDbOptions = hostContext.Configuration.GetSection("Database:BaseDb").Get<DatabaseOptions>();
                        config.UseDatabase(baseDbOptions.ConnectionString, baseDbOptions.DatabaseType);

                        var orderDbOptions = hostContext.Configuration.GetSection("Database:OrderDb").Get<DatabaseOptions>();
                        config.UseDatabase(orderDbOptions.ConnectionString, baseDbOptions.DatabaseType);
                    });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .Build()
                .Run();
        }
    }
}
