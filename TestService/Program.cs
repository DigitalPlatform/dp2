using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace TestService
{
    // https://csharp.christiannagel.com/2019/10/15/windowsservice/
    // https://github.com/ProfessionalCSharp/MoreSamples/tree/master/DotnetCore/WindowsServiceSample/WindowsServiceSample
    class Program
    {
        static void Main(string[] args)
        {
            // Console.WriteLine("Hello World!");

            CreateHostBuilder(args).Build().Run();
        }

        /*
        public static IHostBuilder CreateHostBuilder(string[] args) =>
  Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>()
            .Configure<EventLogSettings>(config =>
        {
            config.LogName = "Sample Service";
            config.SourceName = "Sample Service Source";
        });
    });
    */

        public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureLogging(
            options => options.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Information))
        .ConfigureServices((hostContext, services) =>
        {
            services.AddHostedService<Worker>()
                .Configure<EventLogSettings>(config =>
                {
                    config.LogName = "Sample Service";
                    config.SourceName = "Sample Service Source";
                });
        }).UseWindowsService();
    }
}
