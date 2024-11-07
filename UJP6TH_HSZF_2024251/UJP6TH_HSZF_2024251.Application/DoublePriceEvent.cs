using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UJP6TH_HSZF_2024251.Application
{
    public class DoublePriceEvent
    {
        public void HandleMyEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Event received in subscriber!");
        }
    }
}
