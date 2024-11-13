using Moq;
using NUnit.Framework;
using System.Net.WebSockets;
using UJP6TH_HSZF_2024251.Application;
using UJP6TH_HSZF_2024251.Application.Repository;
using UJP6TH_HSZF_2024251.Application.Repository.UJP6TH_HSZF_2024251.Application;
using UJP6TH_HSZF_2024251.Application.Services;
using UJP6TH_HSZF_2024251.Model.Entities;

namespace UJP6TH_HSZF_2024251.Test
{
    [TestFixture]
    public class AddCar_Tests
    {
        [Test]
        public async Task AddData_DoesNotAddNewCarIfAlreadyExists()
        {
            var jsonData = @"
            {
            ""TaxiCars"": [
                    {
                    ""LicensePlate"": ""DA AA-333"",
                    ""Driver"": ""Bartos Csaba István"",
                    ""Fares"": [
                            {
                            ""From"": ""Budapest, Népliget"",
                            ""To"": ""Dabas, buszállomás"",
                            ""Distance"": 50,
                            ""PaidAmount"": 22000,
                            ""FareStartDate"": ""2024-08-15T00:00:00Z""
                            }
                        ]
                    }
                ]
            }";

            File.WriteAllText("test.json", jsonData);

            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();
            var mockedFareService = new Mock<IFareService>();

            var existingTaxiCar = new TaxiCar("DA AA-333", "Bartos Csaba István");
            mockedTaxiRepository
                .Setup(ctx => ctx.GetExistingCar("DA AA-333"))
                .ReturnsAsync(existingTaxiCar);

            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object, mockedFareService.Object);

            await taxiService.AddData("test.json");

            mockedTaxiRepository.Verify(ctx => ctx.Add(It.IsAny<TaxiCar>()), Times.Never);
            mockedTaxiRepository.Verify(ctx => ctx.Add(It.Is<Fare>(f =>
                f.From == "Budapest, Népliget" &&
                f.To == "Dabas, buszállomás" &&
                f.Distance == 50 &&
                f.PaidAmount == 22000)),
                Times.Once);
        }

        [Test]
        public async Task AddCar_WhenLicensePlateDoesNotExistAddsNewCar()
        {

            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();
            var mockedFareService = new Mock<IFareService>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object, mockedFareService.Object);

            string licensePlate = "DA AA-333";
            string driver = "Bartos Csaba István";

            mockedTaxiRepository
                .Setup(ctx => ctx.GetExistingCar(licensePlate))
                .ReturnsAsync((TaxiCar)null);

            await taxiService.AddCar(licensePlate, driver);

            mockedTaxiRepository.Verify(ctx => ctx.Add(It.Is<TaxiCar>(car =>
                car.LicensePlate == licensePlate &&
                car.Driver == driver)),
                Times.Once);
        }
        [Test]
        public async Task AddCar_WhenLicensePlateExistsThrowsLicensePlateException()
        {

            var mockedTaxiRepository = new Mock<ITaxiRepository>();
            var mockedFareRepository = new Mock<IFareRepository>();
            var mockedFareService = new Mock<IFareService>();
            var taxiService = new TaxiService(mockedTaxiRepository.Object, mockedFareRepository.Object, mockedFareService.Object);


            string licensePlate = "DA AA-333";
            string driver = "Bartos Csaba István";

            var existingTaxiCar = new TaxiCar(licensePlate, driver);
            mockedTaxiRepository
                .Setup(ctx => ctx.GetExistingCar(licensePlate))
                .ReturnsAsync(existingTaxiCar);

            Assert.ThrowsAsync<LicensePlateException>(() => taxiService.AddCar(licensePlate, driver));

            mockedTaxiRepository.Verify(ctx => ctx.Add(It.IsAny<TaxiCar>()), Times.Never);
        }

        
        

        
    }
}
