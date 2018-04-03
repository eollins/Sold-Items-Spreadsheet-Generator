using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sold_Items_Spreadsheet_Generator
{
    class SearchResult
    {
        //public SearchResult(string Year, string Brand, int CardNumber, string Player, string PSAGrade, string Seller, string Price)
        //{
        //    this.Year = Year;
        //    this.Brand = Brand;
        //    this.CardNumber = CardNumber;
        //    this.Player = Player;
        //    this.PSAGrade = PSAGrade;
        //    this.Seller = Seller;
        //    this.Price = Price;
        //}

        public string Year { get; set; }
        public string Brand { get; set; }
        public string CardNumber { get; set; }
        public string Player { get; set; }
        public string PSAGrade { get; set; }
        public string Seller { get; set; }
        public string Price { get; set; }
    }
}
