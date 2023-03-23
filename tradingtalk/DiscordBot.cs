using Discord;
using Discord.WebSocket;


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
        await PrepareBot();
        await RunBot();

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

    private async Task PrepareBot()
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
    }

    private async Task RunBot()
    {
      var _rssReader = new RssReader();
      
      while (true)
      {
        await _rssReader.SendRSSMessage(_client);
        await Task.Delay(30000);
      }
    }
}