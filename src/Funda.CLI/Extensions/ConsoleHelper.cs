using Funda.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FundaAssignment.Extensions
{
    public static class ConsoleHelper
    {
        public static void WriteProgress(int current) => Console.Write("{0}/{1}%", current, 100);

        public static void WriteTable(IEnumerable<TopSellers> data)
        {
            WriteWithBorders(true, "Position", "Seller", "Ads");
            foreach (var (value, index) in data.Select((v, i) => (v, i)))
            {
                WriteWithBorders(false, index, value.SellerName, value.AdsCount);
            }
        }

        private static void WriteWithBorders(bool topBorder, params object[] parameters)
        {
            if (topBorder)
                Console.WriteLine(string.Join(string.Empty, Enumerable.Range(0, 67).Select(i => "-")));
            Console.WriteLine(string.Format("|{0,8}|{1,50}|{2,5}|", parameters));
            Console.WriteLine(string.Join(string.Empty, Enumerable.Range(0, 67).Select(i => "-")));
        }
    }
}
