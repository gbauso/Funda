using System.Text.Json.Serialization;

namespace Funda.Core.Ports
{
    public class Paging
    {
        [JsonPropertyName("AantalPaginas")]
        public double TotalPages { get; set; }

        [JsonPropertyName("HuidigePagina")]
        public double CurrentPage { get; set; }

        public double ProgressPercentage() => this.CurrentPage / this.TotalPages * 100.0;
    }
}