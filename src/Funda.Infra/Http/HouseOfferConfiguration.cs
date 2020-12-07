namespace Funda.Infra.Http
{
    public class HouseOfferConfiguration
    {
        public string BaseUrl { get; set; }

        public string Key { get; set; }

        public int ResultSize { get; set; }

        public int RetryAttempts { get; set; }

        public int CummulativeRetryTime { get; set; }
    }
}
