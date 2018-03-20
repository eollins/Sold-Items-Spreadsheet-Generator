namespace Sold_Items_Spreadsheet_Generator
{
    class SpreadsheetItem
    {
        public SpreadsheetItem(string Year, string Set, string CardNumber, string Quality, string Player, int Quantity, string HighPrice, string AvgPrice, string LowPrice, string Sport, string Index)
        {
            this.Year = Year;
            this.Set = Set;
            this.CardNumber = CardNumber;
            this.Quality = Quality;
            this.Player = Player;
            this.Quantity = Quantity;
            this.HighPrice = HighPrice;
            this.AvgPrice = AvgPrice;
            this.LowPrice = LowPrice;
            this.Sport = Sport;
            this.Index = Index;
        }

        public string Year { get; set; }
        public string Set { get; set; }
        public string CardNumber { get; set; }
        public string Quality { get; set; }
        public string Player { get; set; }
        public int Quantity { get; set; }
        public string HighPrice { get; set; }
        public string AvgPrice { get; set; }
        public string LowPrice { get; set; }
        public string Sport { get; set; }
        public string Index { get; set; }
    }
}