﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace UJP6TH_HSZF_2024251.Model.Entities
{
    [Table("Fares")]
    public class Fare
    {
        public Fare(string from, string to, int distance, int paidAmount, DateTime fareStartDate, TaxiCar taxiCar)
        {
            From = from;
            To = to;
            Distance = distance;
            PaidAmount = paidAmount;
            FareStartDate = fareStartDate;
            TaxiCar = taxiCar;
        }

        public Fare() { }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid FareID { get; set; }
        public Guid TaxiID { get; set; }
        [StringLength(100)]
        public string From { get; set; }
        [StringLength(100)]
        public string To { get; set; }
        public int PaidAmount { get; set; }
        public int Distance { get; set; }
        public DateTime FareStartDate { get; set; }
        public virtual TaxiCar TaxiCar { get; set; }
        public event Action<Fare> HighPaidAmountDetected;

        // Overrides to help tests complete
        public override bool Equals(object obj)
        {
            if (obj is not Fare other)
                return false;

            return FareID == other.FareID &&
                   From == other.From &&
                   To == other.To &&
                   PaidAmount == other.PaidAmount &&
                   Distance == other.Distance &&
                   FareStartDate == other.FareStartDate &&
                   (TaxiCar?.LicensePlate == other.TaxiCar?.LicensePlate);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(
                FareID,
                From,
                To,
                PaidAmount,
                Distance,
                FareStartDate,
                TaxiCar?.LicensePlate
            );
        }
    }
}
