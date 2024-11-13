using UJP6TH_HSZF_2024251.Application.Repository;
using UJP6TH_HSZF_2024251.Model.Entities;

namespace UJP6TH_HSZF_2024251.Application.Services
{
    public interface IFareService
    {
        Task<List<Fare>> GetAllFares();
        void CheckForHighPaidAmount(Fare fare, IEnumerable<Fare> allFares);
        event Action<Fare> HighPaidAmountDetected;
    }
    public class FareService : IFareService
    {
        public IFareRepository fareRepository;
        public FareService(IFareRepository fareRepository)
        {
            this.fareRepository = fareRepository;
        }

        public async Task<List<Fare>> GetAllFares() => await fareRepository.GetAllFares();

        // event
        public event Action<Fare> HighPaidAmountDetected;
        public void CheckForHighPaidAmount(Fare newFare, IEnumerable<Fare> allFares)
        {
            int maxPaidAmount = allFares.Any() ? allFares.Max(f => f.PaidAmount) : 0;

            if (newFare.PaidAmount > maxPaidAmount * 2)
            {
                HighPaidAmountDetected?.Invoke(newFare);
            }
        }

    }
}
