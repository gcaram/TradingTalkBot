using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

internal class DiscordBotWorker: BackgroundService
{
    private IConfiguration _configuration;
    private readonly ILogger<DiscordBotWorker> _logger;
    private DiscordSocketClient _client;
    private bool _ready = false;
    string _token;

    public DiscordBotWorker(ILogger<DiscordBotWorker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client = new DiscordSocketClient();
        _token = _configuration["discordBotClientSecret"];

        _client.Log += Log;
        _client.Ready += OnReady;

        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();

        while (!_ready)
        {
            await Task.Delay(1000);
        }
        
        var channel = _client.GetChannel(1085969769343230044) as IMessageChannel;
        await SendRSSMessage(channel);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("DiscordBotWorker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1_000, stoppingToken);
        }
    }

     private Task Log(LogMessage msg)
    {
      Console.WriteLine(msg.ToString());
      return Task.CompletedTask;
    }

    private async Task OnReady()
    {
      Console.WriteLine("Bot está online e pronto para enviar mensagens!");
      _ready = true;
    }

    private List<string> InitializeRssList()
    {
      List<string> rssList = new List<string>();

      rssList.Add("https://pox.globo.com/rss/valor");
      rssList.Add("https://www.moneytimes.com.br/feed/");
      rssList.Add("https://www.infomoney.com.br/feed/");
      rssList.Add("https://bmcnews.com.br/feed/");
      //rssList.Add("BROADCAST");

      return rssList;
    }


    private async Task SendRSSMessage(IMessageChannel channel)
    {
      var lastUpdate = DateTime.Now;
      var possibleUpdate = DateTime.Now;
      var rssList = InitializeRssList();

      while (true)
      {
        possibleUpdate = DateTime.Now;
        Console.WriteLine($"Atualizando {possibleUpdate}");
        foreach (var rssUrl in rssList)
        {
          if (await CheckActivityAsync(rssUrl, lastUpdate))
          {
            List<string> rssItems = GetRssItems(rssUrl, lastUpdate);

            foreach (string rssItem in rssItems)
            {
              await channel.SendMessageAsync(rssItem);
              lastUpdate = possibleUpdate;
            }
          }
        }

        await Task.Delay(30000);
      }
    }

    private async Task<bool> CheckActivityAsync(string rssUrl, DateTime lastUpdate)
    {      
      HttpClient myHttpClient = new HttpClient();
      myHttpClient.DefaultRequestHeaders.IfModifiedSince = lastUpdate;
      
      try
      {
        using HttpResponseMessage response = await myHttpClient.GetAsync(rssUrl);
        return (response.StatusCode != HttpStatusCode.NotModified);
      }
      catch (Exception e)
      {
        Console.WriteLine("Unexpected Exception " + e.Message);
        return true;
      }   
    }

    private List<string> GetRssItems(string rssUrl, DateTimeOffset lastUpdate)
    {
      List<string> rssItems = new List<string>();

      try
      {
        XmlReader reader = XmlReader.Create(rssUrl);
        SyndicationFeed feed = SyndicationFeed.Load(reader);

        foreach (SyndicationItem item in feed.Items)
        {
          if (item.PublishDate >= lastUpdate)
          {
            string itemTitle = item.Title.Text;
            string itemLink = item.Links[0].Uri.ToString();

            rssItems.Add(itemTitle + " - " + itemLink);
            Console.WriteLine($"{rssUrl} adicionado '{itemTitle}'.");
          }
        }
        return rssItems;
      }
      catch (Exception e)
      {
        Console.WriteLine("Unexpected Exception " + e.Message);
        return rssItems;
      }
    }
}