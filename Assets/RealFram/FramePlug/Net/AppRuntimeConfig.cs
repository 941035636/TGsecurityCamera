using System;
using System.IO;
using UnityEngine;

[Serializable]
public class AppRuntimeSettings
{
    public string apiScheme = "http";
    public string apiHost = "192.168.110.2:7711";
    public string mqttHost = "192.168.110.2";
    public int mqttPort = 1883;
    public string mqttUserName = "";
    public string mqttPassword = "";
    public int mqttReconnectMinSeconds = 2;
    public int mqttReconnectMaxSeconds = 30;
    public string faceServiceUrl = "http://192.168.110.3:9001/FaceService/RtspWithFramApi";
    public bool sendAuthorizationHeader = false;
    public string authorizationHeader = "Authorization";
    public string authorizationScheme = "Bearer";
    public bool enableLegacyMqttReceiver = false;
    public bool enableDebugLogs = false;
    public bool enableOfflineMode = false;
    public int videoPreloadCount = 64;
    public int videoPreloadPerFrame = 4;
}

public static class AppRuntimeConfig
{
    private const string ConfigDirectory = "Configurations";
    private const string ConfigFileName = "AppRuntimeSettings.json";
    private static AppRuntimeSettings settings;

    public static AppRuntimeSettings Settings
    {
        get
        {
            if (settings == null) Reload();
            return settings;
        }
    }

    public static string ApiHost
    {
        get { return Settings.apiHost.Trim().TrimEnd('/'); }
    }

    public static string ApiBaseUrl
    {
        get { return Settings.apiScheme.Trim().TrimEnd(':', '/') + "://" + ApiHost; }
    }

    public static void Reload()
    {
        settings = new AppRuntimeSettings();
        string path = Path.Combine(Application.streamingAssetsPath, ConfigDirectory, ConfigFileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning("运行配置不存在，使用内置部署地址 " + ApiBaseUrl + ": " + path);
            return;
        }

        try
        {
            AppRuntimeSettings loaded = JsonUtility.FromJson<AppRuntimeSettings>(File.ReadAllText(path));
            if (loaded != null && !string.IsNullOrEmpty(loaded.apiHost) && !string.IsNullOrEmpty(loaded.apiScheme))
            {
                settings = loaded;
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("读取运行配置失败，使用内置默认值: " + exception.Message);
        }
    }

    public static string ApiUrl(string path)
    {
        if (string.IsNullOrEmpty(path)) return ApiBaseUrl;
        return ApiBaseUrl + (path[0] == '/' ? path : "/" + path);
    }

    public static string NormalizeApiUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return url;
        if (url[0] == '/') return ApiUrl(url);

        string httpPrefix = "http://" + ApiHost;
        string httpsPrefix = "https://" + ApiHost;
        if (url.StartsWith(httpPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return ApiBaseUrl + url.Substring(httpPrefix.Length);
        }
        if (url.StartsWith(httpsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return ApiBaseUrl + url.Substring(httpsPrefix.Length);
        }
        return url;
    }
}
