using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UJP6TH_HSZF_2024251.Application.Dto
{
    public class FareDto
    {
        public string From { get; set; }
        public string To { get; set; }
        public int Distance { get; set; }
        public int PaidAmount { get; set; }
        public DateTime FareStartDate { get; set; }
    }
}
