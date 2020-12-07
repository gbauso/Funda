using Dapper;
using Funda.Core.Model;
using Funda.Core.Ports;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Funda.Infra.Repository
{
    public class HouseOfferRepository : IHouseOfferRepository
    {
        private readonly IDistributedCache _distributedCache;
        private readonly DistributedCacheEntryOptions _cacheOptions;

        public HouseOfferRepository(
          IDistributedCache distributedCache,
          DistributedCacheEntryOptions cacheEntryOptions)
        {
            _distributedCache = distributedCache;
            _cacheOptions = cacheEntryOptions;
            CreateDatabase();
        }

        private static string DbFile => 
            Environment.CurrentDirectory + "\\HouseOffer.sqlite";

        private static SQLiteConnection SimpleDbConnection() => 
            new SQLiteConnection($"Data Source={DbFile}");

        public async Task<IEnumerable<TopSellers>> GetOrCreate(
          bool withGarden,
          int take = 10)
        {
            string cacheKey = GetCacheKey(withGarden, take);
            if (_distributedCache.ContainsKey(cacheKey))
                return JsonSerializer.Deserialize<IEnumerable<TopSellers>>(_distributedCache.Get(cacheKey));

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                IEnumerable<TopSellers> topSellers =
                    await cnn.QueryAsync<TopSellers>(
                        $@"SELECT SellerName, COUNT(1) as AdsCount
                          FROM HouseOffer
                          GROUP BY SellerId, SellerName
                          ORDER BY adsCount DESC
                          LIMIT 0, {take}");

                await _distributedCache.SetStringAsync(cacheKey,
                                                       JsonSerializer.Serialize(topSellers),
                                                       _cacheOptions);

                return topSellers;
            }
        }

        public async Task SaveHouseOffer(IEnumerable<HouseOffer> houseOffer)
        {
            using (SQLiteConnection cnn = SimpleDbConnection())
            {
                cnn.Open();
                await cnn.ExecuteAsync(@"INSERT INTO HouseOffer( GlobalId, SellerName, SellerId ) 
                                         VALUES (@GlobalId, @SellerName, @SellerId)",
                                         houseOffer);
            }
        }

        private static void CreateDatabase()
        {
            if (File.Exists(DbFile))
                File.Delete(DbFile);

            using (SQLiteConnection cnn = SimpleDbConnection())
            {
                cnn.Open();
                cnn.Execute(@"CREATE TABLE HouseOffer
                            (
                                GlobalId        integer not null,
                                SellerId        integer not null,
                                SellerName      varchar(100) not null
                            )"
                );
            }
        }

        public bool HasCacheEntry(bool withGarden, int top = 10) => 
            _distributedCache.ContainsKey(GetCacheKey(withGarden, top));

        private string GetCacheKey(bool withGarden, int top = 10) => 
            string.Format("TopSellerWithAdvertising::{0}::{1}", withGarden ? "Garden" : "Simple", top);
    }
}