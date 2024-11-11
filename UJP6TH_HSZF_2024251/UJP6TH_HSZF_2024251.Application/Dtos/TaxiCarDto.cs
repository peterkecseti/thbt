using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UJP6TH_HSZF_2024251.Application.Dto
{
    public class TaxiCarDto
    {
        public string LicensePlate { get; set; }
        public string Driver { get; set; }
        public List<FareDto> Fares { get; set; }
    }
}
