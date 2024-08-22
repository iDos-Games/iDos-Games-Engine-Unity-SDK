using System.Collections.Generic;

namespace IDosGames
{
    public class CreateInvoiceRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Payload { get; set; }
        public string ProviderToken { get; set; }
        public string Currency { get; set; }
        public List<LabeledPrice> Prices { get; set; }
    }

    public class LabeledPrice
    {
        public string Label { get; set; }
        public int Amount { get; set; }
    }
}
