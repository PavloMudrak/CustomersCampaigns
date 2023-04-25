using CsvHelper;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace CampaignsCustomers.Campaign
{
    public class CampaignSenderService
    {

        private const string CustomersFilePath = "customers.csv";
        private static List<Customer> Customers = new List<Customer>();
        private static List<Campaign> Campaigns = new List<Campaign>();
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public CampaignSenderService()
        {
            //Get all Customers from file
            Customers = GetCustomersFromCsv(CustomersFilePath);

            //Get all hardcoded campaigns
            Campaigns = GetCampaigns();

            //Sort Capaigns by priority
            Campaigns = Campaigns.OrderBy(c => c.Priority).ToList();

            //Fill Campaigns with Customers by condition
            FillCampaignsWithCustomers();

            //create chedule for compaigns
            ScheduleAndSendCampaigns();
        }

        #region Private methods

        private void ScheduleAndSendCampaigns()
        {
            foreach (var campaign in Campaigns)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(campaign.SendAt - DateTime.Now);

                    await SendCampaignAsync(campaign);

                });
            }

        }

        private async Task SendCampaignAsync(Campaign campaign)
        {
            string filePath = Path.Combine("campaigns", $"{campaign.Name}_{DateTime.Now.ToString("HH-mm-ss")}.txt");

            await semaphore.WaitAsync(); 

            try
            {
                var text = $"{campaign.Template}\n" +
                        $"{campaign.Name}, was sent {campaign.SendAt}. Sent to {campaign.Customers.Count} customers";
                await File.WriteAllTextAsync(filePath, text);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private List<Customer> GetCustomersFromCsv(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<Customer>();
            return records.ToList();
        }

        private List<Customer> SortCustomers(List<Customer> customers, string condition)
        {
            string[] parts = condition.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            string fieldName = parts[0];
            string opetator = parts[1];
            string value = parts[2];

            PropertyInfo prop = typeof(Customer).GetProperty(fieldName);

            List<Customer> result = new List<Customer>();

            foreach (Customer customer in customers)
            {
                if (customer.AlreadyGetAnCampaign == true)
                    continue;

                object propValue = prop.GetValue(customer);

                bool match = false;

                switch (opetator)
                {
                    case ">":
                        match = ((IComparable)propValue).CompareTo(int.Parse(value)) > 0;
                        break;
                    case "<":
                        match = ((IComparable)propValue).CompareTo(int.Parse(value)) < 0;
                        break;
                    case ">=":
                        match = ((IComparable)propValue).CompareTo(int.Parse(value)) >= 0;
                        break;
                    case "<=":
                        match = ((IComparable)propValue).CompareTo(int.Parse(value)) <= 0;
                        break;
                    case "==":
                        match = propValue.Equals(value);
                        break;
                    case "!=":
                        match = !propValue.Equals(value);
                        break;
                }

                if (match)
                {
                    customer.AlreadyGetAnCampaign = true;
                    result.Add(customer);
                }
            }

            return result;
        }

        private List<Campaign> GetCampaigns()
        {
            var result = new List<Campaign>
            {
                // Added custom time for Campaigns(almost the same, but more comfortable for testing
                new Campaign
                {
                    Name = "Campaign1",
                    Customers = new List<Customer>(),
                    Priority = 1,
                    SendAt = DateTime.Now.AddSeconds(10),
                    Template = "Template A",
                    Condition= "Gender == Male"
                },
                new Campaign
                {
                    Name = "Campaign2",
                    Customers = new List<Customer>(),
                    Priority = 2,
                    SendAt = DateTime.Now.AddSeconds(1),
                    Template = "Template B",
                    Condition = "Age > 45"
                },
                new Campaign
                {
                    Name = "Campaign3",
                    Customers = new List<Customer>(),
                    SendAt = DateTime.Now.AddSeconds(5),
                    Priority = 5,
                    Template = "Template C",
                    Condition = "City == London"
                },
                new Campaign
                {
                    Name = "Campaign4",
                    Customers = new List<Customer>(),
                    Priority = 3,
                    SendAt = DateTime.Now.AddSeconds(10),
                    Template = "Template A",
                    Condition = "Deposit > 100"
                },
                new Campaign
                {
                    Name = "Campaign5",
                    Customers = new List<Customer>(),
                    Priority = 4,
                    SendAt = DateTime.Now.AddSeconds(1),
                    Template = "Template C",
                    Condition = "NewCustomer == true"
                },
            };
            return result;
        }

        private void FillCampaignsWithCustomers()
        {
            foreach (var campaign in Campaigns)
            {
                var customersForCampaign = SortCustomers(Customers, campaign.Condition);
                campaign.Customers = customersForCampaign;
            }
        }

        #endregion
    }
}
