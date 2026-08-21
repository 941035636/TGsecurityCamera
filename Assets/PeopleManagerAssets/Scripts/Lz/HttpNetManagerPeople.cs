using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


public class HttpCallBackArgsPeople : EventArgs
{

    public bool HasError;//是否有错
    public string ErrorValue;//错误信息
    public string Value;//值
    public byte[] Data;//数据
    public Action<List<object>, int> callBak;
    public void Init()
    {
        HasError = false;
        ErrorValue = string.Empty;
        Value = string.Empty;
        Data = null;
        callBak = null;
    }

}

public delegate void HttpSendDataCallBackPeople(HttpCallBackArgsPeople callBackArgs);
public class HttpNetManagerPeople
{


    private static HttpNetManagerPeople instance;
    public static HttpNetManagerPeople GetInstance()
    {
        if (instance == null)
        {
            instance = new HttpNetManagerPeople();
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
    public void SendDataStr(string url, HttpSendDataCallBackPeople sendCallBack, Action<List<object>, int> callBak, bool isPost = false, bool isPostBody = false, bool isGetData = false, string data = null)
    {
        if (isPost)
        {
            if (isPostBody)
                PosturlBody(url, data, sendCallBack, callBak, isGetData); //Post  Body体传输
            else
                PosturlForm(url, data, sendCallBack, callBak, isGetData); //Post  表单传输

        }
        else
        {
            GetUrl(url, sendCallBack, callBak, isGetData);
        }
        if (!isGetData)
        {
             Log.Debug("<color=#ffa200>发送消息:</color><color=#FFFB80>" + url + "</color>");
        }
    }

    public void SendDataObj(string url, HttpSendDataCallBackPeople sendCallBack, bool isPost = false, bool isGetData = false, object data = null)
    {
        string json = string.Empty;
        if (isPost)
        {
            json = JsonConvert.SerializeObject(data, Formatting.Indented);

            PosturlBody(url, json, sendCallBack, null, isGetData);
        }
        else
        {
            GetUrl(url, sendCallBack, null, isGetData);
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
    private void GetUrl(string url, HttpSendDataCallBackPeople sendCallBack,
        Action<List<object>, int> callBak, bool isGetData)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(AppRuntimeConfig.NormalizeApiUrl(url));
        HttpNetManager.PrepareRequest(webRequest);
        GameStart.Instance.StartCoroutine(Request(webRequest, sendCallBack, callBak, isGetData));
    }
    /// <summary>
    /// Post   Form
    /// </summary>
    /// <param name="url"></param>
    /// <param name="json"></param>
    private void PosturlForm(string url, string json, HttpSendDataCallBackPeople sendCallBack,
        Action<List<object>, int> callBak, bool isGetData)
    {
        WWWForm form = new WWWForm();
        form.AddField("json", json);
        UnityWebRequest webRequest = UnityWebRequest.Post(AppRuntimeConfig.NormalizeApiUrl(url), form);
        webRequest.SetRequestHeader("Content-Type", "application/json");
        HttpNetManager.PrepareRequest(webRequest);
        GameStart.Instance.StartCoroutine(Request(webRequest, sendCallBack, callBak, isGetData));
    }
    /// <summary>
    /// Post  Body
    /// </summary>
    /// <param name="url"></param>
    /// <param name="json"></param>
    private void PosturlBody(string url, string json, HttpSendDataCallBackPeople sendCallBack,
        Action<List<object>, int> callBak, bool isGetData)
    {
        GameStart.Instance.StartCoroutine(RequestByJsonBodyPost(url, json, sendCallBack, callBak, isGetData));
    }
    private IEnumerator RequestByJsonBodyPost(string url, string json,
        HttpSendDataCallBackPeople sendCallBack, Action<List<object>, int> callBak, bool isGetData)
    {
        UnityWebRequest www = new UnityWebRequest(AppRuntimeConfig.NormalizeApiUrl(url), UnityWebRequest.kHttpVerbPOST);
        DownloadHandler downloadHandler = new DownloadHandlerBuffer();
        www.downloadHandler = downloadHandler;
        www.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        HttpNetManager.PrepareRequest(www);
        yield return Request(www, sendCallBack, callBak, isGetData);

    }

    private IEnumerator Request(UnityWebRequest webRequest, HttpSendDataCallBackPeople sendCallBack,
        Action<List<object>, int> callBak, bool isGetData)
    {
        yield return webRequest.SendWebRequest();
        HttpCallBackArgsPeople callBackArgs = new HttpCallBackArgsPeople();
        callBackArgs.Init();
        callBackArgs.callBak = callBak;
        if (webRequest.isHttpError || webRequest.isNetworkError)
        {
            callBackArgs.HasError = true;
            callBackArgs.ErrorValue = webRequest.error;
            if (!isGetData)
            {
                 Log.Debug("<color=#00eaff>接收消息:</color><color=#00ff9c>" + webRequest.url + "</color>");
                 Log.Debug("HTTP错误码:" + webRequest.responseCode);
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
        sendCallBack?.Invoke(callBackArgs);
        webRequest.Dispose();
    }

}
