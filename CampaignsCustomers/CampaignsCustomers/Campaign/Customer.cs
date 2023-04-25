using CsvHelper.Configuration.Attributes;

namespace CampaignsCustomers.Campaign
{
    public class Customer
    {
        [Name("CUSTOMER_ID")]
        public int Id { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string City { get; set; }
        public int Deposit { get; set; }
        public bool NewCustomer { get; set; }
        [Ignore]
        public bool AlreadyGetAnCampaign { get; set; } = false;
    }
}
