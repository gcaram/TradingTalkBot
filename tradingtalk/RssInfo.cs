using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Serialization;

internal class RssInfo
{
  public Uri Uri;
  public DateTime LastUpdate;
  public ulong Channel;

    private readonly ILogger<RssInfo> _logger;

    public RssInfo(ILogger<RssInfo> logger)
    {
        _logger = logger;
    }

    public RssInfo(string uri, ulong channel)
    {
        Uri = new Uri(uri);
        LastUpdate = DateTime.Now;
        Channel = channel;
    }

    public async Task<List<string>?> GetNewMessagesAsync()
    {
      var possibleUpdate = DateTime.Now;
      var check = await CheckNewMessagesAsync();
      if (check.modified)
      {
        var items = GetRssItems(LastUpdate, check.syndicationFeed);
        if(items.Count > 0)
          LastUpdate = possibleUpdate;

          return items;
      }
      return null;
    }

    private async Task<(bool modified, SyndicationFeed? syndicationFeed)> CheckNewMessagesAsync()
    {
      HttpClient myHttpClient = new HttpClient();
      myHttpClient.DefaultRequestHeaders.IfModifiedSince = LastUpdate;
      try
      {
        using HttpResponseMessage response = await myHttpClient.GetAsync(Uri);
        
        if(response.StatusCode != HttpStatusCode.NotModified)
          return(true, SyndicationFeed.Load(XmlReader.Create(response.Content.ReadAsStream())));
        else
          return(false, null);
      }
      catch (Exception e)
      {
        Console.WriteLine($"Unexpected Exception in {Uri}.: {e.Message}");
        return(false, null);
      }  
    }

    private List<string> GetRssItems(DateTimeOffset lastUpdate, SyndicationFeed feed)
    {
      List<string> rssItems = new List<string>();

      try
      {
        foreach (SyndicationItem item in feed.Items)
        {
          if (item.PublishDate >= lastUpdate)
          {
            string itemTitle = item.Title.Text;
            string itemLink = item.Links[0].Uri.ToString();

            rssItems.Add(itemTitle + " - " + itemLink);
            Console.WriteLine($"{Uri} adicionado '{itemTitle}'.");
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