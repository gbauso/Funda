using CommandLine;
using Funda.Application;
using Funda.CLI.Extensions;
using Funda.Core.Model;
using Funda.Core.Ports;
using Funda.Infra.Http;
using Funda.Infra.Repository;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Funda.CLI
{
    public class Program
    {
        public static IConfiguration Configuration;

        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();
            ITopSellerOperations operations = host.Services.GetRequiredService<ITopSellerOperations>();
            CancellationToken token = new CancellationToken();
            Parser.Default.ParseArguments<Options>(args).
                WithParsed(o =>
                {
                    var observable = operations.FetchTopSellers(token, o.WithGarden, o.ForceFetching);

                    observable.Subscribe((result) =>
                    {
                        Console.Clear();
                        if (result.Status != FetchStatus.Skipped)
                            ConsoleHelper.WriteProgress(result.FetchingProgress);

                    }, async () =>
                    {
                        Console.Clear();
                        IEnumerable<TopSellers> topSellers = await operations.GetTopSellers(o.WithGarden);
                        ConsoleHelper.WriteTable(topSellers);
                    });

                });
            host.WaitForShutdown();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureServices(((hostContext, serviceCollection) =>
            {
                serviceCollection.AddHttpClient("HouseOffer", (cfg => cfg.Timeout = TimeSpan.FromSeconds(10.0)));
                serviceCollection.AddTransient<HttpRetryPolicy>();
                serviceCollection.AddTransient<IHouseOfferProvider, HouseOfferHttpClient>();
                serviceCollection.AddTransient<IHouseOfferRepository, HouseOfferRepository>();
                serviceCollection.AddTransient<ITopSellerOperations, TopSellerOperations>();
                serviceCollection.AddSingleton(new DistributedCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromHours(double.Parse(hostContext.Configuration.GetSection("Cache")["ExpireTimeHours"]))
                });
                serviceCollection.AddStackExchangeRedisCache((options =>
                {
                    options.Configuration = hostContext.Configuration.GetSection("Cache")["Host"];
                    options.InstanceName = hostContext.Configuration.GetSection("Cache")["InstanceName"];
                }));

                Configuration = new ConfigurationBuilder()
                                        .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                                        .AddJsonFile("appsettings.json", false)
                                        .Build();

                serviceCollection.Configure<HouseOfferConfiguration>(Configuration.GetSection("Api"));

            }))
            .ConfigureLogging(((host, log) =>
            {
                Logger logger = new LoggerConfiguration()
                                    .WriteTo.File(host.Configuration.GetSection("Logging")["PathFormat"])
                                    .CreateLogger();

                log.ClearProviders();
                log.AddSerilog(logger);
            }));

        public class Options
        {
            [Option('g', "garden", HelpText = "Set output to top 10 sellers of houses containing garden.", Required = false)]
            public bool WithGarden { get; set; }

            [Option('f', "force", HelpText = "Force fetching and overwriting cache entry.", Required = false)]
            public bool ForceFetching { get; set; }
        }
    }
}
