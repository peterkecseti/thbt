using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Spectre.Console;
using UJP6TH_HSZF_2024251.Application.Dto;
using UJP6TH_HSZF_2024251.Application.Repository;
using UJP6TH_HSZF_2024251.Application.Repository.UJP6TH_HSZF_2024251.Application;
using UJP6TH_HSZF_2024251.Model;


namespace UJP6TH_HSZF_2024251.Application
{
    public interface ITaxiService
    {
        void AddData();
        void ListAllCars();
        void PrintCarsTree(List<TaxiCar> cars);
        void AddCar();
        bool LicensePlateExists(string licensePlate);
        void DeleteCar();
        void ModifyCar();
        void AddFareToCar();
        void Filter();
        string ReadPrompt(string prompt);
        void GenerateStatistics();
    }
    public class TaxiService : ITaxiService
    {
        public ITaxiRepository context;
        public TaxiService(ITaxiRepository context) {
            this.context = context;
        }

        // event
        public delegate void FareWarningEventHandler(object sender);
        public event FareWarningEventHandler FareWarning;
        public void AddData()
        {
            string path = ReadPrompt("Fájl elérési útvonala: ");
            AnsiConsole.Clear();
            try
            {
                var json = File.ReadAllText("A:\\progi\\szfa\\taxi.json");
                //var json = File.ReadAllText(path);
                var taxiData = JsonConvert.DeserializeObject<RootTaxiDto>(json);

                foreach (var taxiCarData in taxiData.TaxiCars)
                {
                    var existingTaxiCar = context.GetAllCars()
                        .Where(tc => tc.LicensePlate == taxiCarData.LicensePlate).FirstOrDefault();

                    TaxiCar taxiCar;
                    if (existingTaxiCar != null)
                    {
                        taxiCar = existingTaxiCar;
                    }
                    else
                    {
                        taxiCar = new TaxiCar(
                            taxiCarData.LicensePlate,
                            taxiCarData.Driver);
                        context.Add(taxiCar);
                    }

                    foreach (var fareData in taxiCarData.Fares)
                    {
                        var fare = new Fare(
                            fareData.From,
                            fareData.To,
                            fareData.Distance,
                            fareData.PaidAmount,
                            fareData.FareStartDate,
                            taxiCar);

                        taxiCar.Fares.Add(fare);
                    }
                }

                AnsiConsole.MarkupLine("[green]Beolvasás sikeres![/]");
                Console.ReadKey();
                AnsiConsole.Clear();

            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Hiba történt:[/]\n[grey]{ex.Message}[/]");
                Console.ReadKey();
                AnsiConsole.Clear();
            }
        }

        public void ListAllCars()
        {
            var cars = context.GetAllCars();

            PrintCarsTree(cars);
        }

        public void PrintCarsTree(List<TaxiCar> cars)
        {
            if (cars.Count == 0)
            {
                AnsiConsole.WriteLine("A keresésnek nem volt eredménye.");
                Console.ReadKey();
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
            Console.ReadKey();
            AnsiConsole.Clear();
        }

        public void AddCar()
        {
            // License plate input
            string licensePlate = AnsiConsole.Prompt(
                new TextPrompt<string>("Autó rendszáma: ")
                    .Validate((lp) =>
                    {
                        return !LicensePlateExists(lp)
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]Már létezik ilyen rendszámú autó![/]");
                    }
                 ));

            // Driver input
            string driver = AnsiConsole.Prompt(
                new TextPrompt<string>("Autó sofőrje: "));

            // Display the king if demanded
            if(driver == "gaspar laci")
            {
                var image = new CanvasImage("UJP6TH_HSZF_2024251.MostImportantAsset.png");
                image.MaxWidth = 64;
                AnsiConsole.Write(image);
            }

            // Save to database
            TaxiCar newCar = new TaxiCar(licensePlate, driver);
            context.Add(newCar);

            AnsiConsole.MarkupLine("[green]Autó sikeresen hozzáadva![/]");
            Console.ReadKey();
        }

        
        public bool LicensePlateExists(string licensePlate)
        {
            var taxiCar = context.GetAllCars()
                .FirstOrDefault(tc => tc.LicensePlate == licensePlate);

            if (taxiCar == null) return false;
            else return taxiCar.LicensePlate == licensePlate;
        }

        public void DeleteCar()
        {
            AnsiConsole.Clear();
            var cars = context.GetAllCars();

            AnsiConsole.WriteLine("Melyik autót szeretné törölni?");
            var carSelect = AnsiConsole.Prompt(
                new SelectionPrompt<TaxiCar>()
                    .AddChoices(cars));

            context.Remove(carSelect);
        }
        public void ModifyCar()
        {
            var cars = context.GetAllCars();
            var selectCar = AnsiConsole.Prompt(
                new SelectionPrompt<TaxiCar>()
                    .Title("Melyik autó adatait szeretné változtatni?")
                    .AddChoices(cars));

            List<Option> modifyOptions = new List<Option>
            {
                new("Rendszám", () =>
                {
                    string licensePlate = AnsiConsole.Prompt(
                        new TextPrompt<string>("Autó rendszáma: ")
                            .Validate((lp) =>
                            {
                                return !LicensePlateExists(lp)
                                ? ValidationResult.Success()
                                : ValidationResult.Error("[red]Már létezik ilyen rendszámú autó![/]");
                            }
                    ));
                    selectCar.LicensePlate = licensePlate;
                    context.UpdateCar(selectCar);
                }),
                new("Sofőr", () =>
                {
                    string newDriver = ReadPrompt("Új sofőr neve: ");
                    selectCar.Driver = newDriver;
                    context.UpdateCar(selectCar);
                })
            };

            var changeOptions = AnsiConsole.Prompt(
                new MultiSelectionPrompt<Option>()
                    .Title($"Mely adatokat szeretné változtatni? ({selectCar})")
                    .AddChoices(modifyOptions));

            foreach(var option in changeOptions)
            {
                option.action();
            }

            AnsiConsole.MarkupLine("[green]Adat(ok) sikeresen megváltoztatva! Autó új adatai:[/]");
            PrintCarsTree(new List<TaxiCar> { selectCar });
        }

        public void AddFareToCar()
        {
            AnsiConsole.Clear();
            var cars = context.GetAllCars();

            if(cars.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Nincsenek felvett autók![/]");
                Console.ReadLine();
                return;
            }

            var carSelect = AnsiConsole.Prompt(
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

            DateTime now = DateTime.Now;
            Fare fare = new Fare(from, to, distance, paidAmount, now, carSelect);

            AddFare(fare, carSelect);
            context.Add(fare);

            AnsiConsole.MarkupLine("[green]Út felvétele sikeres![/]");
            Console.ReadKey();
        }

        public void AddFare(Fare newFare, TaxiCar car)
        {
            if (car.Fares.Any(f => f.PaidAmount < newFare.PaidAmount * 2))
            {
                FareWarning?.Invoke(this);
            }

            car.Fares.Add(newFare);
        }

        public void Filter()
        {
            var cars = context.GetAllCars();

            var asd = cars.Where(l => l.Fares.Any(f => f.From == "asd"));

            List<SearchOption> filterOptions = new List<SearchOption>
                    {
                        new("Kisebb, mint",     () => {return 0; }),
                        new("Nagyobb, mint",    () => {return 1; }),
                        new("Egyenlő",          () => {return 2; })
                    };

            List<Option> menuOptions = new List<Option>() {
                new("Rendszám", () =>
                {
                    string filterValue = ReadPrompt("Rendszám: ");
                    var filteredCars = cars.Where(l => l.LicensePlate.Contains(filterValue)).ToList();
                    cars = filteredCars;
                }),
                new("Sofőr", () =>
                {
                    string filterValue = ReadPrompt("Sofőr: ");
                    var filteredCars = cars.Where(l => l.Driver.Contains(filterValue)).ToList();
                    cars = filteredCars;
                }),
                new("Indulás helye", () =>
                {
                    string filterValue = ReadPrompt("Indulás helye: ");
                    var filteredCars = cars.Where(l => l.Fares.Any(f => f.From.Contains(filterValue)))
                                           .Select(car => new TaxiCar(
                                               car.Driver,
                                               car.LicensePlate,
                                               car.Fares.Where(f => f.From.Contains(filterValue)).ToList()))
                                           .Where(car => car.Fares.Any()).ToList();
                    cars = filteredCars;
                }),
                new("Érkezés helye", () =>
                {
                    string filterValue = ReadPrompt("Érkezés helye: ");
                    var filteredCars = cars.Where(l => l.Fares.Any(f => f.To.Contains(filterValue)))
                                           .Select(car => new TaxiCar(
                                               car.Driver,
                                               car.LicensePlate,
                                               car.Fares.Where(f => f.To.Contains(filterValue)).ToList()))
                                           .Where(car => car.Fares.Any()).ToList();
                    cars = filteredCars;
                }),
                new("Megtett távolság", () =>
                {
                    SearchOption filterMethod = AnsiConsole.Prompt(
                        new SelectionPrompt<SearchOption>()
                        .Title("Keresés módja: ")
                        .AddChoices(filterOptions));

                    if(filterMethod.func.Invoke() == 0)
                    {
                        var filterValue = int.Parse(ReadPrompt("Megtett távolság kisebb mint: "));
                        var filteredCars = cars.Where(car => car.Fares.Any(fare => fare.Distance < filterValue)).Select(
                            car => new TaxiCar(
                                car.LicensePlate,
                                car.Driver,
                                car.Fares.Where(f => f.Distance < filterValue).ToList()))
                        .Where(car => car.Fares.Any()).ToList();
                        cars = filteredCars;
                    }
                    else if(filterMethod.func.Invoke() == 1)
                    {
                        var filterValue = int.Parse(ReadPrompt("Megtett távolság nagyobb mint: "));
                        var filteredCars = cars.Where(car => car.Fares.Any(fare => fare.Distance > filterValue)).Select(
                            car => new TaxiCar(
                                car.LicensePlate,
                                car.Driver,
                                car.Fares.Where(f => f.Distance > filterValue).ToList()))
                        .Where(car => car.Fares.Any()).ToList();
                        cars = filteredCars;
                    }
                    else
                    {
                        var filterValue = int.Parse(ReadPrompt("Megtett távolság egyenlő: "));
                        var filteredCars = cars.Where(car => car.Fares.Any(fare => fare.Distance == filterValue)).Select(
                            car => new TaxiCar(
                                car.LicensePlate,
                                car.Driver,
                                car.Fares.Where(f => f.Distance == filterValue).ToList()))
                        .Where(car => car.Fares.Any()).ToList();
                        cars = filteredCars;
                    }
                }),
                new("Fizetett összeg", () =>
                {
                    SearchOption filterMethod = AnsiConsole.Prompt(
                        new SelectionPrompt<SearchOption>()
                        .Title("Keresés módja: ")
                        .AddChoices(filterOptions));

                    if(filterMethod.func.Invoke() == 0)
                    {
                        var filterValue = int.Parse(ReadPrompt("Fizetett összeg kisebb mint: "));
                        var filteredCars = cars.Where(car => car.Fares.Any(fare => fare.PaidAmount < filterValue)).Select(
                            car => new TaxiCar(
                                car.LicensePlate,
                                car.Driver,
                                car.Fares.Where(f => f.PaidAmount < filterValue).ToList()))
                        .Where(car => car.Fares.Any()).ToList();
                        cars = filteredCars;
                    }
                    else if(filterMethod.func.Invoke() == 1)
                    {
                        var filterValue = int.Parse(ReadPrompt("Fizetett összeg nagyobb mint: "));
                        var filteredCars = cars.Where(car => car.Fares.Any(fare => fare.PaidAmount > filterValue)).Select(
                            car => new TaxiCar(
                                car.LicensePlate,
                                car.Driver,
                                car.Fares.Where(f => f.PaidAmount > filterValue).ToList()))
                        .Where(car => car.Fares.Any()).ToList();
                        cars = filteredCars;
                    }
                    else
                    {
                        var filterValue = int.Parse(ReadPrompt("Fizetett összeg egyenlő: "));
                        var filteredCars = cars.Where(car => car.Fares.Any(fare => fare.PaidAmount == filterValue)).Select(
                            car => new TaxiCar(
                                car.LicensePlate,
                                car.Driver,
                                car.Fares.Where(f => f.PaidAmount == filterValue).ToList()))
                        .Where(car => car.Fares.Any()).ToList();
                        cars = filteredCars;
                    }
                })};

            var filters = AnsiConsole.Prompt(
                            new MultiSelectionPrompt<Option>()
                                .Title("Mire szeretne rákeresni?")
                                .InstructionsText("[blue]<space> [/][grey]- kiválasztás,\n[/][blue]<enter>[/][grey] - továbblépés[/]")
                                .AddChoices(menuOptions));

            foreach(var filter in filters)
            {
                filter.action();
            }

            PrintCarsTree(cars);
        }

        public string ReadPrompt(string prompt)
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>(prompt));
        }


        public void GenerateStatistics()
        {
            var cars = context.GetAllCars();
            var statistics = cars
              .Select(car => new {
                  car.LicensePlate,
                  ShortTripsCount = car.Fares.Count(f => f.Distance < 10),
                  DistanceStatistics = new
                  {
                      AverageDistance = car.Fares.Any() ? car.Fares.Average(f => f.Distance) : 0,
                      ShortestTrip = car.Fares.OrderBy(f => f.Distance)
                        .Select(f => new {f.From, f.To, f.Distance, f.PaidAmount, f.FareStartDate})
                        .FirstOrDefault(),
                      LongestTrip = car.Fares.OrderByDescending(fare => fare.Distance)
                        .Select(f => new { f.From, f.To, f.Distance, f.PaidAmount, f.FareStartDate })
                        .FirstOrDefault()
                  },
                  MostFrequentDestination = car.Fares
                  .GroupBy(f => f.To)
                  .OrderByDescending(g => g.Count())
                  .Select(g => new {
                      Destination = g.Key,
                      Count = g.Count()
                  })
                  .FirstOrDefault()
              })
              .ToList();

            var json = JsonConvert.SerializeObject(statistics, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            File.WriteAllText("A:\\progi\\szfa\\statistics.json", json);
            AnsiConsole.MarkupLine("[green]Statisztika generálva![/]");
            Console.ReadKey();


        }
    }
}
