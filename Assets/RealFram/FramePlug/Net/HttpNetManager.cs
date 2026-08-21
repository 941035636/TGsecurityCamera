using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;


public class HttpCallBackArgs : EventArgs
{
  

    public bool HasError;//是否有错
    public string ErrorValue;//错误信息
    public string Value;//值
    public byte[] Data;//数据
    public long ResponseCode;//HTTP 状态码

    public void Init()
    {
        HasError = false;
        ErrorValue = string.Empty;
        Value = string.Empty;
        Data = null;
        ResponseCode = 0;
    }

}


[Serializable]
public class HttpResponse
{
    public string msg = string.Empty;
    public int code;
    public data data;




}
[Serializable]
public class data
{
    public string auth = string.Empty;

}



public delegate void HttpSendDataCallBack(HttpCallBackArgs callBackArgs);
public class HttpNetManager
{
    private readonly HashSet<UnityWebRequest> activeRequests = new HashSet<UnityWebRequest>();
    private readonly Dictionary<UnityWebRequest, object> requestOwners = new Dictionary<UnityWebRequest, object>();
    private readonly HashSet<UnityWebRequest> suppressedCallbacks = new HashSet<UnityWebRequest>();
    private readonly HashSet<string> activeRequestKeys = new HashSet<string>();
    private int callbackGeneration;
    public static string AuthorizationToken { get; private set; }

    public static void SetAuthorizationToken(string token)
    {
        AuthorizationToken = token ?? string.Empty;
    }

    public static void ClearAuthorizationToken()
    {
        AuthorizationToken = string.Empty;
    }

    public void CancelAllRequests()
    {
        callbackGeneration++;
        foreach (UnityWebRequest request in activeRequests)
        {
            if (request != null)
            {
                request.Abort();
            }
        }
        activeRequestKeys.Clear();
    }

    public void CancelRequestsFor(object owner)
    {
        if (owner == null)
        {
            return;
        }
        List<UnityWebRequest> requestsToCancel = new List<UnityWebRequest>();
        foreach (KeyValuePair<UnityWebRequest, object> entry in requestOwners)
        {
            if (ReferenceEquals(entry.Value, owner))
            {
                requestsToCancel.Add(entry.Key);
            }
        }
        for (int i = 0; i < requestsToCancel.Count; i++)
        {
            UnityWebRequest request = requestsToCancel[i];
            suppressedCallbacks.Add(request);
            request.Abort();
        }
    }

    //本地服务器
    //public static readonly string LocalIP = "192.168.199.150:7711";
    //测试服务器IP
    //public static readonly string TestIP = "192.168.1.237:7711";
    ////服务器IP

    //public static readonly string IP = "192.168.110.2:7711";
    //public static readonly string MqttIP = "192.168.110.2";

    //public static readonly string IP = "192.168.20.220:7711";
    //public static readonly string MqttIP = "192.168.20.220";



    private static HttpNetManager instance;
    public static HttpNetManager GetInstance() 
    {
        if (instance==null)
        {
            instance = new HttpNetManager();
        }
        return instance;
    }
    /// <summary>
    /// Http请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="sendCallBack">收到消息后触发回调</param>
    /// <param name="isPost">是否采用Post请求</param>
    /// <param name="isPostBody">True采用Post body体传输，false采用表单传输</param>
    /// <param name="isGetData"></param>
    /// <param name="data">Post传输数据</param>
    /// 
    public void SendDataInt(string url, HttpSendDataCallBack sendCallBack, bool isPost = false, bool isPostBody = false, bool isGetData = false, int data=0)
    {
        if (isPost)
        {
            if (isPostBody)
                PosturlBody(url, data.ToString(), sendCallBack, isGetData); //Post  Body体传输
            else
                PosturlFormint(url, data, sendCallBack, isGetData); //Post  表单传输

        }
        else
        {
            GetUrl(url, sendCallBack, isGetData);
        }
        if (!isGetData)
        {
             Log.Debug("<color=#ffa200>发送消息:</color><color=#FFFB80>" + url + "</color>");
        }
    }

    public void SendDataStr(string url, HttpSendDataCallBack sendCallBack, bool isPost = false,bool isPostBody=false, bool isGetData = false, string data = null)
    {
        if (isPost)
        {
            if (isPostBody)
                PosturlBody(url, data, sendCallBack, isGetData); //Post  Body体传输
            else
            {
                PosturlForm(url, data, sendCallBack, isGetData); //Post  表单传输
            }
              

        }
        else
        {
            GetUrl(url, sendCallBack, isGetData);
        }
        if (!isGetData)
        {
             Log.Debug("<color=#ffa200>发送消息:</color><color=#FFFB80>" + url + "</color>");
        }
    }

    public void SendDataObj(string url, HttpSendDataCallBack sendCallBack, bool isPost = false, bool isGetData = false, object data = null)
    {
        string json = string.Empty;
        if (isPost)
        {
            json = JsonConvert.SerializeObject(data, Formatting.Indented);

            PosturlBody(url, json, sendCallBack, isGetData);

        }
        else
        {
            GetUrl(url, sendCallBack, isGetData);
        }
        if (!isGetData)
        {
             Log.Debug("<color=#ffa200>发送消息:</color><color=#FFFB80>" + url + "</color>");
     
        }
    }
    
    
    
    /// <summary>
    /// Get请求
    /// </summary>
    /// <param name="url"></param>
    private void GetUrl(string url, HttpSendDataCallBack sendCallBack, bool isGetData)
    {
        string normalizedUrl = AppRuntimeConfig.NormalizeApiUrl(url);
        string requestKey = BuildRequestKey("GET", normalizedUrl, null, sendCallBack);
        if (!activeRequestKeys.Add(requestKey))
        {
            Log.Debug("忽略重复请求: " + normalizedUrl);
            return;
        }
        UnityWebRequest webRequest = UnityWebRequest.Get(normalizedUrl);
        PrepareRequest(webRequest);
        GameStart.Instance.StartCoroutine(Request(webRequest, sendCallBack, isGetData, requestKey));
    }

    private static string BuildRequestKey(string method, string url, string payload, HttpSendDataCallBack callback)
    {
        int payloadHash = payload == null ? 0 : payload.GetHashCode();
        if (callback == null)
        {
            return method + "|" + url + "|" + payloadHash;
        }
        object target = callback.Target;
        int targetId = target == null ? 0 : RuntimeHelpers.GetHashCode(target);
        return method + "|" + url + "|" + payloadHash + "|" + callback.Method.MetadataToken + "|" + targetId;
    }
    /// <summary>
    /// Post   Form
    /// </summary>
    /// <param name="url"></param>
    /// <param name="json"></param>
    private void PosturlForm(string url, string json, HttpSendDataCallBack sendCallBack, bool isGetData)
    {
        string normalizedUrl = AppRuntimeConfig.NormalizeApiUrl(url);
        string requestKey = BuildRequestKey("POST_FORM", normalizedUrl, json, sendCallBack);
        if (!activeRequestKeys.Add(requestKey))
        {
            Log.Debug("忽略重复请求: " + normalizedUrl);
            return;
        }
        WWWForm form = new WWWForm();
        form.AddField("json", json);
        UnityWebRequest webRequest = UnityWebRequest.Post(normalizedUrl, form);
        webRequest.SetRequestHeader("Content-Type", "application/json");
        PrepareRequest(webRequest);
        GameStart.Instance.StartCoroutine(Request(webRequest, sendCallBack, isGetData, requestKey));


    }
    private void PosturlFormint(string url, int data, HttpSendDataCallBack sendCallBack, bool isGetData)
    {
        string normalizedUrl = AppRuntimeConfig.NormalizeApiUrl(url);
        string requestKey = BuildRequestKey("POST_FORM", normalizedUrl, data.ToString(), sendCallBack);
        if (!activeRequestKeys.Add(requestKey))
        {
            Log.Debug("忽略重复请求: " + normalizedUrl);
            return;
        }
        WWWForm form = new WWWForm();
        form.AddField("json", data);
        UnityWebRequest webRequest = UnityWebRequest.Post(normalizedUrl, form);
        webRequest.SetRequestHeader("Content-Type", "application/json");
        PrepareRequest(webRequest);
        GameStart.Instance.StartCoroutine(Request(webRequest, sendCallBack, isGetData, requestKey));

    }
    /// <summary>
    /// Post  Body
    /// </summary>
    /// <param name="url"></param>
    /// <param name="json"></param>
    private void PosturlBody(string url, string json, HttpSendDataCallBack sendCallBack, bool isGetData)
    {
        string normalizedUrl = AppRuntimeConfig.NormalizeApiUrl(url);
        string requestKey = BuildRequestKey("POST_JSON", normalizedUrl, json, sendCallBack);
        if (!activeRequestKeys.Add(requestKey))
        {
            Log.Debug("忽略重复请求: " + normalizedUrl);
            return;
        }
        GameStart.Instance.StartCoroutine(RequestByJsonBodyPost(normalizedUrl, json, sendCallBack, isGetData, requestKey));
    }
    private IEnumerator RequestByJsonBodyPost(string url, string json, HttpSendDataCallBack sendCallBack, bool isGetData, string requestKey)
    {
        UnityWebRequest www = new UnityWebRequest(AppRuntimeConfig.NormalizeApiUrl(url),UnityWebRequest.kHttpVerbPOST);
        DownloadHandler downloadHandler = new DownloadHandlerBuffer();
        www.downloadHandler = downloadHandler;
        www.SetRequestHeader("Content-Type","application/json;charset=utf-8");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        PrepareRequest(www);
        yield return Request(www, sendCallBack, isGetData, requestKey);

    }

    public static void PrepareRequest(UnityWebRequest webRequest)
    {
        webRequest.timeout = 20;
        AppRuntimeSettings config = AppRuntimeConfig.Settings;
        if (config.sendAuthorizationHeader && !string.IsNullOrEmpty(config.authorizationHeader) &&
            !string.IsNullOrEmpty(AuthorizationToken))
        {
            string prefix = string.IsNullOrEmpty(config.authorizationScheme)
                ? string.Empty
                : config.authorizationScheme.Trim() + " ";
            webRequest.SetRequestHeader(config.authorizationHeader.Trim(), prefix + AuthorizationToken);
        }
    }

    private IEnumerator Request(UnityWebRequest webRequest, HttpSendDataCallBack sendCallBack, bool isGetData, string requestKey)
    {
        int requestGeneration = callbackGeneration;
        activeRequests.Add(webRequest);
        requestOwners[webRequest] = sendCallBack == null ? null : sendCallBack.Target;
        yield return webRequest.SendWebRequest();
        HttpCallBackArgs callBackArgs = new HttpCallBackArgs();
        callBackArgs.Init();
        callBackArgs.ResponseCode = webRequest.responseCode;
        if (webRequest.isHttpError || webRequest.isNetworkError)
        {
            callBackArgs.HasError = true;
            callBackArgs.ErrorValue = webRequest.error;
            if (!isGetData)
            {
                 Log.Debug("<color=#00eaff>接收消息:</color><color=#00ff9c>" + webRequest.url + "</color>");
                 Log.Debug("HTTP错误码:"+webRequest.responseCode);
            }
        }
        else
        {
            callBackArgs.Value = webRequest.downloadHandler.text;
            callBackArgs.Data = webRequest.downloadHandler.data;
            if (!isGetData)
            {
                 Log.Debug("<color=#00eaff>接收消息:</color><color=#00ff9c>" + webRequest.url + "</color>");
            }
        }
        try
        {
            if (requestGeneration == callbackGeneration && !suppressedCallbacks.Contains(webRequest))
            {
                sendCallBack?.Invoke(callBackArgs);
            }
        }
        catch (Exception exception)
        {
            Log.Error("HTTP回调执行失败: " + exception.Message);
        }
        finally
        {
            activeRequests.Remove(webRequest);
            requestOwners.Remove(webRequest);
            suppressedCallbacks.Remove(webRequest);
            if (requestKey != null && requestGeneration == callbackGeneration)
            {
                activeRequestKeys.Remove(requestKey);
            }
            webRequest.Dispose();
        }
    }

}
