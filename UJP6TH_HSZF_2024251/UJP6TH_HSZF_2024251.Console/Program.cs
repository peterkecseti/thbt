using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using UJP6TH_HSZF_2024251.Application;
using UJP6TH_HSZF_2024251.Application.Repository;
using UJP6TH_HSZF_2024251.Application.Repository.UJP6TH_HSZF_2024251.Application;
using UJP6TH_HSZF_2024251.Model;
using UJP6TH_HSZF_2024251.Persistence.MsSql;


namespace UJP6TH_HSZF_2024251.Console
{
    public class Program
    {
        private static ServiceProvider serviceProvider;

        public static async Task Main(string[] args)
        {
            Init();

            // Resolve the TaxiController and run the main async loop
            var taxiController = serviceProvider.GetRequiredService<TaxiController>();
            await taxiController.RunAsync();
        }

        private static void Init()
        {
            System.Console.OutputEncoding = Encoding.UTF8;

            var services = new ServiceCollection();

            services.AddDbContext<TaxiDbContext>(options =>
                options
                    .UseInMemoryDatabase("TaxiDb")
                    .UseLazyLoadingProxies());

            // Register services and repositories
            services.AddScoped<ITaxiService, TaxiService>();
            services.AddScoped<IFareService, FareService>(); // Add IFareService and its implementation
            services.AddScoped<ITaxiRepository, TaxiRepository>();
            services.AddScoped<IFareRepository, FareRepository>();

            // Register the controller
            services.AddScoped<TaxiController>();

            // Build the service provider
            serviceProvider = services.BuildServiceProvider();
        }
    }
}
