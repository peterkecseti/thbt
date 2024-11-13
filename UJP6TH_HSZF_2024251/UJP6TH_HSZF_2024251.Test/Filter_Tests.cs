using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UJP6TH_HSZF_2024251.Application.Repository.UJP6TH_HSZF_2024251.Application;
using UJP6TH_HSZF_2024251.Application.Repository;
using UJP6TH_HSZF_2024251.Application.Services;
using UJP6TH_HSZF_2024251.Model.Entities;

namespace UJP6TH_HSZF_2024251.Test
{
    [TestFixture]
    public class Filter_Tests
    {
        [Test]
        public async Task Filter_Test()
        {
            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();
            var mockedFareService = new Mock<IFareService>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object, mockedFareService.Object);

            var expected = new TaxiCar("DA AA-333", "Bartos Csaba István");

            var expectedFare = new Fare(
                "Budapest, Keleti Pályaudvar",
                "Gyál, Vasútállomás",
                20,
                15000,
                DateTime.Parse("2024-09-05T09:30:00Z"),
                expected);

            expected.Fares.Add(expectedFare);

            var cars = new List<TaxiCar> { expected };
            mockedTaxiRepository.Setup(repo => repo.GetAllCars()).ReturnsAsync(cars);

            List<Func<List<TaxiCar>, List<TaxiCar>>> filterActions = new List<Func<List<TaxiCar>, List<TaxiCar>>>()
                {
                    cars => taxiService.FilterByLicensePlate(cars, "DA AA"),
                    cars => taxiService.FilterByDriver(cars, "Bartos"),
                    cars => taxiService.FilterByPaidAmount(cars, 15000, (actual, filterValue) => actual == filterValue),
                    cars => taxiService.FilterByDistance(cars, 15, (actual, filterValue) => actual > filterValue)
                };

            var result = await taxiService.Filter(filterActions);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(expected));
        }
    }
}
