using CampaignsCustomers.Campaign;

var builder = WebApplication.CreateBuilder(args);
var campaignService = new CampaignSenderService();
var app = builder.Build();
app.MapGet("/", () => "Hello World!");

app.Run();
