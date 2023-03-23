using Discord;
using Discord.WebSocket;


internal class RssReader
{
    private readonly ILogger<DiscordBotWorker> _logger;
    private List<RssInfo> _rssList;

    public RssReader(ILogger<DiscordBotWorker> logger)
    {
        _logger = logger;
    }

    public RssReader()
    {
        _rssList = InitializeRssList();
    }

    
    private List<RssInfo> InitializeRssList()
    {
      var rssList = new List<RssInfo>();

      rssList.Add(new RssInfo("https://pox.globo.com/rss/valor", (ulong)ChannelsEnum.HomologaçãoGeral));
      rssList.Add(new RssInfo("https://www.moneytimes.com.br/feed/", (ulong)ChannelsEnum.HomologaçãoGeral));
      rssList.Add(new RssInfo("https://www.infomoney.com.br/feed/", (ulong)ChannelsEnum.HomologaçãoGeral));
      rssList.Add(new RssInfo("https://bmcnews.com.br/feed/", (ulong)ChannelsEnum.HomologaçãoGeral));
      //rssList.Add("BROADCAST");
      rssList.Add(new RssInfo("https://www.agrolink.com.br/rss/noticias.rss", (ulong)ChannelsEnum.HomologaçãoAgro));
      rssList.Add(new RssInfo("https://www.gazetadopovo.com.br/feed/rss/agronegocio.xml", (ulong)ChannelsEnum.HomologaçãoAgro));
      rssList.Add(new RssInfo("https://www.canalrural.com.br/feed/", (ulong)ChannelsEnum.HomologaçãoAgro));
      
      
      return rssList;
    }


    public async Task SendRSSMessage(DiscordSocketClient _client)
    {
      var rssList = InitializeRssList();

        foreach (var rssInfo in rssList)
        {
          var messages = await rssInfo.GetNewMessagesAsync();
          if(messages?.Count>0)
          {
            var channel = _client.GetChannel(rssInfo.Channel) as IMessageChannel;
            foreach (string message in messages)
            {
              await channel.SendMessageAsync(message);
            } 
          }
        }
    }
}