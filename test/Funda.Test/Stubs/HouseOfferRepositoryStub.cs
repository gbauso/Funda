using Funda.Core.Model;
using Funda.Core.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Funda.Test.Stubs
{
    public class HouseOfferRepositoryStub : IHouseOfferRepository
    {
        private readonly List<HouseOffer> HouseOffers = new List<HouseOffer>();
        private readonly IDictionary<bool, IEnumerable<TopSellers>> Cache = 
            new Dictionary<bool, IEnumerable<TopSellers>>();

        public Task<IEnumerable<TopSellers>> GetOrCreate(
          bool withGarden,
          int take = 10)
        {
            if (HasCacheEntry(withGarden, 10))
                return Task.FromResult(Cache[withGarden]);

            IEnumerable<TopSellers> result = HouseOffers
                .GroupBy(g => new
                {
                    SellerId = g.SellerId,
                    SellerName = g.SellerName
                })
                .OrderByDescending(o => o.Count())
                .Take(take)
                .Select(i => new TopSellers()
                {
                    SellerName = i.Key.SellerName,
                    AdsCount = i.Count()
                }
            );
            
            Cache[withGarden] = result;
            return Task.FromResult(result);
        }

        public bool HasCacheEntry(bool withGarden, int top = 10) => Cache.ContainsKey(withGarden);

        public void InsertOrUpdateCacheEntry(bool withGarden, IEnumerable<TopSellers> topSellers) => 
            Cache[withGarden] = topSellers;

        public Task SaveHouseOffer(IEnumerable<HouseOffer> houseOffer)
        {
            HouseOffers.AddRange(houseOffer);
            return Task.CompletedTask;
        }
    }
}
