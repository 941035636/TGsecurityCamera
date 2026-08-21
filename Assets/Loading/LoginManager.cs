using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityTimer;
using static ManManagentUi;

public class LoginManager : SingletonManager<LoginManager>
{



    // 登录成功页面
    public GameObject signSucceed;

    // 登录页面
    public GameObject signIn;
    public InputField inUserName;
    public InputField inPassword;
    public Text inTips;

    //登录成功后获取全局唯一RoleName/UserId/UserName
    public static string RoleName = string.Empty;
    public static string UserId = string.Empty;
    public static string UserName = string.Empty;
    public static int RoleId;
    //全局唯一角色权限
    Roles SingleRole = new Roles();
    //角色模块权限
    public bool IsPreview = false;
    public bool IsReplay = false;
    public bool IsCameraEvent = false;
    public bool IsDoorEvent = false;
    public bool IsPeopleManager = false;
    public bool IsUserManager = false;
    public bool IsCameraSet = false;
    public bool IsDoorSet = false;

    public List<GetAreaGroupDevs> PreviewAuthList ;
    public List<GetAreaGroupDevs> ReplayAuthList;

    [Serializable]
    public class LoginResData
    {
        public string msg = string.Empty;
        public int code;
        public data data;




    }
    [Serializable]
    public class data
    {
        public string auth = string.Empty;
        public string roleName = string.Empty;
        public string userName = string.Empty;
        public string userId = string.Empty;
        public int roleId;
    }

    private void Start()
    {
        ResetPermissions();
        inUserName.text = PlayerPrefs.GetString("userName");
        // Passwords must never be persisted in PlayerPrefs (plain text on disk).
        PlayerPrefs.DeleteKey("password");
        inPassword.text = string.Empty;
    }

    //登录回调
    public void LoginCallback(HttpCallBackArgs args)
    {
        if (args == null || args.HasError || string.IsNullOrEmpty(args.Value))
        {
            inTips.text = "服务器连接失败，请稍后重试";
            inTips.color = Color.red;
            return;
        }

        LoginResData response = JsonUtility.FromJson<LoginResData>(args.Value);
        if (response == null)
        {
            inTips.text = "登录响应格式错误";
            inTips.color = Color.red;
            return;
        }

        if (response.code == 200 && response.data != null)
        {
            RoleName = response.data.roleName;
            UserName = response.data.userName;
            UserId = response.data.userId;
            RoleId = response.data.roleId;
            HttpNetManager.SetAuthorizationToken(response.data.auth);
            int numericUserId;
            if (int.TryParse(UserId, out numericUserId))
            {
                PlayerPrefs.SetInt("userId", numericUserId);
            }
            Log.Debug("登陆成功");
            PlayerPrefs.SetString("userName", inUserName.text.Trim());
            PlayerPrefs.SetInt("IsLogin", 1);
            inTips.text = response.msg;
            inTips.color = Color.green;
            Timer.Register(0.5f, () =>
            {
                inTips.text = "";
            });
            ResetPermissions();
            UIManager.Instance.CloseWnd(ConStr.LOGINPAGE);
            UIManager.Instance.PopUpWnd(ConStr.FIRSTPAGE, true);
            GetuserAuthFromServer(RoleId);
        }
        else
        {
            inTips.text = response.msg;
            inTips.color = Color.red;
        }
    }
    /// <summary>
    /// 获取该账号对应的角色权限
    /// </summary>
    /// <param name="token"></param>
    void GetuserAuthFromServer(int roleId)
    {
        string url = GameStart.ApiUrl("/api/perm/tg/role/info?roleId=" + roleId);
        Log.Debug(url);
        HttpNetManager.GetInstance().SendDataStr(url, GetAuthcallBack, false, false, false);

    }

    public void ResetPermissions()
    {
        IsPreview = false;
        IsReplay = false;
        IsCameraEvent = false;
        IsDoorEvent = false;
        IsPeopleManager = false;
        IsUserManager = false;
        IsCameraSet = false;
        IsDoorSet = false;
        PreviewAuthList = new List<GetAreaGroupDevs>();
        ReplayAuthList = new List<GetAreaGroupDevs>();
    }

    void GetAuthcallBack(HttpCallBackArgs args)
    {
        Log.Debug("向服务器请求该账号角色信息");
        ResetPermissions();

        //IsPreview = true;
        //IsReplay = true;
        //IsCameraEvent = true;
        //IsDoorEvent = true;
        //IsPeopleManager = true;
        //IsUserManager = true;
        //IsCameraSet = true;
        //IsDoorSet = true;


        //获取权限赋值

        if (args != null && !args.HasError && !string.IsNullOrEmpty(args.Value))
        {
            Log.Debug("该角色权限信息:" + args.Value);
            //{"roleList":[{"roleId":10,"roleName":"测试角色1","menuList":[{"menuId":12,"menuName":"monitor_config","areaList":null}]}]}
            roleListData authList = JsonConvert.DeserializeObject<roleListData>(args.Value);
            if (authList != null && authList.roleList != null && authList.roleList.Count > 0 &&
                authList.roleList[0].menuList != null)
            {
                foreach (var item in authList.roleList[0].menuList)
                {
                    switch (item.menuName)
                    {
                        case "monitor_config":
                            IsCameraSet = true;
                            break;
                        case "door_config":
                            IsDoorSet = true;
                            break;
                        case "monitor_preview":
                            IsPreview = true;
                            if (item.areaList != null)
                                PreviewAuthList = item.areaList;
                            else
                                PreviewAuthList = new List<GetAreaGroupDevs>();
                            break;
                        case "monitor_playback":
                            IsReplay = true;
                            ReplayAuthList = item.areaList ?? new List<GetAreaGroupDevs>();
                            break;
                        case "monitor_event":
                            IsCameraEvent = true;
                            break;
                        case "door_event":
                            IsDoorEvent = true;
                            break;
                        case "user_management":
                            IsUserManager = true;
                            break;
                        case "personnel_management":
                            IsPeopleManager = true;
                            break;
                        default:
                            break;
                    }

                }

            }

        }

        else
        {
            GameStart.Instance.ShowTip("获取角色权限失败!");
        }


    }
    public class LoginData
    {
        public string userName = string.Empty;
        public string password = string.Empty;



    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {

            OnSignInClicked();
        }
    }

    public void OnSignInClicked() // 登录页面登录按钮
    {

        string name = inUserName.text;
        string password = inPassword.text;

        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(password))
        {
            LoginData loginData = new LoginData();
            loginData.userName = name;
            loginData.password = password;

            string loginstr = JsonUtility.ToJson(loginData, true);
            string loginUrl = GameStart.ApiUrl("/auth/login");
            Log.Debug("发送登录请求: " + loginUrl);
            SendDataStr(loginUrl, LoginCallback, true, true, false, loginstr);
        }
        else if (inUserName.text.Trim() == "" || inPassword.text.Trim() == "")
        {
            inTips.text = "用户名密码不能为空，请重新输入！";
            inTips.color = Color.red;
        }
        else if (PlayerPrefs.GetString(inUserName.text.Trim()) == null)
        {
            inTips.text = "用户不存在！请注册后再登录！";
            inTips.color = Color.red;
        }
        else if (PlayerPrefs.GetString(inUserName.text.Trim()) != inPassword.text.Trim())
        {
            inTips.text = "用户密码错误，请重新输入！";
            inTips.color = Color.red;
        }

    }
    //本地服务器
    public static readonly string LocalIP = "192.168.199.150:7711";
    //测试服务器IP
    public static readonly string TestIP = "192.168.1.237:7711";
    //服务器IP
    public static readonly string IP = "192.168.110.2:7711";

    private HttpSendDataCallBack1 m_SendCallBack;
    private readonly HttpCallBackArgs m_CallBackArgs;
    private bool m_IsBusy;

    /// <summary>
    /// 是否获取Data数据
    /// </summary>
    private bool m_IsGetData;

    public LoginManager()
    {
        m_CallBackArgs = new HttpCallBackArgs();
        m_CallBackArgs.Init();
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
    public void SendDataStr(string url, HttpSendDataCallBack1 sendCallBack, bool isPost = false, bool isPostBody = false, bool isGetData = false, string data = null)
    {
        if (m_IsBusy) return;
        m_IsBusy = true;
        m_IsGetData = isGetData;
        m_SendCallBack = sendCallBack;
        if (isPost)
        {
            if (isPostBody)
                PosturlBody(url, data); //Post  Body体传输
            else
                PosturlForm(url, data); //Post  表单传输

        }
        else
        {
            GetUrl(url);
        }
        if (!m_IsGetData)
        {
             Log.Debug("<color=#ffa200>发送消息:</color><color=#FFFB80>" + url + "</color>");
        }
    }

    public void SendDataObj(string url, HttpSendDataCallBack1 sendCallBack, bool isPost = false, bool isGetData = false, object data = null)
    {
        if (m_IsBusy) return;
        m_IsBusy = true;
        m_IsGetData = isGetData;
        m_SendCallBack = sendCallBack;
        string json = string.Empty;
        if (isPost)
        {
            json = JsonConvert.SerializeObject(data, Formatting.Indented);

            Log.Debug(json);
            //PosturlForm(url, json);
            PosturlBody(url, json);

        }
        else
        {
            GetUrl(url);
        }
        if (!m_IsGetData)
        {
             Log.Debug("<color=#ffa200>发送消息:</color><color=#FFFB80>" + url + "</color>");

        }
    }



    /// <summary>
    /// Get请求
    /// </summary>
    /// <param name="url"></param>
    private void GetUrl(string url)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(AppRuntimeConfig.NormalizeApiUrl(url));
        webRequest.timeout = 20;
        StartCoroutine(Request(webRequest));
    }
    /// <summary>
    /// Post   Form
    /// </summary>
    /// <param name="url"></param>
    /// <param name="json"></param>
    private void PosturlForm(string url, string json)
    {
        WWWForm form = new WWWForm();
        form.AddField("json", json);
        UnityWebRequest webRequest = UnityWebRequest.Post(AppRuntimeConfig.NormalizeApiUrl(url), form);
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.timeout = 20;
        StartCoroutine(Request(webRequest));

    }
    /// <summary>
    /// Post  Body
    /// </summary>
    /// <param name="url"></param>
    /// <param name="json"></param>
    private void PosturlBody(string url, string json)
    {
        StartCoroutine(RequestByJsonBodyPost(url, json));
    }
    private IEnumerator RequestByJsonBodyPost(string url, string json)
    {
        UnityWebRequest www = new UnityWebRequest(AppRuntimeConfig.NormalizeApiUrl(url), UnityWebRequest.kHttpVerbPOST);
        DownloadHandler downloadHandler = new DownloadHandlerBuffer();
        www.downloadHandler = downloadHandler;
        www.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.timeout = 20;
        yield return www.SendWebRequest();
        m_IsBusy = false;
        if (www.isHttpError || www.isNetworkError)
        {


            m_CallBackArgs.HasError = true;
            m_CallBackArgs.ErrorValue = www.error;
            m_SendCallBack?.Invoke(m_CallBackArgs);
            if (!m_IsGetData)
            {
                 Log.Debug("<color=#00eaff>接收消息:</color><color=#00ff9c>" + www.url + "</color>");
                 Log.Debug("<color=#c5e1dc>==>>" + JsonUtility.ToJson(m_CallBackArgs) + "</color>");
                 Log.Debug("HTTP错误码:" + www.responseCode);
            }
        }
        else
        {
            m_CallBackArgs.HasError = false;
            m_CallBackArgs.Value = www.downloadHandler.text;
            //webRequest.downloadHandler.data 数据是字节数组，序列化也序列化不出来内容 非GetData打印日志，并在data赋值之前打印
            if (!m_IsGetData)
            {
                Log.Debug("<color=#00eaff>接收消息:</color><color=#00ff9c>" + www.url + "</color>");
                Log.Debug("<color=#c5e1dc>==>>" + JsonUtility.ToJson(m_CallBackArgs) + "</color>");
            }
            m_CallBackArgs.Data = www.downloadHandler.data;
            m_SendCallBack?.Invoke(m_CallBackArgs);
        }
        www.Dispose();
        www = null;

    }

    private IEnumerator Request(UnityWebRequest webRequest)
    {
        yield return webRequest.SendWebRequest();
        m_IsBusy = false;
        if (webRequest.isHttpError || webRequest.isNetworkError)
        {


            m_CallBackArgs.HasError = true;
            m_CallBackArgs.ErrorValue = webRequest.error;
            m_SendCallBack?.Invoke(m_CallBackArgs);
            if (!m_IsGetData)
            {
                 Log.Debug("<color=#00eaff>接收消息:</color><color=#00ff9c>" + webRequest.url + "</color>");
                 Log.Debug("<color=#c5e1dc>==>>" + JsonUtility.ToJson(m_CallBackArgs) + "</color>");
                 Log.Debug("HTTP错误码:" + webRequest.responseCode);
            }
        }
        else
        {
            m_CallBackArgs.HasError = false;
            m_CallBackArgs.Value = webRequest.downloadHandler.text;
            //webRequest.downloadHandler.data 数据是字节数组，序列化也序列化不出来内容 非GetData打印日志，并在data赋值之前打印
            if (!m_IsGetData)
            {
                 Log.Debug("<color=#00eaff>接收消息:</color><color=#00ff9c>" + webRequest.url + "</color>");
                 Log.Debug("<color=#c5e1dc>==>>" + JsonUtility.ToJson(m_CallBackArgs) + "</color>");
            }
            m_CallBackArgs.Data = webRequest.downloadHandler.data;
            m_SendCallBack?.Invoke(m_CallBackArgs);
        }
        webRequest.Dispose();
        webRequest = null;

    }
}
public delegate void HttpSendDataCallBack1(HttpCallBackArgs callBackArgs);
public class HttpCallBackArgs1 : EventArgs
{


    public bool HasError;//是否有错
    public string ErrorValue;//错误信息
    public string Value;//值
    public byte[] Data;//数据

    public void Init()
    {
        HasError = false;
        ErrorValue = string.Empty;
        Value = string.Empty;
        Data = null;
    }

}
