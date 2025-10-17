using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTO.Reponse
{
    public class GHTKFeeResponse
    {
        public bool Success { get; set; }
        public FeeDetail Fee { get; set; }
        public string Message { get; set; }
    }

    public class FeeDetail
    {
        public string Name { get; set; }
        public double Fee { get; set; }
        public double InsuranceFee { get; set; }
        public bool Delivery { get; set; }
        public double ShipFeeOnly { get; set; }
        public FeeOptions Options { get; set; }
    }

    public class FeeOptions
    {
        public double ShipMoney { get; set; }
        public string ShipMoneyText { get; set; }
    }

}
