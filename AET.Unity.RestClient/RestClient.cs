using System;
using AET.Unity.SimplSharp;
using AET.Unity.SimplSharp.HttpClient;
using AET.Unity.SimplSharp.Timer;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Ssh;

namespace AET.Unity.RestClient {
  public class RestClient {
    public RestClient() {
      HttpClient = new CrestronHttpClient(5);
    }

    public RestClient(IHttpClient httpClient) {
      HttpClient = httpClient;
    }

    #region Properties for Test Double injection
    public IHttpClient HttpClient { get; set; }
    #endregion

    private string hostName;
    public string HostName {
      get { return hostName; }
      set {
        hostName = value.ToLower().TrimEnd('/');
        if (!hostName.StartsWith("http")) hostName = "http://" + hostName;
      }
    }

    public string HttpPost(ApiObject command) {
      return HttpClient.Post(BuildFullUrl(command.SetUrl), command.GetJson());
    }

    public void HttpPostOptimized(ApiObject command) {
      var json = command.GetOptimizedJson();
      if (json.IsNullOrWhiteSpace()) {
        if(HttpClient.Debug == 1) CrestronConsole.PrintLine("Unity.RestClient.HttpPostOptimized({0}) OptimizedJson == null.", command.GetUrl);
        return;
      }
      HttpClient.Post(BuildFullUrl(command.SetUrl), json);
    }

    public string HttpPost(string path, string contents) {
      var url = BuildFullUrl(path);
      return HttpClient.Post(url, contents);
    }

    public string HttpGet(string url) {
      return HttpClient.Get(BuildFullUrl(url));
    }

    internal string BuildFullUrl(string url) {
      return string.Format("{0}{1}", HostName, url);
    }
  }
}