using System;
using AET.Unity.SimplSharp;
using AET.Unity.SimplSharp.HttpClient;
using AET.Unity.SimplSharp.Timer;
using Crestron.SimplSharp;

namespace AET.Unity.RestClient {
  public class RestClient {
    public RestClient() {
      HttpClient = new CrestronHttpClient();
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

    internal void HttpPost(ApiObject command) {
      HttpClient.Post(BuildFullUrl(command.SetUrl), command.GetJson());
    }

    internal void HttpPostOptimized(ApiObject command) {
      var json = command.GetOptimizedJson();
      if (json.IsNullOrWhiteSpace()) {
        if(HttpClient.Debug == 1) CrestronConsole.PrintLine("Unity.RestClient.HttpPostOptimized({0}) OptimizedJson == null.", command.GetUrl);
        return;
      }
      HttpClient.PostAsync(BuildFullUrl(command.SetUrl), json);
    }

    internal void HttpGet(string getUrl, Action<string> callbackAction) {
      HttpClient.GetAsync(BuildFullUrl(getUrl), callbackAction);
    }

    internal string BuildFullUrl(string url) {
      return string.Format("{0}{1}", HostName, url);
    }
  }
}