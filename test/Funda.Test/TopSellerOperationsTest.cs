using FluentAssertions;
using Funda.Application;
using Funda.Core.Model;
using Funda.Core.Ports;
using Funda.Infra.Http;
using Funda.Test.Stubs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Funda.Test
{
    public class TopSellerOperationsTest
    {
        private readonly Func<int, int, bool, ITopSellerOperations> _topSellerOperations;
        private readonly HouseOfferRepositoryStub _repository;

        public TopSellerOperationsTest()
        {
            _repository = new HouseOfferRepositoryStub();
            _topSellerOperations = ((pages, resultsPerpage, forceError) =>
                new TopSellerOperations(
                    new HouseOfferProviderStub(pages, resultsPerpage, forceError),
                    _repository,
                    Mock.Of<ILogger<TopSellerOperations>>(),
                    new HttpRetryPolicy(Mock.Of<ILogger<HttpRetryPolicy>>(),
                        Options.Create(
                            new HouseOfferConfiguration()
                            {
                                CummulativeRetryTime = 1,
                                RetryAttempts = 1
                            }
                        )
                    )
               )
           );
        }

        [Theory]
        [InlineData(2, 5, false)]
        [InlineData(2, 5, true)]
        public async Task TopSellerOperations_FetchTopSellers_OnSuccess_ShouldStore_AndCache(
          int pages,
          int resultsPerPage,
          bool withGarden)
        {
            ITopSellerOperations operations = _topSellerOperations(pages, resultsPerPage, false);

            bool completed = false;
            List<FetchResponse> responses = new List<FetchResponse>();
            Action<FetchResponse> onNext = result => responses.Add(result);

            Action onComplete = () => completed = true;

            operations.FetchTopSellers(new CancellationToken(),
                                       withGarden).Subscribe(onNext, onComplete);

            while (!completed) ;
            IEnumerable<TopSellers> topSellers = await operations.GetTopSellers(withGarden);

            responses.Last().FetchingProgress.Should().Be(100);
            responses.Last().Status.Should().Be(FetchStatus.APIRetrieve);
            topSellers.Count().Should().Be(pages);
            _repository.HasCacheEntry(withGarden, 10).Should().BeTrue();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TopSellerOperations_FetchTopSellers_OnSuccess_WhenHasCacheEntry_StatusShouldBeHitCache(
          bool withGarden)
        {
            ITopSellerOperations operations = _topSellerOperations(2, 2, false);

            List<TopSellers> topSellers = new List<TopSellers>();
            topSellers.Add(new TopSellers()
            {
                AdsCount = 2,
                SellerName = "Seller"
            });

            _repository.InsertOrUpdateCacheEntry(withGarden, topSellers);

            bool completed = false;
            List<FetchResponse> responses = new List<FetchResponse>();
            Action<FetchResponse> onNext = result => responses.Add(result);

            Action onComplete = () => completed = true;

            operations.FetchTopSellers(new CancellationToken(),
                                       withGarden).Subscribe(onNext, onComplete);

            while (!completed) ;

            IEnumerable<TopSellers> topSellersFromRepository = await _repository.GetOrCreate(withGarden, 10);
            responses.First().FetchingProgress.Should().Be(100);
            responses.First().Status.Should().Be(FetchStatus.HitCache);
            _repository.HasCacheEntry(withGarden, 10).Should().BeTrue();
            topSellersFromRepository.Should().BeEquivalentTo(topSellers);
        }

        [Fact]
        public void TopSellerOperations_FetchTopSellers_OnError_ShouldContainSkippedInFetchResponseList()
        {
            int pages = 3;
            ITopSellerOperations sellerOperations = _topSellerOperations(pages, 1, true);

            bool completed = false;
            List<FetchResponse> responses = new List<FetchResponse>();
            Action<FetchResponse> onNext = result => responses.Add(result);

            Action onComplete = () => completed = true;

            sellerOperations.FetchTopSellers(new CancellationToken(),true)
                .Subscribe(onNext, onComplete);

            while (!completed) ;
            responses.Count(x => x.Status == FetchStatus.Skipped).Should().Be(pages - 1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TopSellerOperations_GetTopSellers_OnCacheEntryExists_ShoudRetrieveFromCache(
          bool withGarden)
        {
            List<TopSellers> topSellers = new List<TopSellers>();
            topSellers.Add(new TopSellers()
            {
                AdsCount = 2,
                SellerName = "Seller"
            });

            _repository.InsertOrUpdateCacheEntry(withGarden, topSellers);
            _repository.HasCacheEntry(withGarden, 10).Should().BeTrue();

            await _repository.GetOrCreate(withGarden, 10);
            _repository.InsertOrUpdateCacheEntry(withGarden, topSellers);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TopSellerOperations_GetTopSellers_OnCacheEntryNotExists_ShoudRetrieveFromTable_AndCache(
          bool withGarden)
        {
            _repository.HasCacheEntry(withGarden, 10).Should().BeFalse();
            IEnumerable<TopSellers> topSellersFromRepository = await _repository.GetOrCreate(withGarden, 10);
            topSellersFromRepository.Should().BeEmpty();
            _repository.HasCacheEntry(withGarden, 10).Should().BeTrue();
        }
    }
}