using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System;
using System.Net.Http;

namespace Funda.Infra.Http
{
    public class HttpRetryPolicy
    {
        private readonly ILogger<HttpRetryPolicy> _logger;
        private readonly HouseOfferConfiguration _configuration;

        public HttpRetryPolicy(
          ILogger<HttpRetryPolicy> logger,
          IOptions<HouseOfferConfiguration> options)
        {
            _logger = logger;
            _configuration = options.Value;
        }

        public AsyncRetryPolicy GetRetryPolicy() => 
            Policy.Handle<HttpRequestException>()
                .WaitAndRetryAsync(
                    _configuration.RetryAttempts,
                    retryAttempt =>
                    {
                        _logger.LogInformation("Retrying call attempt {0} of {1}",
                                               retryAttempt,
                                               _configuration.RetryAttempts);
                        return TimeSpan.FromSeconds((retryAttempt * _configuration.CummulativeRetryTime));
                    }
            );
    }
}