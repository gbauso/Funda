using Funda.Core.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Funda.Application
{
    public interface ITopSellerOperations
    {
        IObservable<FetchResponse> FetchTopSellers(
          CancellationToken cancellationToken,
          bool withGarden,
          bool forceFetching = false);

        Task<IEnumerable<TopSellers>> GetTopSellers(bool withGarden, int take = 10);
    }
}
