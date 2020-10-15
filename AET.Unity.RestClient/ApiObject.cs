using AET.Unity.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace AET.Unity.RestClient {

  public abstract class ApiObject {
    private readonly string setUrl, getUrl;
    protected JObject lastSent = null;

    protected ApiObject(string setUrl, string getUrl) {
      Json = new JObject();
      this.setUrl = setUrl;
      this.getUrl = getUrl;
    }

    public RestClient RestClient { get; set; }

    public string GetUrl {
      get { return getUrl; }
    }

    public string SetUrl {
      get { return setUrl; }
    }

    protected JObject Json { get; set; }

    #region Json Object Helpers
    protected int GetInt(string key) {
      JToken value;
      if (!Json.TryGetValue(key, out value)) return 0;
      if (value == null) return 0;
      return value.Value<int>();
    }

    protected bool GetBool(string key) {
      JToken value;
      if (!Json.TryGetValue(key, out value)) return false;
      if (value == null) return false;
      return value.Value<bool>();
    }

    protected double GetDouble(string key) {
      JToken value;
      if (!Json.TryGetValue(key, out value)) return 0;
      if (value == null) return 0;
      return value.Value<double>();
    }

    internal virtual string GetJson() {
      return GetJson(Json);
    }

    private string GetJson(JObject o) {
      if (!o.HasValues) return null;
      var json = o.ToString(Formatting.None);
      return json;

    }

    internal virtual string GetOptimizedJson() {
      JObject o = null;
      try {
        if (lastSent == null) {
          lastSent = JObject.FromObject(Json);
          return GetJson();
        }

        o = Diff(lastSent, Json);
        lastSent = JObject.FromObject(Json);
        return GetJson(o);
      }
      catch(Exception ex) {
        ErrorMessage.Error("Unity.RestClient.ApiObject.GetOptimizedJson: {0}", ex);
        return null;
      }
    }

    protected JObject Deserialize(string json) {
      try {
        return JObject.Parse(json);
      } catch (Exception ex) {
        ErrorMessage.Error("AET.Unity.RestClient: Unable to deserialize: ({0})\r\n{1}", ex.Message, json);
        return null;
      }
    }

    protected JObject Diff(JObject jOld, JObject jNew) {
      var o = new JObject();
      foreach (var prop in jNew.Properties()) {
        if (prop.Value is JObject) {
          var o2 = Diff(jOld[prop.Name] as JObject, prop.Value as JObject);
          if (o2.HasValues) o.Add(prop.Name, o2);
        } else {
          var v1 = prop.Value;
          var v2 = jOld[prop.Name];
          if (!v1.Equals(v2)) {
            o.Add(prop);
          }
        }
      }
      return o;
    }
    #endregion

    public virtual void Send() {
      if (RequiredFieldsAreValid()) RestClient.HttpPostOptimized(this);
    }

    public void SendAll() {
      if (RequiredFieldsAreValid()) RestClient.HttpPost(this);
    }

    public abstract bool RequiredFieldsAreValid();
    public abstract void Poll();

    protected void FillJson() {
      Action callback = null;
      FillJson(callback);
    }
    protected void FillJson(Action callback) {
      try {
        var response = RestClient.HttpGet(GetUrl);        
        var json = Deserialize(response);
        if (json == null) return;
        Json = json;
        if (callback != null) callback();
        lastSent = JObject.FromObject(Json);        
      }
      catch (Exception ex) {
        ErrorMessage.Error("Unity.RestClient.FillJson(): {0}", ex.Message);
      }
    }

    protected JObject GetJObject() {
      try {
        var response = RestClient.HttpGet(GetUrl);
        var json = Deserialize(response);
        if (json == null) return null ;
        return json;
      } catch (Exception ex) {
        ErrorMessage.Error("Unity.RestClient.GetJObject(): {0}", ex.Message);
        return null;
      }      
    }

    protected void FillJson(string jsonPath) {
      try {
        var responseString = RestClient.HttpGet(GetUrl);
        var j = JObject.Parse(responseString);
        Json = j[jsonPath] as JObject;
        lastSent = JObject.FromObject(Json);
      }
      catch (Exception ex) {
        ErrorMessage.Error("Unity.RestClient.FillJson({0}): {1}", jsonPath, ex.Message);
      }
    }

    protected void FillJsonFromPost(string postContents, Action callback) {
      try {
        var response = RestClient.HttpPost(GetUrl, postContents);
        var json = Deserialize(response);
        if (json == null) return;
        Json = json;
        lastSent = JObject.FromObject(Json);
      } catch (Exception ex) {
        ErrorMessage.Error("Unity.RestClient.FillJson(): {0}", ex.Message);
      }
    }
    



    #region Helper Methods
    protected ushort ConvertTo16Bit(int value, int scale) {
      return (ushort)(value * 65535 / scale);
    }

    protected ushort ConvertTo16Bit(long? value, int scale) {
      return ConvertTo16Bit((int)(value ?? 0), scale);
    }

    protected ushort ConvertFrom16Bit(ushort value, int scale) {
      return (ushort)((value * scale) / 65535);
    }

    public static bool FalseWithErrorMessage(string message) {
      ErrorMessage.Error(message);
      return false;
    }

    public static bool FalseWithErrorMessage(string message, params object[] args) {
      ErrorMessage.Error(message, args);
      return false;
    }

    protected string Clean(string value) {
      if (value == null) return null;
      if (value == "") return null;
      return value.ToLower();
    }

    protected bool ValueIsValid<T>(T value, string name, T[] allowedValues) {
      if (value == null) return true;
      if (allowedValues.Contains(value)) return true;
      return FalseWithErrorMessage("{0} must be {1}.", name, allowedValues.FormatAsList());
    }

    protected bool ValueIsValid(ushort? value, string name, ushort minValue, ushort maxValue) {
      if (value == null) return true;
      if (value >= minValue && value <= maxValue) return true;
      return FalseWithErrorMessage("{0} must be {1} to {2}.", name, minValue, maxValue);
    }

    #endregion
  }
}
