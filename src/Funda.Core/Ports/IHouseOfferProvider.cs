using System.Threading;
using System.Threading.Tasks;

namespace Funda.Core.Ports
{
    public interface IHouseOfferProvider
    {
        Task<HouseOfferResponse> GetOffers(
          CancellationToken cancellationToken,
          bool withGarden = false,
          string location = "amsterdam",
          int page = 1);
    }
}
