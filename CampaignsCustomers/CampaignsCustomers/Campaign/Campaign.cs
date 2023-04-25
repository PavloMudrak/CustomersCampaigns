namespace CampaignsCustomers.Campaign
{
    public class Campaign
    {
        public string Name { get; set; }
        public string Template { get; set; }
        public List<Customer> Customers { get; set; }
        public DateTime SendAt { get; set; }
        public int Priority { get; set; }
        public string Condition { get; set; }

    }
}
