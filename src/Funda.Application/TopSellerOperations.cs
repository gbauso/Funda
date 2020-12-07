using Funda.Core.Model;
using Funda.Core.Ports;
using Funda.Infra.Http;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Funda.Application
{
    public class TopSellerOperations : ITopSellerOperations
    {
        private readonly IHouseOfferProvider _houseOfferProvider;
        private readonly IHouseOfferRepository _houseOfferRepository;
        private readonly ILogger<TopSellerOperations> _logger;
        private readonly IAsyncPolicy _httpRetryPolicy;

        public TopSellerOperations(
          IHouseOfferProvider houseOfferProvider,
          IHouseOfferRepository houseOfferRepository,
          ILogger<TopSellerOperations> logger,
          HttpRetryPolicy httpRetryPolicy)
        {
            _houseOfferProvider = houseOfferProvider;
            _houseOfferRepository = houseOfferRepository;
            _logger = logger;
            _httpRetryPolicy = httpRetryPolicy.GetRetryPolicy();
        }

        public Task<IEnumerable<TopSellers>> GetTopSellers(
          bool withGarden,
          int take = 10)
        {
            return _houseOfferRepository.GetOrCreate(withGarden, take);
        }

        public IObservable<FetchResponse> FetchTopSellers(
          CancellationToken cancellationToken,
          bool withGarden,
          bool forceFetching = false)
        {
            if (_houseOfferRepository.HasCacheEntry(withGarden) && !forceFetching)
            {
                return Observable.Return(new FetchResponse(100, FetchStatus.HitCache));
            }

            return Observable.Create<FetchResponse>(async observer =>
            {
                await Handle(withGarden, observer, cancellationToken);
                observer.OnCompleted();
                return Disposable.Empty;
            });
        }

        private async Task Handle(
          bool withGarden,
          IObserver<FetchResponse> observer,
          CancellationToken cancellationToken,
          int page = 1,
          int totalPages = 1)
        {
            if (page > totalPages) return;

            _logger.LogInformation("Fetching page {0} of {1}", page, totalPages);
            try
            {
                HouseOfferResponse houseOffer = await _httpRetryPolicy.ExecuteAsync(() => 
                    _houseOfferProvider.GetOffers(cancellationToken, withGarden, page: page)
                );
                await _houseOfferRepository.SaveHouseOffer(houseOffer.Offers);
                FetchResponse response = new FetchResponse((int)houseOffer.Paging.ProgressPercentage(),
                                                           FetchStatus.APIRetrieve);
                observer.OnNext(response);
                await Handle(withGarden, observer, cancellationToken, page + 1, (int)houseOffer.Paging.TotalPages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Skipping page {0} of {1}", page, totalPages);
                FetchResponse response = new FetchResponse(0, FetchStatus.Skipped);
                observer.OnNext(response);
                await Handle(withGarden, observer, cancellationToken, page + 1, totalPages);
            }

        }
    }
}