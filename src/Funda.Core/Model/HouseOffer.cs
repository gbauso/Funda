using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Funda.Core.Model
{
    public class HouseOffer
    {
        public int GlobalId { get; set; }

        [JsonPropertyName("MakelaarId")]
        public int SellerId { get; set; }

        [JsonPropertyName("MakelaarNaam")]
        public string SellerName { get; set; }
    }
}