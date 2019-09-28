using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TwitchVote
{
    public class Program
    {
        public static IConfigurationRoot? Configuration { get; set; }

        private static void Main(string[] args)
        {
            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) || devEnvironmentVariable.ToLower() == "development";
            //Determines the working environment as IHostingEnvironment is unavailable in a console app

            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables();


            if (isDevelopment) //only add secrets in development
            {
                builder.AddUserSecrets<TwitchInfo>();
            }

            Configuration = builder.Build();

            var services = new ServiceCollection()
                .Configure<TwitchInfo>(Configuration.GetSection(nameof(TwitchInfo)))
                .AddOptions()
                .BuildServiceProvider();


            var bot = new VoteBot(services.GetService<IOptions<TwitchInfo>>());

            Console.ReadKey();
        }
    }
}
