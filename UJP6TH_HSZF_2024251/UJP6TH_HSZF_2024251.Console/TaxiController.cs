using Newtonsoft.Json;
using Spectre.Console;
using UJP6TH_HSZF_2024251.Application.Services;
using UJP6TH_HSZF_2024251.Model.Entities;

namespace UJP6TH_HSZF_2024251.Application
{
    public class TaxiController
    {
        private readonly ITaxiService taxiService;
        private readonly IFareService fareService;

        public TaxiController(ITaxiService taxiService, IFareService fareService)
        {
            this.taxiService = taxiService;
            this.fareService = fareService;

            // event subscription
            Fare.HighPaidAmountDetected += OnHighPaidAmountDetected;
        }

        private static void OnHighPaidAmountDetected(Fare fare)
        {
            AnsiConsole.MarkupLine($"[Red]Az új út fizetett összege [/][grey]({fare.PaidAmount})[/][red] több mint kétszerese a korábbi maximum összegnek![/]");
        }

        public async Task RunAsync()
        {
            AnsiConsole.Clear();
            Rule rule = new Rule("[yellow]THBt adminisztráció[/]");
            rule.Justification = Justify.Left;
            AnsiConsole.Write(rule);

            List<Option> MenuOptions = new List<Option>()
            {
                new("\U0001F4C1 Autók beolvasása JSON-ból", AddData),
                new("\U0001F697 Összes autó kiírása", ListAllCars),
                new("\u2795 Autó felvétele", AddCar),
                new("\u2796 Autó törlése", DeleteCar),
                new("\U0001F58A  Autó módosítása", UpdateCar),
                new("\U0001F4CC Út hozzáadása autóhóz", AddFareToCar),
                new("\U0001F50D Keresés", async () => {Filter();}),
                new("\U0001F4CA Statisztika generálása", GenerateStatistics)
            };

            var menuValue = AnsiConsole.Prompt(
                new SelectionPrompt<Option>()
                    .AddChoices(MenuOptions));

            await menuValue.ActionAsync();

            await RunAsync();
        }

        public async Task AddData()
        {
            string path = Browser.GetPath(BrowserType.File, "json");
            try
            {
                await taxiService.AddData(path);
                AnsiConsole.MarkupLine("[green]Beolvasás sikeres![/]");
                
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[/]\n[red]{ex.Message}[/]");
            }

            System.Console.ReadKey();
            AnsiConsole.Clear();
        }
        public async Task ListAllCars() => await PrintCarsTree(taxiService.GetAllCars().Result);

        public string ReadPrompt(string prompt) => AnsiConsole.Prompt(new TextPrompt<string>(prompt));
        public async Task PrintCarsTree(List<TaxiCar> cars)
        {
            if (cars.Count == 0)
            {
                AnsiConsole.WriteLine("A keresésnek nem volt eredménye.");
                System.Console.ReadKey();
                AnsiConsole.Clear();
                return;
            }

            var root = new Tree("[bold yellow]Autók:[/]");

            foreach (var car in cars)
            {
                var carNode = root.AddNode($"[bold green]Rendszám:[/] {car.LicensePlate}")
                    .AddNode($"[bold green]Sofőr:[/] {car.Driver}")
                    .AddNode("[bold green]Utak:[/]");

                int index = 1;
                foreach (var fare in car.Fares)
                {
                    var fareNode = carNode.AddNode($"[bold]Út {index++}:[/]");

                    fareNode.AddNode($"[blue]Indulás helye:[/] {fare.From}");
                    fareNode.AddNode($"[blue]Érkezés helye:[/] {fare.To}");
                    fareNode.AddNode($"[blue]Megtett táv:[/] {fare.Distance} km");
                    fareNode.AddNode($"[blue]Fizetett:[/] {fare.PaidAmount} Ft");
                    fareNode.AddNode($"[blue]Indulás ideje:[/] {fare.FareStartDate}");
                }
            }

            AnsiConsole.Write(root);
            System.Console.ReadLine();
            AnsiConsole.Clear();
        }

        public async Task AddCar()
        {
            try
            {
                // License plate input
                string licensePlate = AnsiConsole.Prompt(
                    new TextPrompt<string>("Autó rendszáma: "));

                // Driver input
                string driver = AnsiConsole.Prompt(
                    new TextPrompt<string>("Autó sofőrje: "));

                await taxiService.AddCar(licensePlate, driver);

                // Display the king if demanded
                if (driver == "gaspar laci")
                {
                    var image = new CanvasImage("UJP6TH_HSZF_2024251.MostImportantAsset.png");
                    image.MaxWidth = 64;
                    AnsiConsole.Write(image);
                }

                AnsiConsole.MarkupLine("[green]Autó sikeresen hozzáadva![/]");
                System.Console.ReadKey();
            }
            catch(LicensePlateException ex)
            {
                AnsiConsole.MarkupLine(ex.Message.ToString());
            }
        }

        public async Task DeleteCar()
        {
            AnsiConsole.Clear();
            var cars = await taxiService.GetAllCars();

            AnsiConsole.WriteLine("Melyik autót szeretné törölni?");
            var carSelect = AnsiConsole.Prompt(
                new SelectionPrompt<TaxiCar>()
                    .AddChoices(cars));

            await taxiService.DeleteCar(carSelect);
        }

        public async Task UpdateCar()
        {
            try
            {
                var cars = await taxiService.GetAllCars();
                var toUpdate = AnsiConsole.Prompt(
                    new SelectionPrompt<TaxiCar>()
                        .Title("Melyik autó adatait szeretné változtatni?")
                        .AddChoices(cars));

                List<Option> modifyOptions = new List<Option>
                {
                    new("Rendszám", async () =>
                    {
                        string newLicensePlate = ReadPrompt("Új rendszám: ");
                
                        await taxiService.UpdateCar(toUpdate, newLicensePlate);
                    }),
                    new("Sofőr", async () =>
                    {
                        string newDriver = ReadPrompt("Új sofőr neve: ");
                        toUpdate.Driver = newDriver;

                        await taxiService.UpdateCar(toUpdate);
                    })
                };

                var changeOptions = AnsiConsole.Prompt(
                    new MultiSelectionPrompt<Option>()
                        .Title($"Mely adatokat szeretné változtatni? ({toUpdate})")
                        .AddChoices(modifyOptions));

                foreach (var option in changeOptions)
                {
                    await option.ActionAsync();
                }

                AnsiConsole.MarkupLine("[green]Adat(ok) sikeresen megváltoztatva! Autó új adatai:[/]");
                await PrintCarsTree(new List<TaxiCar> { toUpdate });
            }
            catch (LicensePlateException ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            }
        }

        public async Task AddFareToCar()
        {
            AnsiConsole.Clear();
            var cars = await taxiService.GetAllCars();

            if (cars.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Nincsenek felvett autók![/]");
                System.Console.ReadLine();
                return;
            }

            var selectedCar = AnsiConsole.Prompt(
                new SelectionPrompt<TaxiCar>()
                    .AddChoices(cars));

            string from = AnsiConsole.Prompt(
                new TextPrompt<string>("Indulás helye: "));

            string to = AnsiConsole.Prompt(
                new TextPrompt<string>("Érkezés helye: "));

            int distance = AnsiConsole.Prompt(
                new TextPrompt<int>("Megtett távolság: "));

            int paidAmount = AnsiConsole.Prompt(
                new TextPrompt<int>("Fizetett összeg: "));

            await taxiService.AddFareToCar(from, to, distance, paidAmount, selectedCar);

            AnsiConsole.MarkupLine("[green]Út felvétele sikeres![/]");
            System.Console.ReadKey();
        }

        public async Task Filter(List<TaxiCar> toFilter = null)
        {
            

            // List to store desired filtering methods
            List<Func<List<TaxiCar>, List<TaxiCar>>> filterActions = new List<Func<List<TaxiCar>, List<TaxiCar>>>();

            // Menu options for selecting int operator on numeric filtering
            List<SearchOptions> filterOptions = new List<SearchOptions>
            {
                new("kisebb, mint",     () => 0),
                new("nagyobb, mint",    () => 1),
                new("egyenlő",          () => 2)
            };

            // Returns int operator based on number value
            var comparisonFuncs = new Dictionary<int, Func<int, int, bool>>
            {
                { 0, (actual, filterValue) => actual < filterValue },
                { 1, (actual, filterValue) => actual > filterValue },
                { 2, (actual, filterValue) => actual == filterValue }
            };

            // List that stores every filtering option
            List<Option> menuOptions = new List<Option>
            {
                new("Rendszám", async () =>
                {
                    string filterValue = ReadPrompt("Rendszám: ");

                    // Add filter to the action pool
                    filterActions.Add(cars => taxiService.FilterByLicensePlate(cars, filterValue));
                }),
                new("Sofőr", async () =>
                {
                    string filterValue = ReadPrompt("Sofőr: ");

                    // Add filter to the action pool
                    filterActions.Add(cars => taxiService.FilterByDriver(cars, filterValue));
                }),
                new("Fizetett összeg", async () =>
                {
                    string title = "Fizetett összeg";
                    SearchOptions filterMethod = AnsiConsole.Prompt( // Prompt the user to select filtering method
                        new SelectionPrompt<SearchOptions>()
                            .Title(title)
                            .AddChoices(filterOptions));

                    int filterValue = int.Parse(ReadPrompt($"{title} {filterOptions[filterMethod.func.Invoke()].name}:"));

                     // Get the int operator for the filtering
                    var paidAmountComparison = comparisonFuncs[filterMethod.func.Invoke()];

                    // Add filter to the action pool
                    filterActions.Add(cars => taxiService.FilterByPaidAmount(cars, filterValue, paidAmountComparison));
                }),
                new("Megtett távolság", async () =>
                {
                    string title = "Megtett távolság";
                    SearchOptions filterMethod = AnsiConsole.Prompt( // Prompt the user to select filtering method
                        new SelectionPrompt<SearchOptions>()
                            .Title(title)
                            .AddChoices(filterOptions));

                    int filterValue = int.Parse(ReadPrompt($"{title} {filterOptions[filterMethod.func.Invoke()].name}:"));

                    // Get the int operator for the filtering
                    var distanceComparison = comparisonFuncs[filterMethod.func.Invoke()]; 

                    // Add filter to the action pool
                    filterActions.Add(cars => taxiService.FilterByDistance(cars, filterValue, distanceComparison));
                })
            };

            // Prompt the user to select which filters to apply
            var selectedOptions = AnsiConsole.Prompt(
                new MultiSelectionPrompt<Option>()
                    .Title("Mire szeretne rákeresni?")
                    .InstructionsText("[blue]<space> [/][grey]- kiválasztás,\n[/][blue]<enter>[/][grey] - továbblépés[/]")
                    .AddChoices(menuOptions));

            // Execute each selected filtering criteria to collect input values
            foreach (var option in selectedOptions)
            {
                await option.ActionAsync();
            }

            // Execute the filtering in the service
            var result = await taxiService.Filter(filterActions, toFilter);

            // Print the result in tree format
            await PrintCarsTree(result);

            // Menu options to determine whether or not to continue filtering
            List<SearchOptions> continueFilteringOptions = new List<SearchOptions>
            {
                new("Igen",   () => 1),
                new("Nem",    () => 0)
            };

            var answer = AnsiConsole.Prompt(
                new SelectionPrompt<SearchOptions>()
                    .Title("Szeretné tovább szűrni a keresés eredményét?")
                    .AddChoices(continueFilteringOptions));

            // Recall the filter function with the previously gotten results
            if (answer.func.Invoke() == 1) await Filter(result);
        }

        public async Task GenerateStatistics()
        {

            string path = Browser.GetPath(BrowserType.Folder);

            var statistics = taxiService.GenerateStatistics();
            var json = JsonConvert.SerializeObject(statistics, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            File.WriteAllText(path + "\\statistics.json", json);
            AnsiConsole.MarkupLine("[green]Statisztika generálva![/]");
            System.Console.ReadKey();
        }
    }
}
