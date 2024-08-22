using System.Collections.Generic;

namespace IDosGames
{
    public class ItemToGrant
    {
        public string ItemClass { get; set; }
        public string Type { get; set; }
        public string CurrencyID { get; set; }
        public string Catalog { get; set; }
        public string ItemID { get; set; }
        public string Amount { get; set; }
        public string ImagePath { get; set; }
    }

    public class Product
    {
        public string Name { get; set; }
        public string ItemID { get; set; }
        public string ProductType { get; set; }
        public string ItemClass { get; set; }
        public string PriceRM { get; set; }
        public string ImagePath { get; set; }
        public List<ItemToGrant> ItemsToGrant { get; set; }
    }
}
