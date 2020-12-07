using Funda.Core.Model;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Funda.Core.Ports
{
    public class HouseOfferResponse
    {
        [JsonPropertyName("Objects")]
        public IEnumerable<HouseOffer> Offers { get; set; }

        public Paging Paging { get; set; }
    }
}