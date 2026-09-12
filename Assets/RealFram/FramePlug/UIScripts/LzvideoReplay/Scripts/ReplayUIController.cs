using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ZTools;
//using UMP;
using RenderHeads.Media.AVProVideo;
using UnityTimer;
using TMPro;
using LitJson;
using System.Text;
using UnityEngine.Networking;
using HslCommunication.LogNet;
using Vuplex.WebView.Demos;
using Vuplex.WebView;
using System.Runtime.InteropServices;
using System.Linq;
using System.Threading.Tasks;

public class ReplayUIController : SingletonManager<ReplayUIController>
{
    private Button _foldBtn1;
    private bool _isFold1;
    private Button _jianKongDian;
    private Button _shiJian;
    private Button _pos;
    private Button _atm;
    private Button _yinPin;
    private GameObject _zcalendar;
    private Button _dateFrame;
    private Text _startDay;
    private Text _endDay;
    private GameObject _menuUnfoldPage;
    private bool _isFold2;
    private GameObject _rightPage;
    public GameObject _videoArea;
    private Button _fullScreenBtn;
    private Button _showVideoNumBtn;
    private Button _clearVideoBtn;
    public Toggle _playVideoTog;
    private GameObject _segmentationBG;
    public int ChooseViewNum;
    public bool isFull;
    private Button _segmentation1;
    private Button _segmentation4;
    private Button _segmentation6;
    private Button _segmentation8;
    private Button _segmentation9;
    private Button _segmentation13;
    private Button _segmentation16;
    private Button FastFBtn;//快进
    private Button FastBBtn;//快退
    public TextMeshProUGUI RateTxt;
    private GameObject _dateCreatedScroll;
    private Transform DateScrollConten;
    private float VideoAreaHeight;
    private Toggle[] toggles;
    public string RtspUrl;
    private Text maskTxt;
    public float rate = 1;
    public bool IsUseAvProReplay = false;
    public string Sendstr = string.Empty;
    public string OldUrl = string.Empty;
    public class ExitReplay
    {
        public long userId;
        public int camId;
        public string url = string.Empty;
        public ExitReplay(long userid, int camid, string _url)
        {
            userId = userid;
            camId = camid;
            url = _url;
        }
    }

    private void OnDisable()
    {
        EventCenter.RemoveListener<string>(Eventdefine.SendPlayback, PlaybackListener);
        EventCenter.RemoveListener(Eventdefine.StopPlayVideo, StopPlay);
        EventCenter.RemoveListener(Eventdefine.StartPlayVideo, StartPlay);
        EventCenter.RemoveListener(Eventdefine.resetTime, resetTime);
        //退出的时候想服务器发送关闭该userid下的camid设备
        //if (VideoPlayBack.isCounting)
        //{
            CloseReplay();
        //}


    }
    private void OnApplicationQuit()
    {

        //if (VideoPlayBack.isCounting)
        //{

            CloseReplay();
        //}


    }

    private void CloseReplay()
    {
        //退出的时候想服务器发送关闭该userid下的camid设备
        ExitReplay exitReplay = new ExitReplay(long.Parse(PlayerPrefs.GetInt("userId").ToString()), UIManger.ChoiceCameraId, OldUrl);
        //ExitReplay exitReplay = new ExitReplay(123, 212);
        string str = JsonUtility.ToJson(exitReplay);
        string Url = "http://" + GameStart.IP + "/api/video/playback/del/path";
        HttpNetManager.GetInstance().SendDataStr(Url, ExitReplayCallback, true, true, false, str);
        Log.Debug("平台退出向服务器发送关闭录像:" + str + "   Url:" + Url);

    }



    void ExitReplayCallback(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {
            Log.Debug("收到退出回放服务器返回消息:" + args.Value);
        }
    }

    void Start()
    {
        IsUseAvProReplay = false;
        _isFold1 = false;
        _isFold2 = false;
        isFull = false;
        _foldBtn1 = transform.Find("LeftPage/MenuBtnPage/Scroll View/Viewport/FoldBtn1").GetComponent<Button>();
        _jianKongDian = transform.Find("LeftPage/MenuBtnPage/Scroll View/Viewport/Content/JianKongDian").GetComponent<Button>();
        _dateFrame = transform.Find("LeftPage/MenuUnfoldPage/bg/DateFrame").GetComponent<Button>();
        _zcalendar = transform.Find("Zcalendar").gameObject;
        _startDay = transform.Find("LeftPage/MenuUnfoldPage/bg/DateFrame/Day1").GetComponent<Text>();
        _endDay = transform.Find("LeftPage/MenuUnfoldPage/bg/DateFrame/Day2").GetComponent<Text>();
        _menuUnfoldPage = transform.Find("LeftPage/MenuUnfoldPage").gameObject;
        _rightPage = transform.Find("LeftPage/RightPage").gameObject;
        _videoArea = transform.Find("LeftPage/RightPage/VideoArea").gameObject;
        _fullScreenBtn = transform.Find("LeftPage/RightPage/RightDownPage/UpBtn/FullScreenBtn").GetComponent<Button>();
        _showVideoNumBtn = transform.Find("LeftPage/RightPage/RightDownPage/UpBtn/ShowVideoNumBtn").GetComponent<Button>();
        _clearVideoBtn = transform.Find("LeftPage/RightPage/RightDownPage/UpBtn/ClearVideo").GetComponent<Button>();
        _playVideoTog = transform.Find("LeftPage/RightPage/RightDownPage/UpBtn/play").GetComponent<Toggle>();
        _segmentationBG = transform.Find("LeftPage/RightPage/RightDownPage/UpBtn/ShowVideoNumBtn/SegmentationBG").gameObject;
        _segmentation1 = transform.Find("LeftPage/RightPage/RightDownPage/UpBtn/ShowVideoNumBtn/SegmentationBG/1").GetComponent<Button>();
        _segmentation4 = transform.Find("LeftPage/RightPage/RightDownPage/UpBtn/ShowVideoNumBtn/SegmentationBG/4").GetComponent<Button>();
        _segmentation6 = transform.Find("LeftPage/RightPage/RightDownPage/UpBtn/ShowVideoNumBtn/SegmentationBG/6").GetComponent<Button>();
        _segmentation8 = transform.Find("LeftPage/RightPage/RightDownPage/UpBtn/ShowVideoNumBtn/SegmentationBG/8").GetComponent<Button>();
        _segmentation9 = transform.Find("LeftPage/RightPage/RightDownPage/UpBtn/ShowVideoNumBtn/SegmentationBG/9").GetComponent<Button>();
        _segmentation13 = transform.Find("LeftPage/RightPage/RightDownPage/UpBtn/ShowVideoNumBtn/SegmentationBG/13").GetComponent<Button>();
        _segmentation16 = transform.Find("LeftPage/RightPage/RightDownPage/UpBtn/ShowVideoNumBtn/SegmentationBG/16").GetComponent<Button>();
        FastFBtn = transform.Find("LeftPage/RightPage/RightDownPage/UpBtn/FastFBtn").GetComponent<Button>();
        FastBBtn = transform.Find("LeftPage/RightPage/RightDownPage/UpBtn/FastBBtn").GetComponent<Button>();
        RateTxt = transform.Find("LeftPage/RightPage/RightDownPage/UpBtn/rateTxt").GetComponent<TextMeshProUGUI>();
        _dateCreatedScroll = transform.Find("LeftPage/RightPage/RightDownPage/DownBtn/DateCreatedScroll").gameObject;
        maskTxt = transform.Find("LeftPage/RightPage/mask").GetComponent<Text>();
        DateScrollConten = _dateCreatedScroll.transform.GetChild(0).GetChild(0);
        _foldBtn1.onClick.AddListener(FoldOtherBtnOne);
        _dateFrame.onClick.AddListener(DateTimeq);
        _showVideoNumBtn.onClick.AddListener(ShowVideoNum);
        //_clearVideoBtn.onClick.AddListener(ClearVideoArew);
        _segmentation1.onClick.AddListener(() => Segmentation(1, 1));
        _segmentation4.onClick.AddListener(() => Segmentation(4, 4));
        _segmentation6.onClick.AddListener(() => Segmentation(10, 6));
        _segmentation8.onClick.AddListener(() => Segmentation(17, 8));
        _segmentation9.onClick.AddListener(() => Segmentation(9, 9));
        _segmentation13.onClick.AddListener(() => Segmentation(17, 13));
        _segmentation16.onClick.AddListener(() => Segmentation(16, 16));
        _fullScreenBtn.onClick.AddListener(FullScreenViewArea);
        VideoAreaHeight = 905;
        //if (!IsUseAvProReplay)
        //    Segmentation(1, 1);//用UMP播放就打开
        EventCenter.addlistener<string>(Eventdefine.SendPlayback, PlaybackListener);

        EventCenter.addlistener(Eventdefine.StopPlayVideo, StopPlay);
        EventCenter.addlistener(Eventdefine.StartPlayVideo, StartPlay);
        EventCenter.addlistener(Eventdefine.resetTime, resetTime);

        //FastFBtn.onClick.AddListener(() =>
        //{
        //    if (IsUseAvProReplay)
        //        _videoArea.transform.GetChild(0).GetChild(1).GetComponent<VideoPlayBack>().FastPlay();
        //    else
        //        _videoArea.transform.GetChild(0).GetChild(0).GetComponent<VideoPlayBack>().FastPlay();
        //    RateTxt.text = rate + "X";

        //    Log.Debug("rate:" + rate);
        //});
        //FastBBtn.onClick.AddListener(() =>
        //{
        //    if (IsUseAvProReplay)
        //        _videoArea.transform.GetChild(0).GetChild(1).GetComponent<VideoPlayBack>().SlowPlay();
        //    else
        //        _videoArea.transform.GetChild(0).GetChild(0).GetComponent<VideoPlayBack>().SlowPlay();
        //    RateTxt.text = rate + "X";
        //    Log.Debug("rate:" + rate);
        //});

        maskTxt.GetComponent<Text>().text = LoginManager.UserName;

        Timer timer = null;

        _playVideoTog.onValueChanged.AddListener((ison) =>
        {

            if (ison)
            {
                _playVideoTog.transform.Find("Background").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/录像回放/1");
                _playVideoTog.transform.Find("Background").GetComponent<Image>().SetNativeSize();

               
                if (IsPause && IsnewPlay)
                {
                    if (UIManger.ChoiceCameraId != 0 && TimeSlider.SliderTime != "000000")
                    {


                        //SendPlayback playback = new SendPlayback();
                        //playback.camId = UIManger.ChoiceCameraId;
                        //playback.userId = PlayerPrefs.GetInt("userId");
                        //playback.startTime = StartTime;
                        //playback.endTime = EndTime;
                        //Sendstr = JsonUtility.ToJson(playback);
                        //Log.Debug("向服务器发送的Json:" + Sendstr);

                        //PlaybackListener(Sendstr);
                    }
                    else if (UIManger.ChoiceCameraId == 0)
                    {

                        GameStart.Instance.ShowTip("请选择要回放的监控设备");
                        _playVideoTog.isOn = false;

                    }
                    else if (TimeSlider.SliderTime == "000000")
                    {

                        GameStart.Instance.ShowTip("请选择要回放的时间段");
                        _playVideoTog.isOn = false;
                    }


                }
                else
                {
                    EventCenter.BroadCast(Eventdefine.StartPlayVideo);
                }

            }
            else
            {
                Log.Debug("暂停播放");
                EventCenter.BroadCast(Eventdefine.StopPlayVideo);
                _playVideoTog.transform.Find("Background").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/录像回放/前进-1");
                _playVideoTog.transform.Find("Background").GetComponent<Image>().SetNativeSize();
            }




        });



    }


    public IEnumerator PostData(string jsondata, string url)
    {


        byte[] databyte = Encoding.UTF8.GetBytes(jsondata);
        UnityWebRequest _request = new UnityWebRequest(AppRuntimeConfig.NormalizeApiUrl(url), UnityWebRequest.kHttpVerbPOST);
        _request.uploadHandler = new UploadHandlerRaw(databyte);
        _request.downloadHandler = new DownloadHandlerBuffer();

        _request.SetRequestHeader("Content-Type", "application/json");
        HttpNetManager.PrepareRequest(_request);
        yield return _request.SendWebRequest();

        if (_request.error != null)
        {
            Debug.LogError(_request.error);
        }
        else
        {

        }
        _request.Dispose();
    }






    void ExitReplayCallbackMainPlay(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {
            Log.Debug("收到退出回放服务器返回消息:" + args.Value);
            PlaybackListener(Sendstr);
        }
    }



    //录像回放播放
    private void PlaybackListener(string sendstr)
    {
        //VideoPlayBack.Ins.LoadingReplay();
        Log.Debug("回放URl: " + "http://" + GameStart.IP + "/api/video/playback/addr/path");
        HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/video/playback/addr/path", PlayBackhttpCallback, true, true, false, sendstr);
    }

    //private void playFastBListener(string sendstr)
    //{
    //    Log.Debug("播放快进快退:" + sendstr);
    //    HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/video/playback/fast/addr", PlayBackhttpCallback, true, true, false, sendstr);
    //}
  


    [Serializable]
    public class SendPlayback
    {
        public int camId;
        public string startTime = string.Empty;
        public string endTime = string.Empty;
        public int userId;
        public string oldUrl = string.Empty;
    }
    [Serializable]
    public class SendPlaybackFast
    {
        public int camId;
        public string startTime = string.Empty;
        public string endTime = string.Empty;
        public string userId = string.Empty;
        public int speedRate;
    }

    [Serializable]
    public class PlaybackUrl
    {

        public string statusDesc = string.Empty;
        public string rstpAddr = string.Empty;
    }
    //调用JS传入URL
    [DllImport("__Internal")]
    public static extern void ChangeUrl(string url);


    //回放Http请求Post回调
    public void PlayBackhttpCallback(HttpCallBackArgs args)
    {
        Log.Debug("收到回放服务器返回信息:" + args.Value);
        if (!string.IsNullOrEmpty(args.Value))
        {


            PlaybackUrl playback = JsonUtility.FromJson<PlaybackUrl>(args.Value);

            if (playback.statusDesc == "SUCCESS")
            {

                GameStart.Instance.ShowTip("请求成功，请稍后...");
                Log.Debug("播放链接:" + playback.rstpAddr);
                string path = playback.rstpAddr;
                OldUrl = path;
                if (string.IsNullOrEmpty(path) || string.Equals(path, "null", StringComparison.OrdinalIgnoreCase))
                {
                    GameStart.Instance.ShowTip("此监控不存在该时间段内的录像文件！");
                }
                else 
                {
                    //if (IsUseAvProReplay)
                    //    _videoArea.transform.GetChild(0).GetChild(1).GetComponent<VideoPlayBack>().PlayVideoByAvpro(path);
                    //else
                    //    _videoArea.transform.GetChild(0).GetChild(0).GetComponent<VideoPlayBack>().PlayVideoByUmp(path);


                    if (mainWebViewPrefab != null)
                        mainWebViewPrefab.Destroy();
                    if (_hardwareKeyboardListener != null)
                        GameObject.Destroy(_hardwareKeyboardListener.gameObject);
                    OpenWebGl(_videoArea.transform.GetChild(0), BuildPlaybackHtml(path), true);
                }


              
            }
            else
            {
                GameStart.Instance.ShowTip("请求失败，请重试");

            }

        }
        else
        {

            GameStart.Instance.ShowTip("收到回放服务器返回信息为空");

        }




    }




    #region
    //内嵌网页
    CanvasWebViewPrefab _focusedPrefab;
    HardwareKeyboardListener _hardwareKeyboardListener;
    CanvasWebViewPrefab mainWebViewPrefab;
    private static string BuildPlaybackHtml(string url)
    {
        string safeUrl = (url ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("'", "\\'")
            .Replace("</", "<\\/");
        return "<!doctype html><html><head><meta charset='utf-8'>" +
               "<meta name='viewport' content='width=device-width,height=device-height,initial-scale=1'>" +
               "<link href='https://cdnjs.cloudflare.com/ajax/libs/video.js/7.3.0/video-js.min.css' rel='stylesheet'>" +
               "<script src='https://cdnjs.cloudflare.com/ajax/libs/video.js/7.3.0/video.min.js'></script>" +
               "<style>html,body,#replay{width:100%;height:100%;margin:0;background:#000}.video-js{width:100%;height:100%}</style>" +
               "</head><body><video id='replay' class='video-js vjs-default-skin vjs-big-play-centered' controls muted autoplay></video>" +
               "<script>videojs('replay',{autoplay:true,muted:true,preload:'auto',fluid:false," +
               "playbackRates:[0.5,1,1.5,2,4],sources:[{src:'" + safeUrl + "',type:'application/x-mpegURL'}]});</script>" +
               "</body></html>";
    }

    async void OpenWebGl(Transform parent, string content, bool loadHtml = false)
    {
         mainWebViewPrefab = CanvasWebViewPrefab.Instantiate();
        mainWebViewPrefab.Resolution = 0.8f;
        mainWebViewPrefab.PixelDensity = 1;
        mainWebViewPrefab.Native2DModeEnabled = true;
        mainWebViewPrefab.transform.SetParent(parent, false);
        ////注册关闭页面回调
        //mainWebViewPrefab.transform.Find("CloseBtn").GetComponent<Button>().onClick.AddListener(() =>
        //{
        //    mainWebViewPrefab.Destroy();
        //    GameObject.Destroy(_hardwareKeyboardListener.gameObject);
        //});
        mainWebViewPrefab.transform.Find("CloseBtn").gameObject.SetActive(false);

        var rectTransform = mainWebViewPrefab.transform as RectTransform;
        rectTransform.anchoredPosition3D = Vector3.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(1530,950);
        mainWebViewPrefab.transform.localScale = Vector3.one;
        _focusedPrefab = mainWebViewPrefab;

        _setUpKeyboards();

        // Wait for the CanvasWebViewPrefab to initialize, because the CanvasWebViewPrefab.WebView property
        // is null until the prefab has initialized.
        await mainWebViewPrefab.WaitUntilInitialized();

        // The CanvasWebViewPrefab has initialized, so now we can use its WebViewPrefab.WebView property.
        if (loadHtml)
            mainWebViewPrefab.WebView.LoadHtml(content);
        else
            mainWebViewPrefab.WebView.LoadUrl(content);

        var webViewWithPopups = mainWebViewPrefab.WebView as IWithPopups;
        if (webViewWithPopups == null)
        {
            return;
        }
       
        webViewWithPopups.SetPopupMode(PopupMode.LoadInNewWebView);
        webViewWithPopups.PopupRequested += async (webView, eventArgs) =>
        {
            Log.Debug("Popup opened with URL: " + eventArgs.Url);
            var popupPrefab = CanvasWebViewPrefab.Instantiate(eventArgs.WebView);
            popupPrefab.Resolution = mainWebViewPrefab.Resolution;
            _focusedPrefab = popupPrefab;

            popupPrefab.transform.SetParent(parent, false);
            var popupRectTransform = popupPrefab.transform as RectTransform;
            popupRectTransform.anchoredPosition3D = Vector3.zero;
            popupRectTransform.offsetMin = Vector2.zero;
            popupRectTransform.offsetMax = Vector2.zero;
            popupPrefab.transform.localScale = Vector3.one;
            // Place the popup in front of the main webview.
            var localPosition = popupPrefab.transform.localPosition;
            localPosition.z = 0.1f;
            popupPrefab.transform.localPosition = localPosition;

            await popupPrefab.WaitUntilInitialized();
            popupPrefab.WebView.CloseRequested += (popupWebView, closeEventArgs) =>
            {
                Log.Debug("Closing the popup");
                _focusedPrefab = mainWebViewPrefab;
                popupPrefab.Destroy();
            };
        };


    }

    void _setUpKeyboards()
    {

        // Send keys from the hardware (USB or Bluetooth) keyboard to the webview.
        // Use separate KeyDown() and KeyUp() methods if the webview supports
        // it, otherwise just use IWebView.SendKey().
        // https://developer.vuplex.com/webview/IWithKeyDownAndUp
        _hardwareKeyboardListener = HardwareKeyboardListener.Instantiate();
        _hardwareKeyboardListener.KeyDownReceived += (sender, eventArgs) =>
        {
            var webViewWithKeyDown = _focusedPrefab.WebView as IWithKeyDownAndUp;
            if (webViewWithKeyDown != null)
            {
                webViewWithKeyDown.KeyDown(eventArgs.Value, eventArgs.Modifiers);
            }
            else
            {
                _focusedPrefab.WebView.SendKey(eventArgs.Value);
            }
        };
        _hardwareKeyboardListener.KeyUpReceived += (sender, eventArgs) =>
        {
            var webViewWithKeyUp = _focusedPrefab.WebView as IWithKeyDownAndUp;
            webViewWithKeyUp?.KeyUp(eventArgs.Value, eventArgs.Modifiers);
        };
    }

    const string NOT_SUPPORTED_HTML = @"
            <body>
                <style>
                    body {
                        font-family: sans-serif;
                        display: flex;
                        justify-content: center;
                        align-items: center;
                        line-height: 1.25;
                    }
                    div {
                        max-width: 80%;
                    }
                    li {
                        margin: 10px 0;
                    }
                </style>
                <div>
                    <p>
                        Sorry, but this 3D WebView package doesn't support yet the <a href='https://developer.vuplex.com/webview/IWithPopups'>IWithPopups</a> interface. Current packages that support popups:
                    </p>
                    <ul>
                        <li>
                            <a href='https://developer.vuplex.com/webview/StandaloneWebView'>3D WebView for Windows and macOS</a>
                        </li>
                        <li>
                            <a href='https://developer.vuplex.com/webview/AndroidWebView'>3D WebView for Android</a>
                        </li>
                        <li>
                            <a href='https://developer.vuplex.com/webview/AndroidGeckoWebView'>3D WebView for Android with Gecko Engine</a>
                        </li>
                    </ul>
                </div>
            </body>
        ";
    #endregion


    ///暂停播放
    ///

    public static bool IsPause = true;
    public void StopPlay()
    {
        IsPause = true;

        //VideoPlayBack.isPlaying = false;
        //if (IsUseAvProReplay)
        //    _videoArea.transform.GetChild(0).GetChild(1).GetComponent<VideoPlayBack>().Pause();
        //else
        //    _videoArea.transform.GetChild(0).GetChild(0).GetComponent<VideoPlayBack>().Pause();
    }
    /// <summary>
    /// 恢复播放
    /// </summary>
    public void StartPlay()
    {
        IsPause = false;
        //if (IsUseAvProReplay)
        //    _videoArea.transform.GetChild(0).GetChild(1).GetComponent<VideoPlayBack>().Play();
        //else
        //    _videoArea.transform.GetChild(0).GetChild(0).GetComponent<VideoPlayBack>().Play();
    }



    /// <summary>
    /// 折叠最左侧回放菜单页
    /// </summary>
    void FoldOtherBtnOne()
    {
        if (_isFold1)
        {
            gameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(220, gameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y);
            _rightPage.GetComponent<RectTransform>().sizeDelta = new Vector2(_rightPage.GetComponent<RectTransform>().sizeDelta.x - 160, _rightPage.GetComponent<RectTransform>().sizeDelta.y);
            _videoArea.GetComponent<LayoutElement>().preferredWidth -= 160;
            FoldViewArea();
            gameObject.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(false);
            gameObject.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            gameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(60, gameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y);
            _rightPage.GetComponent<RectTransform>().sizeDelta = new Vector2(_rightPage.GetComponent<RectTransform>().sizeDelta.x + 160, _rightPage.GetComponent<RectTransform>().sizeDelta.y);
            _videoArea.GetComponent<LayoutElement>().preferredWidth += 160;
            FoldViewArea();
            gameObject.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(false);
            gameObject.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
        }
        _isFold1 = !_isFold1;
    }
    /// <summary>
    /// 折叠菜单展开详情页
    /// </summary>
    void FoldOtherBtnTwo()
    {
        if (_isFold2)
        {
            _menuUnfoldPage.transform.DOScaleX(1, 0);
            _menuUnfoldPage.GetComponent<RectTransform>().sizeDelta = new Vector2(250, _menuUnfoldPage.GetComponent<RectTransform>().sizeDelta.y);
            _rightPage.GetComponent<RectTransform>().sizeDelta = new Vector2(_rightPage.GetComponent<RectTransform>().sizeDelta.x - 250, _rightPage.GetComponent<RectTransform>().sizeDelta.y);
            _videoArea.GetComponent<LayoutElement>().preferredWidth -= 250;
            FoldViewArea();
            _menuUnfoldPage.SetActive(false);
            _menuUnfoldPage.SetActive(true);
        }
        else
        {
            _menuUnfoldPage.transform.DOScaleX(0, 0);
            _menuUnfoldPage.GetComponent<RectTransform>().sizeDelta = new Vector2(0, _menuUnfoldPage.GetComponent<RectTransform>().sizeDelta.y);
            _rightPage.GetComponent<RectTransform>().sizeDelta = new Vector2(_rightPage.GetComponent<RectTransform>().sizeDelta.x + 250, _rightPage.GetComponent<RectTransform>().sizeDelta.y);
            _videoArea.GetComponent<LayoutElement>().preferredWidth += 250;
            FoldViewArea();
            _menuUnfoldPage.SetActive(false);
            _menuUnfoldPage.SetActive(true);
        }
        _isFold2 = !_isFold2;
    }
    /// <summary>
    /// 折叠左边菜单时对监控区域画面大小的适配
    /// </summary>
    void FoldViewArea()
    {
        switch (ChooseViewNum)
        {
            case 1:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize
                = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth, _videoArea.GetComponent<RectTransform>().sizeDelta.y);
                break;
            case 4:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize
                = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 2, _videoArea.GetComponent<RectTransform>().sizeDelta.y / 2);
                break;
            case 6:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize
                = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 3, _videoArea.GetComponent<RectTransform>().sizeDelta.y / 3);
                Timer.Register(0.01f, () =>
                {
                    _videoArea.transform.Find("View0").GetComponent<RectTransform>().sizeDelta
                = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 3 * 2 + 2, _videoArea.GetComponent<RectTransform>().sizeDelta.y / 3 * 2 + 2);
                });

                break;
            case 8:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize
                = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 4, _videoArea.GetComponent<RectTransform>().sizeDelta.y / 4);
                Timer.Register(0.01f, () =>
                {
                    _videoArea.transform.Find("View0").GetComponent<RectTransform>().sizeDelta
                = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 4 * 3 + 4, _videoArea.GetComponent<RectTransform>().sizeDelta.y / 4 * 3 + 4);
                });

                break;
            case 9:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize
                = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 3, _videoArea.GetComponent<RectTransform>().sizeDelta.y / 3);
                break;
            case 13:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize
                = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 4, _videoArea.GetComponent<RectTransform>().sizeDelta.y / 4);
                Timer.Register(0.01f, () =>
                {
                    _videoArea.transform.Find("View0").GetComponent<RectTransform>().sizeDelta
                = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 4 * 2 + 2, _videoArea.GetComponent<RectTransform>().sizeDelta.y / 4 * 2 + 2);
                });

                break;
            case 16:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize
                = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 4, _videoArea.GetComponent<RectTransform>().sizeDelta.y / 4);
                break;
        }
    }
    /// <summary>
    /// 打开日期查询
    /// </summary>
    void DateTimeq()
    {
        _zcalendar.GetComponent<ZCalendar>().Show();

    }

    /// <summary>
    /// 根据选择的日期创建时间段内的每个日期
    /// </summary>
    /// <param name="Day1"></param>
    /// <param name="Day2"></param>
    /// 
    public static string STime = string.Empty;
    public static string ETime = string.Empty;
    public static string Date_ime = string.Empty;
    public static string StartTime = string.Empty;
    public static string EndTime = string.Empty;
    public static string startDate = string.Empty;
    public static string endDate = string.Empty;
    public static bool IsnewPlay = true;


    int Start_H, Start_M, Start_S;
    public void CreatedData(DateTime Day1, DateTime Day2)
    {
        if (DateScrollConten.childCount > 0)
        {
            for (int i = DateScrollConten.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(DateScrollConten.GetChild(i).gameObject);
            }
        }



        Log.Error("Day1:" + Day1 + " Day2:" + Day2 + " Day1.CompareTo(Day2):" + Day1.CompareTo(Day2));

        startDate = Day1.ToString("yyyyMMdd");
        endDate = Day2.ToString("yyyyMMdd");
        _startDay.text = Day1.ToString("yyyy-MM-dd");
        _endDay.text = Day2.ToString("yyyy-MM-dd");
        TimeSpan difference = Day2 - Day1;
        int days = difference.Days;
        _dateCreatedScroll.GetComponent<DatePageTurn>().OnInitRollScroll((days + 5 - 1) / 5, 0);
        while (Day1.CompareTo(Day2) <= 0)
        {
            GameObject DateBtn = Resources.Load<GameObject>("DateBtn");
            DateBtn = Instantiate(DateBtn);
            DateBtn.transform.SetParent(_dateCreatedScroll.transform.GetChild(0).GetChild(0));
            DateBtn.GetComponent<Toggle>().group = DateBtn.transform.parent.GetComponent<ToggleGroup>();
            DateBtn.transform.localScale = Vector3.one;
            DateBtn.transform.GetChild(1).GetComponent<TMP_Text>().text = Day1.ToString("MM-dd");
            DateBtn.name = Day1.ToString("MMdd");
            DateBtn.transform.GetChild(1).name = Day1.ToString("yyyy");
            // Log.Debug(Day1.ToString("yyyy-MM-dd"));
            Day1 = Day1.AddDays(1);
        }

        InputField S_HInput = _zcalendar.transform.Find("bak/Starttimebg/TimeBtn/HINput").GetComponent<InputField>();
        InputField S_MInput = _zcalendar.transform.Find("bak/Starttimebg/TimeBtn/MINput").GetComponent<InputField>();
        InputField S_SInput = _zcalendar.transform.Find("bak/Starttimebg/TimeBtn/SINput").GetComponent<InputField>();


        InputField E_HInput = _zcalendar.transform.Find("bak/Endtimebg/TimeBtn/HINput").GetComponent<InputField>();
        InputField E_MInput = _zcalendar.transform.Find("bak/Endtimebg/TimeBtn/MINput").GetComponent<InputField>();
        InputField E_SInput = _zcalendar.transform.Find("bak/Endtimebg/TimeBtn/SINput").GetComponent<InputField>();


        Start_H = int.Parse(S_HInput.text);
        Start_M = int.Parse(S_MInput.text);
        Start_S = int.Parse(S_SInput.text);

        int End_H, End_M, End_S;
        End_H = int.Parse(E_HInput.text);
        End_M = int.Parse(E_MInput.text);
        End_S = int.Parse(E_SInput.text);

        STime = S_HInput.text.Trim() + S_MInput.text.Trim() + S_SInput.text.Trim();
        ETime = E_HInput.text.Trim() + E_MInput.text.Trim() + E_SInput.text.Trim();
        TimeSlider.SliderTime = STime;

        TimeSlider.Ins.timeSlider.value = Start_H * 3600 + Start_M * 60 + Start_S;

        StartTime = startDate + STime.Trim();
        EndTime = endDate + ETime.Trim();
        //Log.Debug("选中的时间段:" + StartTime + " - " + EndTime);
        if (!JudgeTime(int.Parse(S_HInput.text.Trim()), int.Parse(S_MInput.text.Trim()), int.Parse(S_SInput.text.Trim()), int.Parse(E_HInput.text.Trim()), int.Parse(E_MInput.text.Trim()), int.Parse(E_SInput.text.Trim())))
            GameStart.Instance.ShowTip("日期选择错误:开始时间晚于截止时间!");

      
        if (startDate.Trim() == System.DateTime.Now.ToString("yyyyMMdd")&& endDate.Trim() == System.DateTime.Now.ToString("yyyyMMdd")) 
        {
            //Log.Error("选择的是当天");
            //Log.Error("当前时间:" + System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));

            int nowH =int.Parse(  System.DateTime.Now.ToString("HH"));
            int nowM= int.Parse(System.DateTime.Now.ToString("mm"));
            int nowS = int.Parse(System.DateTime.Now.ToString("ss"));

            if (!JudgeTime( int.Parse(E_HInput.text.Trim()), int.Parse(E_MInput.text.Trim()), int.Parse(E_SInput.text.Trim()),nowH,nowM,nowS))
                GameStart.Instance.ShowTip("日期选择错误:时间不能晚于当前时间!");



        }
        _zcalendar.GetComponent<ZCalendar>().Hide();
        TimeSlider.Ins.Changed(TimeSlider.Ins.timeSlider.value);
        DateBtn.Ins.BindToggle();








    }





    private void resetTime()
    {


        TimeSlider.Ins.timeSlider.value = Start_H * 3600 + Start_M * 60 + Start_S;
        TimeSlider.Ins.Changed(TimeSlider.Ins.timeSlider.value);

    }



    bool JudgeTime(int Shour, int SMin, int SSenc, int Ehour, int EMin, int ESenc)
    {
        Log.Debug("Shour:" + Shour + " Ehour: " + Ehour);
        if (Shour > Ehour)
            return false;
        else if (Shour == Ehour && SMin > EMin)
            return false;
        else if (Shour == Ehour && SMin == EMin && SSenc > ESenc)
            return false;
        else
            return true;

    }

    /// <summary>
    /// 点击打开窗口分割列表
    /// </summary>
    void ShowVideoNum()
    {
        if (_segmentationBG.activeInHierarchy)
        {
            _segmentationBG.SetActive(false);
        }
        else
        {
            _segmentationBG.SetActive(true);
        }
    }
    /// <summary>
    /// 选择了窗口显示几个回放区域
    /// </summary>
    /// <param name="num"></param>
    /// <param name="ChooseView"></param>
    public void Segmentation(int num, int ChooseView)
    {
        isFull = false;
        if (_videoArea.transform.parent != _rightPage.transform)
        {
            _videoArea.transform.SetParent(_rightPage.transform);
            _videoArea.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth, VideoAreaHeight);
            _videoArea.transform.SetAsFirstSibling();
        }
        _segmentationBG.SetActive(false);
        if (_videoArea.transform.childCount > 0)
        {
            for (int i = 0; i < _videoArea.transform.childCount; i++)
            {
                Destroy(_videoArea.transform.GetChild(i).gameObject);
            }
        }
        for (int i = 0; i < num; i++)
        {
            GameObject view = Resources.Load<GameObject>("View");
            view = Instantiate(view);
            view.GetComponent<Toggle>().group = _videoArea.GetComponent<ToggleGroup>();
            view.transform.SetParent(_videoArea.transform);
            view.transform.localScale = Vector3.one;
            view.name = "View" + i;
            view.SetActive(false);
        }
        switch (ChooseView)
        {
            case 1:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth, VideoAreaHeight);
                _videoArea.GetComponent<GridLayoutGroup>().constraintCount = 1;
                _videoArea.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                _videoArea.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                _videoArea.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                _videoArea.GetComponent<RectTransform>().localPosition = new Vector3(0, -525, 0);
                OpenViewArea(_videoArea.transform);
                ChooseViewNum = 1;
                break;
            case 4:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 2, VideoAreaHeight / 2);
                _videoArea.GetComponent<GridLayoutGroup>().constraintCount = 2;
                _videoArea.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                _videoArea.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                _videoArea.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                _videoArea.GetComponent<RectTransform>().localPosition = new Vector3(0, -525, 0);
                OpenViewArea(_videoArea.transform);
                ChooseViewNum = 4;
                break;
            case 6:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 3, VideoAreaHeight / 3);
                _videoArea.GetComponent<GridLayoutGroup>().constraintCount = 3;
                _videoArea.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                _videoArea.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                _videoArea.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                _videoArea.GetComponent<RectTransform>().localPosition = new Vector3(0, -525, 0);
                Timer.Register(0.01f, () =>
                {
                    _videoArea.transform.Find("View0").SetAsLastSibling();
                    _videoArea.transform.Find("View0").gameObject.AddComponent<LayoutElement>();
                    _videoArea.transform.Find("View0").GetComponent<LayoutElement>().ignoreLayout = true;
                    _videoArea.transform.Find("View0").GetComponent<RectTransform>().sizeDelta
= new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 3 * 2 + 2, _videoArea.GetComponent<RectTransform>().sizeDelta.y / 3 * 2 + 2);
                    _videoArea.transform.Find("View0").GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                    _videoArea.transform.Find("View0").GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                    _videoArea.transform.Find("View0").GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                    _videoArea.transform.Find("View0").GetComponent<RectTransform>().localPosition = new Vector3(0, 432, 0);
                    _videoArea.transform.Find("View1").GetComponent<RawImage>().enabled = false;
                    _videoArea.transform.Find("View2").GetComponent<RawImage>().enabled = false;
                    _videoArea.transform.Find("View4").GetComponent<RawImage>().enabled = false;
                    _videoArea.transform.Find("View5").GetComponent<RawImage>().enabled = false;
                    _videoArea.transform.Find("View1").GetComponent<Toggle>().interactable = false;
                    _videoArea.transform.Find("View2").GetComponent<Toggle>().interactable = false;
                    _videoArea.transform.Find("View4").GetComponent<Toggle>().interactable = false;
                    _videoArea.transform.Find("View5").GetComponent<Toggle>().interactable = false;
                    OpenViewArea(_videoArea.transform);
                });
                ChooseViewNum = 6;
                break;
            case 8:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 4, VideoAreaHeight / 4);
                _videoArea.GetComponent<GridLayoutGroup>().constraintCount = 4;
                _videoArea.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                _videoArea.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                _videoArea.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                _videoArea.GetComponent<RectTransform>().localPosition = new Vector3(0, -525, 0);
                Timer.Register(0.01f, () =>
                {
                    _videoArea.transform.Find("View0").SetAsLastSibling();
                    _videoArea.transform.Find("View0").gameObject.AddComponent<LayoutElement>();
                    _videoArea.transform.Find("View0").GetComponent<LayoutElement>().ignoreLayout = true;
                    _videoArea.transform.Find("View0").GetComponent<RectTransform>().sizeDelta
= new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 4 * 3 + 4, _videoArea.GetComponent<RectTransform>().sizeDelta.y / 4 * 3 + 4);
                    _videoArea.transform.Find("View0").GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                    _videoArea.transform.Find("View0").GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                    _videoArea.transform.Find("View0").GetComponent<RectTransform>().localPosition = new Vector3(0, 433, 0);
                    _videoArea.transform.Find("View1").GetComponent<RawImage>().enabled = false;
                    _videoArea.transform.Find("View2").GetComponent<RawImage>().enabled = false;
                    _videoArea.transform.Find("View3").GetComponent<RawImage>().enabled = false;
                    _videoArea.transform.Find("View5").GetComponent<RawImage>().enabled = false;
                    _videoArea.transform.Find("View6").GetComponent<RawImage>().enabled = false;
                    _videoArea.transform.Find("View7").GetComponent<RawImage>().enabled = false;
                    _videoArea.transform.Find("View9").GetComponent<RawImage>().enabled = false;
                    _videoArea.transform.Find("View10").GetComponent<RawImage>().enabled = false;
                    _videoArea.transform.Find("View11").GetComponent<RawImage>().enabled = false;
                    _videoArea.transform.Find("View1").GetComponent<Toggle>().enabled = false;
                    _videoArea.transform.Find("View2").GetComponent<Toggle>().enabled = false;
                    _videoArea.transform.Find("View3").GetComponent<Toggle>().enabled = false;
                    _videoArea.transform.Find("View5").GetComponent<Toggle>().enabled = false;
                    _videoArea.transform.Find("View6").GetComponent<Toggle>().enabled = false;
                    _videoArea.transform.Find("View7").GetComponent<Toggle>().enabled = false;
                    _videoArea.transform.Find("View9").GetComponent<Toggle>().enabled = false;
                    _videoArea.transform.Find("View10").GetComponent<Toggle>().enabled = false;
                    _videoArea.transform.Find("View11").GetComponent<Toggle>().enabled = false;
                    OpenViewArea(_videoArea.transform);
                    ChooseViewNum = 8;
                });
                break;
            case 9:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 3, VideoAreaHeight / 3);
                _videoArea.GetComponent<GridLayoutGroup>().constraintCount = 3;
                _videoArea.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                _videoArea.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                _videoArea.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                _videoArea.GetComponent<RectTransform>().localPosition = new Vector3(0, -525, 0);
                OpenViewArea(_videoArea.transform);
                ChooseViewNum = 9;
                break;
            case 13:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 4, VideoAreaHeight / 4);
                _videoArea.GetComponent<GridLayoutGroup>().constraintCount = 4;
                _videoArea.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                _videoArea.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                _videoArea.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                _videoArea.GetComponent<RectTransform>().localPosition = new Vector3(0, -525, 0);
                Timer.Register(0.01f, () =>
               {
                   _videoArea.transform.Find("View0").SetAsLastSibling();
                   _videoArea.transform.Find("View0").gameObject.AddComponent<LayoutElement>();
               
                   _videoArea.transform.Find("View0").GetComponent<LayoutElement>().ignoreLayout = true;
                   _videoArea.transform.Find("View0").GetComponent<RectTransform>().sizeDelta
                 = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 4 * 2 + 2, _videoArea.GetComponent<RectTransform>().sizeDelta.y / 4 * 2 + 2);
                   _videoArea.transform.Find("View0").GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                   _videoArea.transform.Find("View0").GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                   _videoArea.transform.Find("View0").GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                   _videoArea.transform.Find("View0").GetComponent<RectTransform>().localPosition = new Vector3(_videoArea.GetComponent<LayoutElement>().preferredWidth / 2, 0, 0);
                   _videoArea.transform.Find("View6").GetComponent<RawImage>().enabled = false;
                   _videoArea.transform.Find("View7").GetComponent<RawImage>().enabled = false;
                   _videoArea.transform.Find("View10").GetComponent<RawImage>().enabled = false;
                   _videoArea.transform.Find("View11").GetComponent<RawImage>().enabled = false;
                   _videoArea.transform.Find("View6").GetComponent<Toggle>().enabled = false;
                   _videoArea.transform.Find("View7").GetComponent<Toggle>().enabled = false;
                   _videoArea.transform.Find("View10").GetComponent<Toggle>().enabled = false;
                   _videoArea.transform.Find("View11").GetComponent<Toggle>().enabled = false;
                   OpenViewArea(_videoArea.transform);
               });
                ChooseViewNum = 13;
                break;
            case 16:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize = new Vector2(_videoArea.GetComponent<LayoutElement>().preferredWidth / 4, VideoAreaHeight / 4);
                _videoArea.GetComponent<GridLayoutGroup>().constraintCount = 4;
                _videoArea.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                _videoArea.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                _videoArea.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                _videoArea.GetComponent<RectTransform>().localPosition = new Vector3(0, -525, 0);
                OpenViewArea(_videoArea.transform);
                ChooseViewNum = 16;
                break;
        }
        Timer.Register(0.1f, () =>
        {
            toggles = _videoArea.transform.GetComponentsInChildren<Toggle>();
            //给toggle添加事件
            for (int i = 0; i < toggles.Length; i++)
            {
                print(toggles[i]);
                //这一步是必须记录的，用来区分那个toggle
                int K = i;
                //toggles[K].onValueChanged.AddListener((bool value) => SetVideoArewToggle(value, K));
            }
        });

    }
    /// <summary>
    /// 选择区域
    /// </summary>
    /// <param name="value"></param>
    /// <param name="k"></param>
    //private void SetVideoArewToggle(bool value, int k)
    //{
    //    if (k == 0 && value)
    //    {
    //        string rtsp = "rtsp://user:password@camera-host:554/h265/ch1/main/av_stream";
    //        toggles[k].transform.GetChild(0).GetComponent<VideoPlayBack>().AddVideo(rtsp);
    //        Log.Debug("第一个toggle");
    //    }
    //    if (k == 1 && value)
    //    {

    //        toggles[k].transform.GetChild(0).GetComponent<VideoPlayBack>().AddVideo(RtspUrl);
    //        Log.Debug("第二个toggle");
    //    }
    //    if (k == 2 && value)
    //    {
    //        toggles[k].transform.GetChild(0).GetComponent<VideoPlayBack>().AddVideo(RtspUrl);
    //        Log.Debug("第三个toggle");
    //    }
    //    if (k == 3 && value)
    //    {
    //        toggles[k].transform.GetChild(0).GetComponent<VideoPlayBack>().AddVideo(RtspUrl);

    //        Log.Debug("第四个toggle");
    //    }
    //}
    /// <summary>
    /// 把隐藏的VideoArea下的子物体全部打开
    /// </summary>
    /// <param name="videoAreaTransform"></param>
    void OpenViewArea(Transform videoAreaTransform)
    {
        for (int i = 0; i < videoAreaTransform.childCount; i++)
        {
            videoAreaTransform.GetChild(i).gameObject.SetActive(true);
        }
    }
    /// <summary>
    /// 全屏显示当前监控回放画面
    /// </summary>
    void FullScreenViewArea()
    {
        ScreenResolution.Ins.ChangeScreenSizeHide(1920, 1080, 960, 540);
        //UIController.Ins.ClosePageUI();
        _videoArea.transform.SetParent(this.gameObject.transform.parent);
        _videoArea.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);
        _videoArea.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        _videoArea.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        _videoArea.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        _videoArea.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        isFull = true;
        switch (ChooseViewNum)
        {
            case 1:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize = new Vector2(1920, 1080);
                _videoArea.GetComponent<GridLayoutGroup>().constraintCount = 1;
                break;
            case 4:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize = new Vector2(1920 / 2, 1080 / 2);
                _videoArea.GetComponent<GridLayoutGroup>().constraintCount = 2;
                break;
            case 6:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize = new Vector2(1920 / 3, 1080 / 3);
                _videoArea.GetComponent<GridLayoutGroup>().constraintCount = 3;
                _videoArea.transform.Find("View0").GetComponent<RectTransform>().sizeDelta = new Vector2(1920 / 3 * 2 + 2, 1080 / 3 * 2 + 2);
                break;
            case 8:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize = new Vector2(1920 / 4, 1080 / 4);
                _videoArea.GetComponent<GridLayoutGroup>().constraintCount = 4;
                _videoArea.transform.Find("View0").GetComponent<RectTransform>().sizeDelta = new Vector2(1920 / 4 * 3 + 4, 1080 / 4 * 3 + 4);

                break;
            case 9:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize = new Vector2(1920 / 3, 1080 / 3);
                _videoArea.GetComponent<GridLayoutGroup>().constraintCount = 3;
                break;
            case 13:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize = new Vector2(1920 / 4, 1080 / 4);
                _videoArea.GetComponent<GridLayoutGroup>().constraintCount = 4;
                _videoArea.transform.Find("View0").GetComponent<RectTransform>().sizeDelta = new Vector2(1920 / 4 * 2 + 2, 1080 / 4 * 2 + 2);

                break;
            case 16:
                _videoArea.GetComponent<GridLayoutGroup>().cellSize = new Vector2(1920 / 4, 1080 / 4);
                _videoArea.GetComponent<GridLayoutGroup>().constraintCount = 4;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 清除显示区域摄像头画面
    /// </summary>
    void ClearVideoArew()
    {
        if (_videoArea.transform.childCount > 0)
        {
            for (int i = 0; i < _videoArea.transform.childCount; i++)
            {
                //_videoArea.transform.GetChild(i).GetComponent<VideoPlayBack>().StopVideo();
            }
        }

    }
}
