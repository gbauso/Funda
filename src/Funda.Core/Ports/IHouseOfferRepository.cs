using Funda.Core.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Funda.Core.Ports
{
    public interface IHouseOfferRepository
    {
        bool HasCacheEntry(bool withGarden, int top = 10);

        Task SaveHouseOffer(IEnumerable<HouseOffer> houseOffer);

        Task<IEnumerable<TopSellers>> GetOrCreate(bool withGarden, int take = 10);
    }
}