using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UJP6TH_HSZF_2024251.Application
{
    public class TaxiCarStatistics
    {
        public string LicensePlate { get; set; }
        public int ShortTripsCount { get; set; }
        public DistanceStats DistanceStatistics { get; set; }
        public FrequentDestination MostFrequentDestination { get; set; }

        public TaxiCarStatistics(string licensePlate, int shortTripsCount, DistanceStats distanceStatistics, FrequentDestination mostFrequentDestination)
        {
            LicensePlate = licensePlate;
            ShortTripsCount = shortTripsCount;
            DistanceStatistics = distanceStatistics;
            MostFrequentDestination = mostFrequentDestination;
        }
    }

    public class DistanceStats
    {
        public double AverageDistance { get; set; }
        public TripDetails ShortestTrip { get; set; }
        public TripDetails LongestTrip { get; set; }

        public DistanceStats(double averageDistance, TripDetails shortestTrip, TripDetails longestTrip)
        {
            AverageDistance = averageDistance;
            ShortestTrip = shortestTrip;
            LongestTrip = longestTrip;
        }
    }

    public class FrequentDestination
    {
        public string Destination { get; set; }
        public int Count { get; set; }

        public FrequentDestination(string destination, int count)
        {
            Destination = destination;
            Count = count;
        }
    }

    public class TripDetails
    {
        public string From { get; set; }
        public string To { get; set; }
        public double Distance { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime FareStartDate { get; set; }

        public TripDetails(string from, string to, double distance, decimal paidAmount, DateTime fareStartDate)
        {
            From = from;
            To = to;
            Distance = distance;
            PaidAmount = paidAmount;
            FareStartDate = fareStartDate;
        }
    }
}
