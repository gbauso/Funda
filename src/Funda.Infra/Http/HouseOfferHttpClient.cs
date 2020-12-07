using Funda.Core.Ports;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Funda.Infra.Http
{
    public class HouseOfferHttpClient : IHouseOfferProvider
    {
        private readonly Func<HttpClient> _httpClient;
        private readonly HouseOfferConfiguration _configuration;

        public HouseOfferHttpClient(
          IHttpClientFactory httpClientFactory,
          IOptions<HouseOfferConfiguration> configuration)
        {
            _httpClient = () => httpClientFactory.CreateClient("HouseOffer");
            _configuration = configuration.Value;
        }

        public async Task<HouseOfferResponse> GetOffers(
          CancellationToken cancellationToken,
          bool withGarden = false,
          string location = "amsterdam",
          int page = 1)
        {
            using (HttpClient client = _httpClient())
            {
                string uri = GetUri(withGarden, location, page);
                client.BaseAddress = new Uri(_configuration.BaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                HttpResponseMessage httpResponseMessage = await client.GetAsync(uri, cancellationToken);

                httpResponseMessage.EnsureSuccessStatusCode();

                return await GetHttpResponseMessageContent(httpResponseMessage);
            }
        }

        private async Task<HouseOfferResponse> GetHttpResponseMessageContent(
          HttpResponseMessage httpResponseMessage)
        {
            string content = await httpResponseMessage.Content.ReadAsStringAsync();
            HouseOfferResponse houseOfferResponse = JsonSerializer.Deserialize<HouseOfferResponse>(content);
            return houseOfferResponse;
        }

        private string GetUri(bool withGarden, string location, int page = 1) => 
            string.Format("feeds/Aanbod.svc/json/{0}/?type=koop&zo=/{1}/{2}&page={3}&pagesize={4}",
                          _configuration.Key,
                          location,
                          withGarden ? "tuin" : string.Empty,
                          page,
                          _configuration.ResultSize);
    }
}
