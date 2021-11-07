using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpotifyAnalysis.Database;
using SpotifyAnalysis.Models.Configuration;
using SpotifyAnalysis.Processing;
using System;
using System.Threading.Tasks;

namespace SpotifyAnalysis
{
    public class Program
    {
#if DEBUG
        private const string Environment = "Development";
#else
        private const string Environment = "Production";
#endif

        private static IServiceProvider ConfigureServices(IServiceCollection serviceProvider)
        {
            // Config setup.
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.{Environment.ToLower()}.json", false, true)
                .Build();

            AppConfiguration appConfiguration = new AppConfiguration();
            configuration.Bind("AppConfig", appConfiguration);

            serviceProvider.AddSingleton(appConfiguration);
            serviceProvider.AddTransient<Transformer>();

            // Logging setup.
            serviceProvider.AddLogging(opt =>
            {
                opt
                .AddConsole()
                .AddConfiguration(configuration.GetSection("Logging"));
            });

            // Database DI.
            serviceProvider.AddDbContext<SpotifyAnalysisContext>(options => options.UseNpgsql(appConfiguration.Database.ConnectionString)
                .EnableSensitiveDataLogging(Environment.Equals("Development"))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution));

            // Main app.
            serviceProvider.AddHostedService<AnalysisProgram>();

            return serviceProvider.BuildServiceProvider(true);
        }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).UseConsoleLifetime().Build().RunAsync().GetAwaiter().GetResult();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
                .CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IServiceProvider serviceProvider = ConfigureServices(services);
                });
    }
}