using Bogus;
using Funda.Core.Model;
using Funda.Core.Ports;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Funda.Test.Stubs
{
    public class HouseOfferProviderStub : IHouseOfferProvider
    {
        private readonly int Pages;
        private readonly int ResultsPerPage;
        private readonly bool ForceError;

        public HouseOfferProviderStub(int pages, int resultsPerpage, bool forceError)
        {
            Pages = pages;
            ResultsPerPage = resultsPerpage;
            ForceError = forceError;
        }

        public Task<HouseOfferResponse> GetOffers(
          CancellationToken cancellationToken,
          bool withGarden = false,
          string location = "amsterdam",
          int page = 1)
        {
            if (ForceError && page > 1)
                throw new HttpRequestException();

            List<Seller> pairs = new Faker<Seller>()
                .RuleFor(i => i.Id, f => f.Random.Int())
                .RuleFor(i => i.Name, f => f.Company.CompanyName())
                .Generate(Pages);

            Faker<HouseOffer> faker = new Faker<HouseOffer>()
                .RuleFor(r => r.GlobalId,f => f.Random.Int())
                .RuleFor(r => r.SellerId, f => pairs[page - 1].Id)
                .RuleFor(r => r.SellerName, f => pairs[page - 1].Name);
            
            return Task.FromResult(new HouseOfferResponse()
            {
                Offers = faker.Generate(this.ResultsPerPage),
                Paging = new Paging()
                {
                    CurrentPage = page,
                    TotalPages = Pages
                }
            });
        }
    }

    public class Seller
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
