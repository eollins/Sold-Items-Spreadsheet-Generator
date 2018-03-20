using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sold_Items_Spreadsheet_Generator
{
    class SearchResult
    {
        public SearchResult(string Year, decimal SoldPrice, string Player, string Set)
        {
            this.Year = Year;
            this.SoldPrice = SoldPrice;
            this.Player = Player;
            this.Set = Set;
        }

        public string Year { get; set; }
        public decimal SoldPrice { get; set; }
        public string Player { get; set; }
        public string Set { get; set; }
    }
}
