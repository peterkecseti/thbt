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
        private static ITaxiService taxiService;
        private static ITaxiRepository taxiRepository;

        private static void HandleFareWarning(object sender)
        {
            AnsiConsole.MarkupLine("[red]Ennek az útnak az ára legalább kétszer több mint az eddigiek közül bármelyik![/]");
        }

        public static void Main(string[] args)
        {
            Init();
            Menu();
        }

        private static void Init()
        {
            System.Console.OutputEncoding = Encoding.UTF8;

            var services = new ServiceCollection();

            services.AddDbContext<TaxiDbContext>(options =>
                options.UseInMemoryDatabase("TaxiDb"));

            services.AddScoped<ITaxiService, TaxiService>();

            IServiceCollection serviceCollection = services.AddScoped<ITaxiRepository, TaxiRepository>();

            var serviceProvider = services.BuildServiceProvider();

            taxiService = serviceProvider.GetRequiredService<ITaxiService>();
            taxiRepository = serviceProvider.GetRequiredService<ITaxiRepository>();
        }

        private static void Menu()
        {
            AnsiConsole.Clear();
            Rule rule = new Rule("[yellow]THBt adminisztráció[/]");
            rule.Justification = Justify.Left;
            AnsiConsole.Write(rule);

            List<Option> MenuOptions = new List<Option>()
        {
            new("Autók beolvasása JSON-ból", taxiService.AddData),
            new("Összes autó kiírása", taxiService.ListAllCars),
            new("Autó felvétele", taxiService.AddCar),
            new("Autó törlése", taxiService.DeleteCar),
            new("Autó módosítása", taxiService.ModifyCar),
            new("Út hozzáadása autóhóz", taxiService.AddFareToCar),
            new("Keresés", taxiService.Filter),
            new("Statisztika generálása", taxiService.GenerateStatistics)
        };

            var menuValue = AnsiConsole.Prompt(
                new SelectionPrompt<Option>()
                    .AddChoices(MenuOptions));

            menuValue.action.Invoke();

            Menu();
        }
    }
}
