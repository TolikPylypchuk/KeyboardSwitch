using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KeyboardSwitch
{
    public static class Program
    {
        public static void Main(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .UseConsoleLifetime()
                .UseSystemd()
                .Build()
                .Run();

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.AddHostedService<Worker>();
        }
    }
}
