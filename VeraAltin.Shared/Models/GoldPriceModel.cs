using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VeraAltin.Shared.Models
{
    public class GoldPriceModel
    {
        public string Metal { get; set; }
        public string Currency { get; set; }
        public double Price { get; set; }
        public long TimeStamp { get; set; }
    }
}