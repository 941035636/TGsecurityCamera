using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using zFramework.Media;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;
using UnityTimer;
using UnityEngine.Experimental.PlayerLoop;
using UMP;
using static ManManagentUi;
using SuperTreeView;
using System.Collections;
using System.Diagnostics.Eventing.Reader;
using System.Data;
using System.IO;

public class MainPreviewUi : Window
{
    public bool LoadDataBYServer = true;
    public static bool IsInitCamera = false;
    public MainPreviewPanel _mainPanel;
    private int temp, value = 0;
    #region 左侧 CameraGroup
    List<GameObject> Cameralist = new List<GameObject>();
    Dictionary<string, GameObject> CameraGroupDic = new Dictionary<string, GameObject>();
    #endregion
    #region 右侧VideoRender
    static List<GameObject> videoRenderList = new List<GameObject>();
    #endregion
    bool IsHFullScreen = false;
    Transform Container6, Container8, Container13, Container32;
    public static int ChoiceIndex = 0;//选择的videorender索引
    private static GameObject cameraPlayer;
    static bool IsMax = false;
    private string InputStrcamename = string.Empty;

    private static bool IsConfiguredCamera(SecurityCamera camera)
    {
        return camera != null && IsConfiguredHost(camera.host);
    }

    private static bool IsConfiguredHost(string host)
    {
        return !string.IsNullOrEmpty(host) && host != "1.1.1.1:1";
    }

    private static void ConfigurePreviewTreeLabel(ItemScript item, string label)
    {
        if (item == null || item.labelText == null)
        {
            return;
        }

        item.labelText.text = label ?? string.Empty;
        item.labelText.interactable = false;
        if (item.labelText.targetGraphic != null)
        {
            item.labelText.targetGraphic.raycastTarget = false;
        }
        if (item.labelText.textComponent != null)
        {
            item.labelText.textComponent.resizeTextForBestFit = false;
            item.labelText.textComponent.fontSize = 14;
            item.labelText.textComponent.raycastTarget = false;
        }
    }
    public override void Awake(params object[] paralist)
    {


        _mainPanel = GameObject.GetComponent<MainPreviewPanel>();
        Container6 = _mainPanel.Grid.transform.parent.Find("Container6");
        Container8 = _mainPanel.Grid.transform.parent.Find("Container8");
        Container13 = _mainPanel.Grid.transform.parent.Find("Container13");
        Container32 = _mainPanel.Grid.transform.parent.Find("Container32");
        Containlistener(Container6);
        Containlistener(Container8);
        Containlistener(Container13);
        Containlistener(Container32);
        AddPlayViewListener();

        //轮训控制
        //Patrol();
        //云台控制
        AddButtonClickListener(_mainPanel.upExpandbtn, PtzImagEnable);
        AddButtonClickListener(_mainPanel.downExpandbtn, PtzImagDisEnable);
        AddButtonClickListener(_mainPanel.PlayfaceImage.Find("closebtn").GetComponent<Button>(), closePlayfaceImageLis);

        ReadPrefsData();
        Init();





    }


    private void Init()
    {
        EventCenter.addlistener<NVRInformation>(Eventdefine.CameraPlayEvent, CameraPlayEvent);

        AddToggleClickListener(_mainPanel.res_tog, Res_Tog);

        AddToggleClickListener(_mainPanel.Shutdown_tog, AddShutdownListener);
        AddToggleClickListener(_mainPanel.hfullscreenTog, HFullScreenListener);
        AddToggleClickListener(_mainPanel.patrol_tog, PatrolScrollviewListener);
        AddButtonClickListener(_mainPanel.searchBtn, SearchCamera);
        _mainPanel.res_tog.isOn = true;
        _mainPanel.search_input.onEndEdit.AddListener((str) =>
        {

            InputStrcamename = str;

        });

        _mainPanel.clearBtn.onClick.AddListener(() =>
        {
            if (_mainPanel.MainPreviewTreeView.transform.childCount != 0)
            {
                for (int i = _mainPanel.MainPreviewTreeView.transform.childCount - 1; i >= 0; i--)
                {
                    int temp = i;
                    if (_mainPanel.MainPreviewTreeView.transform.GetChild(temp).GetComponent<TreeViewItem>() != null && _mainPanel.MainPreviewTreeView.transform.GetChild(temp).name == "ItemPrefab1(Clone)")
                        TreeManagerPreview.GetInstance().OnDeleteBtnClicked(_mainPanel.MainPreviewTreeView.transform.GetChild(temp).GetComponent<TreeViewItem>());
                }

            }
            _mainPanel.search_input.text = "";
            InputStrcamename = "";
            InitCameraGroup(outlineInfoList);


            _mainPanel.searchBtn.gameObject.SetActive(true);
            _mainPanel.clearBtn.gameObject.SetActive(false);

        });
    }


    #region 轮询
    void PtzImagEnable()
    {
        _mainPanel.PTZImg.transform.DOScale(Vector3.one, 0.2f);
        _mainPanel.PTZImg.transform.DOLocalMoveY(222.5f, 0.2f);
    }
    void PtzImagDisEnable()
    {
        _mainPanel.PTZImg.transform.DOScale(Vector3.zero, 0.2f);
        _mainPanel.PTZImg.transform.DOLocalMoveY(0, 0.2f);
    }

    void PatrolScrollviewListener(bool ison)
    {
        _mainPanel.patrolscrollview.SetActive(ison);

        Image patroltog = _mainPanel.patrol_tog.transform.Find("Background").GetComponent<Image>();
        if (ison)
        {
            patroltog.sprite = ResourceManager.Instance.LoadResource<Sprite>(ConStr.VIDEOPREVIEWUI + "轮巡-1.png");
            patroltog.SetNativeSize();

        }
        else
        {
            patroltog.sprite = ResourceManager.Instance.LoadResource<Sprite>(ConStr.VIDEOPREVIEWUI + "轮巡-0.png");
            patroltog.SetNativeSize();
        }

    }
    void Patrol()
    {
        _mainPanel.patrolsgroup.transform.Find("show_tog").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
        {
            if (ison)
            {
                _mainPanel.patrolsgroup.transform.Find("show_tog").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/视频预览/放");
                instanPatrolCamera();
            }
            else
            {
                _mainPanel.patrolsgroup.transform.Find("show_tog").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/视频预览/收");
                SetPatrolCamerasDisactive(false);
            }
            //_mainPanel.patrolallCamera.gameObject.SetActive(ison);
            _mainPanel.patrolsgroup.transform.Find("show_tog").GetComponent<Image>().SetNativeSize();
        });

        //轮训播放
        _mainPanel.patrolsgroup.transform.Find("play_btn").GetComponent<Button>().onClick.AddListener(() =>
        {


        });



    }

    //实例化轮训分组
    void instanPatrolCamera()
    {
        if (_mainPanel.patrolsgroup.transform.parent.childCount == 1)
        {
            //
            if (LoadDataBYServer)
            {
                //HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/cam/info?equiptype=编码设备", ReadCameraCallback, false, false, false, null);
            }


        }
        else
        {
            SetPatrolCamerasDisactive(true);
        }


    }
    //请求设备信息数据结构
    [Serializable]
    public class EquipInfoArr
    {
        public List<NVRInformation> records;
        public int total;//总计多少条
        public int size;//一页多少条
        public int current;//第几页
        public List<object> orders;
        public bool optimizeCountSql;
        public bool searchCount;
        public object maxLimit;
        public string countId;
        public int pages;//一共多少页
        public EquipInfoArr(List<NVRInformation> arr)
        {
            this.records = arr;
        }
    }


    //根据服务器返回的设备容器初始化cameraitem
    void initCameraItemByServer(List<NVRInformation> nvrlist)
    {
        if (!IsInitCamera)
        {
            foreach (var item in nvrlist)
            {
                //initCameraItem(item, true);

                GameObject cameraObj = ObjectManager.Instance.InstantiateObject(ConStr.CAMERAPatrol, false, false);
                cameraObj.transform.SetParent(_mainPanel.patrolsgroup.transform.parent);
                cameraObj.transform.GetChild(0).GetComponent<TMP_Text>().text = item.cameraname;
                resetPrefab(cameraObj);
            }
            IsInitCamera = true;
        }



    }

    void SetPatrolCamerasDisactive(bool ison)
    {
        for (int i = 1; i < _mainPanel.patrolsgroup.transform.parent.childCount; i++)
        {
            _mainPanel.patrolsgroup.transform.parent.GetChild(i).gameObject.SetActive(ison);

        }
    }

    #endregion

    public override void OnShow(params object[] paralist)
    {

        CameraGroupDic.Clear();


        if (LoadDataBYServer)
        {
            GameStart.Instance.StartCoroutine(delayGetCameraGroup());
        }


    }

    IEnumerator delayGetCameraGroup()
    {
        yield return new WaitForEndOfFrame();

        GetCameraGroupDevs();
        yield return new WaitForEndOfFrame();
        _mainPanel.MainPreviewTreeView.transform.parent.parent.GetComponent<ScrollRect>().horizontalNormalizedPosition = 0.45f;
    }



    //点击异形单个播放窗口响应监听
    int libindex = 0;
    Vector2 OriginVec;
    Vector3 LocalPos;
    void Containlistener(Transform container)
    {
        for (int i = 0; i < container.childCount; i++)
        {


            int index = i;
            container.GetChild(index).Find("maskw").GetComponent<Text>().text = LoginManager.UserName;
            container.GetChild(index).GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            container.GetChild(index).GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
            {

                container.GetChild(index).Find("mask").gameObject.SetActive(ison);
                if (ison)
                {

                    ChoiceIndex = index;
                    cameraPlayer = container.GetChild(index).gameObject;
                    //Log.Error("选择了:" + ChoiceIndex + "播放窗口播放状态:" + cameraPlayer.GetComponent<SecurityCamera>().player.IsRealPlaying);
                }

            });
            container.GetChild(index).GetComponent<UIButtonDoubleClick>().OnDoubleClick.RemoveAllListeners();
            int choiceid = 0;

            container.GetChild(index).GetComponent<UIButtonDoubleClick>().OnDoubleClick.AddListener(() =>
            {

                Log.Debug("双击了:" + index);
                if (container.GetChild(index).GetComponent<RectTransform>().sizeDelta.x < 1557 && container.GetChild(index).GetComponent<RectTransform>().sizeDelta.y < 931)
                {

                    IsMax = true;
                    Log.Debug("放大:" + index + "IsMax:" + IsMax);
                    Log.Debug("放大:" + container.GetChild(index).GetComponent<SecurityCamera>().steamType);
                    choiceid = index;
                    if (IsConfiguredCamera(container.GetChild(choiceid).GetComponent<SecurityCamera>()))
                    {
                        Log.Debug("第几个播主流:" + choiceid);
                        CameraPlay(container.GetChild(choiceid).GetComponent<SecurityCamera>().host, container.GetChild(choiceid).GetComponent<SecurityCamera>().sdk, container.GetChild(choiceid).GetComponent<SecurityCamera>().channel, STREAM.MAIN, choiceid, container.GetChild(choiceid).GetComponent<SecurityCamera>().devid, container.GetChild(choiceid).GetComponent<SecurityCamera>().passward);
                    }
                    container.GetChild(index).transform.DOLocalMove(new Vector3(-0.5f, 0.5f, 0), 0.2f);
                    OriginVec = container.GetChild(index).GetComponent<RectTransform>().sizeDelta;
                    LocalPos = container.GetChild(index).GetComponent<Transform>().localPosition;
                    container.GetChild(index).GetComponent<RectTransform>().DOSizeDelta(new Vector2(1557, 931), 0.2f);
                    libindex = container.GetChild(index).transform.GetSiblingIndex();
                    container.GetChild(index).transform.SetAsLastSibling();
                    videoRenderList.RemoveAt(index);
                    index = container.childCount - 1;
                    videoRenderList.Add(container.GetChild(index).gameObject);
                }
                else
                {


                    container.GetChild(index).GetComponent<Transform>().localPosition = LocalPos;
                    container.GetChild(index).GetComponent<RectTransform>().sizeDelta = OriginVec;
                    container.GetChild(index).transform.SetSiblingIndex(libindex);
                    videoRenderList.RemoveAt(index);
                    index = libindex;
                    videoRenderList.Insert(libindex, container.GetChild(index).gameObject);
                    Log.Debug("缩小:" + libindex);
                    IsMax = false;
                    Timer.Register(0.2f, () =>
                    {
                        if (IsConfiguredCamera(container.GetChild(choiceid).GetComponent<SecurityCamera>()))
                            CameraPlay(container.GetChild(choiceid).GetComponent<SecurityCamera>().host, container.GetChild(choiceid).GetComponent<SecurityCamera>().sdk, container.GetChild(choiceid).GetComponent<SecurityCamera>().channel, STREAM.EXTRA, libindex, container.GetChild(choiceid).GetComponent<SecurityCamera>().devid, container.GetChild(choiceid).GetComponent<SecurityCamera>().passward);

                    });
                    //render_pre.transform.parent.GetComponent<GridLayoutGroup>().enabled = true;
                }
            });
            container.GetChild(index).GetComponent<UIButtonDoubleClick>().OnRightClick.RemoveAllListeners();
            container.GetChild(index).GetComponent<UIButtonDoubleClick>().OnRightClick.AddListener(() =>
            {

                Debug.LogError("鼠标右键点击!");
                if (render_preright != null && container.GetChild(index).transform.Find("mask").gameObject.activeInHierarchy && container.GetChild(index).transform.Find("VideoRender").GetComponent<VideoRenderer>().IsRendering)
                {
                    container.GetChild(index).transform.Find("mousepanel").gameObject.SetActive(true);
                    //render_preright.transform.Find("mousepanel").gameObject.SetActive(false);
                    //render_preright = container.GetChild(index);
                }
                else if (container.GetChild(index).transform.Find("mask").gameObject.activeInHierarchy && container.GetChild(index).transform.Find("VideoRender").GetComponent<VideoRenderer>().IsRendering)
                {
                    container.GetChild(index).transform.Find("mousepanel").gameObject.SetActive(true);
                    //render_preright = render_pre;
                }

            });
            //注释掉异形滚动
            //container.GetChild(index).GetComponent<UIButtonDoubleClick>().OnscrollWheel.RemoveAllListeners();
            //container.GetChild(index).GetComponent<UIButtonDoubleClick>().OnscrollWheel.AddListener(() =>
            //{
            //    float mouseCenter = Input.GetAxis("Mouse ScrollWheel");
            //    Log.Error("滚动了:" + mouseCenter); 123
            //    SetMouseChangeIamge(container.GetChild(choiceid).GetChild(0), mouseCenter);
            //});

            container.GetChild(index).transform.Find("mousepanel/facerec").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            container.GetChild(index).transform.Find("mousepanel/facerec").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
            {

                container.GetChild(index).transform.Find("mousepanel").gameObject.SetActive(false);
                Log.Debug("播放开启人脸识别算法的视频窗口");

                //向算法发送请求Rtsp协议


                if (container.GetChild(index).GetComponent<SecurityCamera>().devid != -1)
                {
                    FaceData data = new FaceData();
                    data.dtype = "face_rec";
                    data.state = 1;
                    data.uuid = container.GetChild(index).GetComponent<SecurityCamera>().devid.ToString();
                    if (container.GetChild(index).GetComponent<SecurityCamera>().sdk == SDKTYPE.HK)
                    {
                        //if (container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0].Contains(".26.")|| container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0].Contains(".10.")) 
                        //{
                        //    data.rtsp = "rtsp://admin:abcd1234@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/h264/ch1/main/av_stream";
                        //    facertsp = data.rtsp;
                        //}
                        //else 
                        //{
                        //    data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/h264/ch1/main/av_stream";
                        //    facertsp = data.rtsp;
                        //}
                        data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/h264/ch1/main/av_stream";
                        facertsp = data.rtsp;
                    }
                    else if (container.GetChild(index).GetComponent<SecurityCamera>().sdk == SDKTYPE.DH)
                    {
                        //data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/cam/realmonitor?channel=1&subtype=0";
                        data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/cam/realmonitor?channel=1&subtype=0";
                        facertsp = data.rtsp;
                    }
                    else if (container.GetChild(index).GetComponent<SecurityCamera>().sdk == SDKTYPE.YS)
                    {
                        //data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/media/video1";
                        data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/media/video1";
                        facertsp = data.rtsp;
                    }
                    ReqFaceSevData reqFaceSevData = new ReqFaceSevData();
                    reqFaceSevData.requestMgrHostIp = "192.168.110.2";
                    reqFaceSevData.requestMgrHostPort = "77111";
                    reqFaceSevData.requestFaceRecInfoList.Add(data);
                    string reqstr = JsonUtility.ToJson(reqFaceSevData, true);
                    Log.Debug("向算法服务发送开启人脸检测算法：" + reqstr);
                    HttpNetManager.GetInstance().SendDataStr(AppRuntimeConfig.Settings.faceServiceUrl, RequestFaceSevCallback, true, true, false, reqstr);
                }


            });
            container.GetChild(index).transform.Find("mousepanel/peoplegroup").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            container.GetChild(index).transform.Find("mousepanel/peoplegroup").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
            {

                container.GetChild(index).transform.Find("mousepanel").gameObject.SetActive(false);
                Log.Debug("播放开启人员聚集算法的视频窗口");
                _mainPanel.PlayfaceImage.gameObject.SetActive(true);
                _mainPanel.loadingani.gameObject.SetActive(true);
                _mainPanel.loadingani.Play("loadani");
                //向算法发送请求Rtsp协议



                if (container.GetChild(index).GetComponent<SecurityCamera>().devid != -1)
                {
                    FaceData data = new FaceData();
                    data.dtype = "person_group";
                    data.state = 1;
                    data.uuid = container.GetChild(index).GetComponent<SecurityCamera>().devid.ToString();
                    if (container.GetChild(index).GetComponent<SecurityCamera>().sdk == SDKTYPE.HK)
                    {
                        data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/h264/ch1/main/av_stream";
                        facertsp = data.rtsp;
                    }
                    else if (container.GetChild(index).GetComponent<SecurityCamera>().sdk == SDKTYPE.DH)
                    {
                        data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/cam/realmonitor?channel=1&subtype=0";
                        facertsp = data.rtsp;
                    }
                    else if (container.GetChild(index).GetComponent<SecurityCamera>().sdk == SDKTYPE.YS)
                    {
                        data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/media/video1";
                        facertsp = data.rtsp;
                    }
                    ReqFaceSevData reqFaceSevData = new ReqFaceSevData();
                    reqFaceSevData.requestMgrHostIp = "192.168.110.2";
                    reqFaceSevData.requestMgrHostPort = "77111";
                    reqFaceSevData.requestFaceRecInfoList.Add(data);
                    string reqstr = JsonUtility.ToJson(reqFaceSevData, true);
                    Log.Debug("向算法服务发送开启人员聚集算法：" + reqstr);
                    HttpNetManager.GetInstance().SendDataStr(AppRuntimeConfig.Settings.faceServiceUrl, RequestpersonGroupCallback, true, true, false, reqstr);
                }


            });
            container.GetChild(index).transform.Find("mousepanel/peopleru").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            container.GetChild(index).transform.Find("mousepanel/peopleru").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
            {

                container.GetChild(index).transform.Find("mousepanel").gameObject.SetActive(false);
                Log.Debug("播放开启人员闯入算法的视频窗口");
                _mainPanel.PlayfaceImage.gameObject.SetActive(true);
                _mainPanel.loadingani.gameObject.SetActive(true);
                _mainPanel.loadingani.Play("loadani");
                //向算法发送请求Rtsp协议


                if (container.GetChild(index).GetComponent<SecurityCamera>().devid != -1)
                {
                    FaceData data = new FaceData();
                    data.dtype = "person_brust";
                    data.state = 1;
                    data.uuid = container.GetChild(index).GetComponent<SecurityCamera>().devid.ToString();
                    if (container.GetChild(index).GetComponent<SecurityCamera>().sdk == SDKTYPE.HK)
                    {
                        data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/h264/ch1/main/av_stream";
                        facertsp = data.rtsp;
                    }
                    else if (container.GetChild(index).GetComponent<SecurityCamera>().sdk == SDKTYPE.DH)
                    {
                        data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/cam/realmonitor?channel=1&subtype=0";
                        facertsp = data.rtsp;
                    }
                    else if (container.GetChild(index).GetComponent<SecurityCamera>().sdk == SDKTYPE.YS)
                    {
                        data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/media/video1";
                        facertsp = data.rtsp;
                    }
                    ReqFaceSevData reqFaceSevData = new ReqFaceSevData();
                    reqFaceSevData.requestMgrHostIp = "192.168.110.2";
                    reqFaceSevData.requestMgrHostPort = "77111";
                    reqFaceSevData.requestFaceRecInfoList.Add(data);
                    string reqstr = JsonUtility.ToJson(reqFaceSevData, true);
                    Log.Debug("向算法服务发送开启人员闯入检测算法：" + reqstr);
                    HttpNetManager.GetInstance().SendDataStr(AppRuntimeConfig.Settings.faceServiceUrl, RequestpersonBrustCallback, true, true, false, reqstr);
                }


            });

            container.GetChild(index).transform.Find("mousepanel/peoplefall").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            container.GetChild(index).transform.Find("mousepanel/peoplefall").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
            {

                container.GetChild(index).transform.Find("mousepanel").gameObject.SetActive(false);
                Log.Debug("播放开启人员跌倒的视频窗口");
                _mainPanel.PlayfaceImage.gameObject.SetActive(true);
                _mainPanel.loadingani.gameObject.SetActive(true);
                _mainPanel.loadingani.Play("loadani");
                //向算法发送请求Rtsp协议


                if (container.GetChild(index).GetComponent<SecurityCamera>().devid != -1)
                {
                    FaceData data = new FaceData();
                    data.dtype = "person_fall";
                    data.state = 1;
                    data.uuid = container.GetChild(index).GetComponent<SecurityCamera>().devid.ToString();
                    if (container.GetChild(index).GetComponent<SecurityCamera>().sdk == SDKTYPE.HK)
                    {
                        data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/h264/ch1/main/av_stream";
                        facertsp = data.rtsp;
                    }
                    else if (container.GetChild(index).GetComponent<SecurityCamera>().sdk == SDKTYPE.DH)
                    {
                        data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/cam/realmonitor?channel=1&subtype=0";
                        facertsp = data.rtsp;
                    }
                    else if (container.GetChild(index).GetComponent<SecurityCamera>().sdk == SDKTYPE.YS)
                    {
                        data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/media/video1";
                        facertsp = data.rtsp;
                    }
                    ReqFaceSevData reqFaceSevData = new ReqFaceSevData();
                    reqFaceSevData.requestMgrHostIp = "192.168.110.2";
                    reqFaceSevData.requestMgrHostPort = "77111";
                    reqFaceSevData.requestFaceRecInfoList.Add(data);
                    string reqstr = JsonUtility.ToJson(reqFaceSevData, true);
                    Log.Debug("向算法服务发送开启人员跌倒检测算法：" + reqstr);
                    HttpNetManager.GetInstance().SendDataStr(AppRuntimeConfig.Settings.faceServiceUrl, RequestpersonFallCallback, true, true, false, reqstr);
                }


            });

            container.GetChild(index).transform.Find("mousepanel/carplate").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            container.GetChild(index).transform.Find("mousepanel/carplate").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
            {

                container.GetChild(index).transform.Find("mousepanel").gameObject.SetActive(false);
                Log.Debug("播放开启车牌检测的视频窗口");
                _mainPanel.PlayfaceImage.gameObject.SetActive(true);
                _mainPanel.loadingani.gameObject.SetActive(true);
                _mainPanel.loadingani.Play("loadani");
                //向算法发送请求Rtsp协议


                if (container.GetChild(index).GetComponent<SecurityCamera>().devid != -1)
                {
                    FaceData data = new FaceData();
                    data.dtype = "car_plate";
                    data.state = 1;
                    data.uuid = container.GetChild(index).GetComponent<SecurityCamera>().devid.ToString();
                    if (container.GetChild(index).GetComponent<SecurityCamera>().sdk == SDKTYPE.HK)
                    {
                        data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/h264/ch1/main/av_stream";
                        facertsp = data.rtsp;
                    }
                    else if (container.GetChild(index).GetComponent<SecurityCamera>().sdk == SDKTYPE.DH)
                    {
                        data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/cam/realmonitor?channel=1&subtype=0";
                        facertsp = data.rtsp;
                    }
                    else if (container.GetChild(index).GetComponent<SecurityCamera>().sdk == SDKTYPE.YS)
                    {
                        data.rtsp = "rtsp://admin:" + container.GetChild(index).GetComponent<SecurityCamera>().passward + "@" + container.GetChild(index).GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/media/video1";
                        facertsp = data.rtsp;
                    }
                    ReqFaceSevData reqFaceSevData = new ReqFaceSevData();
                    reqFaceSevData.requestMgrHostIp = "192.168.110.2";
                    reqFaceSevData.requestMgrHostPort = "77111";
                    reqFaceSevData.requestFaceRecInfoList.Add(data);
                    string reqstr = JsonUtility.ToJson(reqFaceSevData, true);
                    Log.Debug("向算法服务发送开启车牌检测算法：" + reqstr);
                    HttpNetManager.GetInstance().SendDataStr(AppRuntimeConfig.Settings.faceServiceUrl, RequestcarPlateCallback, true, true, false, reqstr);
                }


            });



        }
    }

    void Res_Tog(bool ison)
    {
        Image restogbg = _mainPanel.res_tog.transform.Find("Background").GetComponent<Image>();
        if (ison)
        {
            restogbg.sprite = ResourceManager.Instance.LoadResource<Sprite>(ConStr.VIDEOPREVIEWUI + "资源-1.png");
            restogbg.SetNativeSize();

        }
        else
        {
            restogbg.sprite = ResourceManager.Instance.LoadResource<Sprite>(ConStr.VIDEOPREVIEWUI + "资源-0.png");
            restogbg.SetNativeSize();
        }
        _mainPanel.ResScrollview.gameObject.SetActive(ison);



    }







    /// <summary>
    /// 获取监控区域分组设备信息
    /// </summary>



    List<OutlineInfo> outlineInfoList = new List<OutlineInfo>();
    public void GetCameraGroupDevs()
    {
        outlineInfoList.Clear();

        if (_mainPanel.MainPreviewTreeView.transform.childCount != 0)
        {
            for (int i = _mainPanel.MainPreviewTreeView.transform.childCount - 1; i >= 0; i--)
            {
                int temp = i;
                if (_mainPanel.MainPreviewTreeView.transform.GetChild(temp).GetComponent<TreeViewItem>() != null && _mainPanel.MainPreviewTreeView.transform.GetChild(temp).name == "ItemPrefab1(Clone)")
                    TreeManagerPreview.GetInstance().OnDeleteBtnClicked(_mainPanel.MainPreviewTreeView.transform.GetChild(temp).GetComponent<TreeViewItem>());
            }

        }

        if (LoginManager.Ins.PreviewAuthList != null && LoginManager.Ins.PreviewAuthList.Count > 0)
        {  //过滤权限信息或者从服务器回来的就是经过权限过滤的
            for (int i = 0; i < LoginManager.Ins.PreviewAuthList.Count; i++)
            {

                ProductDoorTreeDic(LoginManager.Ins.PreviewAuthList[i]);
                Log.Debug("请求的该角色权限下的监控分组" + i + ":" + JsonUtility.ToJson(LoginManager.Ins.PreviewAuthList[i], true));
            }



            InitCameraGroup(outlineInfoList);


        }
        else
        {
            GameStart.Instance.ShowTip("该用户没有监控设备预览权限！");
        }



    }
    /// <summary>
    /// 递归拆分层及目录架构
    /// </summary>
    /// <param name="devs"></param>
    //private void ProductDoorTreeDic(GetAreaGroupDevs devs)
    //{

    //    OutlineInfo outline = new OutlineInfo();
    //    outline.OutlineId = devs.id.ToString();
    //    outline.ParentId = devs.parentId.ToString();
    //    outline.OutlineName = devs.areaName;
    //    outline.Children = devs.children;
    //    //outline.Type = 1;
    //    outline.DevList = devs.devList;
    //    outlineInfoList.Add(outline);
    //    if (outline.DevList != null)
    //    {
    //        for (int j = 0; j < outline.DevList.Count; j++)
    //        {
    //            OutlineInfo outlinedev = new OutlineInfo();
    //            //outlinedev.OutlineId = outline.DevList[j].id.ToString();
    //            outlinedev.OutlineId = "";
    //            outlinedev.ParentId = outline.OutlineId;
    //            outlinedev.OutlineName = outline.DevList[j].cameraname;
    //            outlinedev.Children = null;
    //            outlinedev.DevList = null;
    //            outlinedev.NVr = outline.DevList[j];
    //            outlineInfoList.Add(outlinedev);
    //        }
    //    }


    //    if (devs.children.Count != 0)
    //    {
    //        for (int i = 0; i < devs.children.Count; i++)
    //        {
    //            ProductDoorTreeDic(devs.children[i]);
    //        }
    //    }

    //}


    private void ProductDoorTreeDic(GetAreaGroupDevs devs)
    {

        OutlineInfo outline = new OutlineInfo();
        outline.OutlineId = devs.id.ToString();
        outline.ParentId = devs.parentId.ToString();
        outline.OutlineName = devs.areaName;
        outline.Children = devs.children;
        //outline.Type = 1;
        outline.DevList = devs.devList;
        outlineInfoList.Add(outline);
        if (outline.DevList != null)
        {
            for (int j = 0; j < outline.DevList.Count; j++)
            {
                OutlineInfo outlinedev = new OutlineInfo();
                //outlinedev.OutlineId = outline.DevList[j].id.ToString();
                outlinedev.OutlineId = "";
                outlinedev.ParentId = outline.OutlineId;
                outlinedev.OutlineName = outline.DevList[j].cameraname;
                outlinedev.Children = null;
                outlinedev.DevList = null;
                outlinedev.NVr = outline.DevList[j];
                outlineInfoList.Add(outlinedev);
            }

        }


        if (devs.children.Count != 0)
        {
            for (int i = 0; i < devs.children.Count; i++)
            {
                ProductDoorTreeDic(devs.children[i]);
            }
        }

    }
    /// <summary>
    /// 模糊查询监控设备
    /// </summary>
    List<OutlineInfo> SearchoutlineInfoList = new List<OutlineInfo>();
    private void SearchCamera()
    {
        SearchoutlineInfoList.Clear();
        //先清除
        if (_mainPanel.MainPreviewTreeView.transform.childCount != 0)
        {
            for (int i = _mainPanel.MainPreviewTreeView.transform.childCount - 1; i >= 0; i--)
            {
                int temp = i;
                if (_mainPanel.MainPreviewTreeView.transform.GetChild(temp).GetComponent<TreeViewItem>() != null && _mainPanel.MainPreviewTreeView.transform.GetChild(temp).name == "ItemPrefab1(Clone)")
                    TreeManagerPreview.GetInstance().OnDeleteBtnClicked(_mainPanel.MainPreviewTreeView.transform.GetChild(temp).GetComponent<TreeViewItem>());
            }

        }
        if (!string.IsNullOrEmpty(InputStrcamename))
        {
            for (int i = 0; i < outlineInfoList.Count; i++)
            {
                if (outlineInfoList[i].OutlineName.Contains(InputStrcamename))
                {
                    Log.Debug("找到相关设备：" + outlineInfoList[i].OutlineName);
                    //if(outlineInfoList[i].)
                    if (outlineInfoList[i].OutlineId == "")
                        SearchoutlineInfoList.Add(outlineInfoList[i]);
                }
            }
            if (SearchoutlineInfoList.Count > 0)
            {
                //再显示
                for (int i = 0; i < SearchoutlineInfoList.Count; i++)
                {

                    if (_mainPanel.MainPreviewTreeView != null)
                    {

                        TreeViewItem childItem = _mainPanel.MainPreviewTreeView.AppendItem("ItemPrefab1");
                        childItem.GetComponent<ItemScript>().id = SearchoutlineInfoList[i].OutlineId;
                        childItem.GetComponent<ItemScript>().parentId = SearchoutlineInfoList[i].ParentId;
                        ConfigurePreviewTreeLabel(childItem.GetComponent<ItemScript>(), SearchoutlineInfoList[i].OutlineName);
                        childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
                        childItem.GetComponent<ItemScript>().type = SearchoutlineInfoList[i].Type;
                        childItem.GetComponent<ItemScript>().children = SearchoutlineInfoList[i].Children;
                        childItem.GetComponent<ItemScript>().devList = SearchoutlineInfoList[i].DevList;
                        if (SearchoutlineInfoList[i].OutlineId == "")
                        {
                            childItem.GetComponent<ItemScript>().icon.sprite = Resources.Load<Sprite>("UI/录像回放/摄像头-0");
                            childItem.GetComponent<ItemScript>().labelText.interactable = false;
                            childItem.GetComponent<ItemScript>().nvr = SearchoutlineInfoList[i].NVr;

                        }






                    }


                }

            }
            else
            {
                GameStart.Instance.ShowTip("没有查到相关设备!");
            }

        }

        _mainPanel.searchBtn.gameObject.SetActive(false);
        _mainPanel.clearBtn.gameObject.SetActive(true);



    }
    List<TreeViewItem> allCameraTreeViewItemList = new List<TreeViewItem>();
    /// <summary>
    ///  生成监控树级目录
    /// </summary>
    /// <param name="outlineInfoList"></param>
    public void InitCameraGroup(List<OutlineInfo> outlineInfoList)
    {




        allCameraTreeViewItemList = new List<TreeViewItem>();
        TreeViewItem item1 = new TreeViewItem();
        for (int i = 0; i < outlineInfoList.Count; i++)
        {
            int temp = i;
            if (string.IsNullOrEmpty(outlineInfoList[temp].ParentId) || int.Parse(outlineInfoList[temp].ParentId) == 0)
            {

                if (_mainPanel.MainPreviewTreeView != null)
                {
                    item1 = _mainPanel.MainPreviewTreeView.AppendItem("ItemPrefab1");
                    item1.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                    item1.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                    ConfigurePreviewTreeLabel(item1.GetComponent<ItemScript>(), outlineInfoList[temp].OutlineName);
                    item1.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                    item1.GetComponent<ItemScript>().SetItem(item1.GetComponent<ItemScript>());
                    allCameraTreeViewItemList.Add(item1);
                }



            }
            else
            {
                for (int j = 0; j < allCameraTreeViewItemList.Count; j++)
                {
                    if (allCameraTreeViewItemList[j].GetComponent<ItemScript>().id.Equals(outlineInfoList[temp].ParentId))
                    {
                        if (_mainPanel.MainPreviewTreeView != null)
                        {

                            TreeViewItem childItem = allCameraTreeViewItemList[j].ChildTree.AppendItem("ItemPrefab1");
                            childItem.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                            childItem.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                            ConfigurePreviewTreeLabel(childItem.GetComponent<ItemScript>(), outlineInfoList[temp].OutlineName);
                            childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
                            childItem.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                            childItem.GetComponent<ItemScript>().children = outlineInfoList[temp].Children;
                            childItem.GetComponent<ItemScript>().devList = outlineInfoList[temp].DevList;

                            if (outlineInfoList[temp].OutlineId == "")
                            {
                                childItem.GetComponent<ItemScript>().icon.sprite = Resources.Load<Sprite>("UI/录像回放/摄像头-0");
                                childItem.GetComponent<ItemScript>().labelText.interactable = false;
                                childItem.GetComponent<ItemScript>().nvr = outlineInfoList[temp].NVr;
                                //NVRController.Instance().Login(outlineInfoList[temp].NVr.host);
                            }


                            allCameraTreeViewItemList.Add(childItem);



                        }

                    }
                }
            }
        }


    }







    //播放选择的设备监听
    private void CameraPlayEvent(NVRInformation nvr)
    {

        if (string.Equals(nvr.equiptype, "编码设备"))
        {
            if (IsMax)
            {
                Log.Debug("最大化播放:" + nvr.cameraname);
                CameraPlay(nvr.host, nvr.type, nvr.channel, STREAM.MAIN, ChoiceIndex, nvr.id, nvr.password);
            }

            else
            {
               
                CameraPlay(nvr.host, nvr.type, nvr.channel, STREAM.EXTRA, ChoiceIndex, nvr.id, nvr.password);
            
                if (videoRenderList.Count > 0 && videoRenderList[ChoiceIndex].GetComponent<SecurityCamera>().player != null && videoRenderList[ChoiceIndex].GetComponent<SecurityCamera>().player.IsRealPlaying)
                    ChoiceIndex = (ChoiceIndex + 1) % videoRenderList.Count;//逐个播放，并在最后一个窗口后回到第一个
                Log.Debug("最小化播放:" + nvr.cameraname + " ChoiceIndex:" + ChoiceIndex);
              
            }


            devid = nvr.id;
            for (int i = 0; i < allCameraTreeViewItemList.Count; i++)
            {
                if (allCameraTreeViewItemList[i].GetComponent<ItemScript>().nvr.id != devid)
                    allCameraTreeViewItemList[i].GetComponent<ItemScript>().selectImg.gameObject.SetActive(false);
                //Log.Error("nvr.id:" + allCameraTreeViewItemList[i].GetComponent<ItemScript>().nvr.id);
            }
            Log.Error("   devid:" + devid);
            if (!_mainPanel.SaveCameraRenderDic.ContainsKey(nvr.host))
                _mainPanel.SaveCameraRenderDic.Add(nvr.host, nvr);

        }

    }




    #region 云台控制
    /// <summary>
    /// 设置单个Camera配置信息
    /// </summary>
    /// <param name="cameraObj"></param>
    /// <param name="host"></param>
    /// <param name="sdktype"></param>
    /// <param name="channel"></param>
    /// <param name="stream"></param>
    void CloundBtnUp_MouseDownListener(int index)
    {
        videoRenderList[index].GetComponent<SecurityCamera>().CloundBtnUp_MouseDown();
    }
    void CloundBtnUp_MouseUpListener(int index)
    {
        videoRenderList[index].GetComponent<SecurityCamera>().CloundBtnUp_MouseUp();
    }

    void CloundBtnDown_MouseUpListener(int index)
    {
        videoRenderList[index].GetComponent<SecurityCamera>().CloundBtnDown_MouseUp();
    }
    void CloundBtnDown_MouseDownListener(int index)
    {
        videoRenderList[index].GetComponent<SecurityCamera>().CloundBtnDown_MouseDown();
    }

    void CloundBtnLeft_MouseDownListener(int index)
    {
        videoRenderList[index].GetComponent<SecurityCamera>().CloundBtnLeft_MouseDown();
    }
    void CloundBtnLeft_MouseUpListener(int index)
    {
        videoRenderList[index].GetComponent<SecurityCamera>().CloundBtnLeft_MouseUp();
    }
    void CloundBtnRight_MouseDownListener(int index)
    {
        videoRenderList[index].GetComponent<SecurityCamera>().CloundBtnRight_MouseDown();
    }
    void CloundBtnRight_MouseUpListener(int index)
    {
        videoRenderList[index].GetComponent<SecurityCamera>().CloundBtnRight_MouseUp();
    }

    void CloundBtnZoomIn_MouseDownListener(int index)
    {
        videoRenderList[index].GetComponent<SecurityCamera>().CloundBtnZoomIn_MouseDown();
    }

    void CloundBtnZoomIn_MouseUpListener(int index)
    {
        videoRenderList[index].GetComponent<SecurityCamera>().CloundBtnZoomIn_MouseUp();
    }


    void CloundBtnZoomOut_MouseDownListener(int index)
    {
        videoRenderList[index].GetComponent<SecurityCamera>().CloundBtnZoomOut_MouseDown();
    }

    void CloundBtnZoomOut_MouseUpListener(int index)
    {
        videoRenderList[index].GetComponent<SecurityCamera>().CloundBtnZoomOut_MouseUp();
    }


    #endregion





    void CameraPlay(string host, SDKTYPE sdktype, int channel, STREAM stream, int ChoiceIndex = -1, int devid = -1, string pwd = "")
    {
        if (!IsConfiguredHost(host))
        {
            return;
        }
        if (ChoiceIndex != -1)
        {
            if (ChoiceIndex < 0 || ChoiceIndex >= videoRenderList.Count || videoRenderList[ChoiceIndex] == null)
            {
                Log.Error("播放窗口索引无效: " + ChoiceIndex);
                return;
            }
            if (host.Contains(":37777"))
            {
                sdktype = SDKTYPE.DH;
            }
            else
            {
                sdktype = SDKTYPE.HK;
            }
            //ChoiceIndex++;
            Log.Debug("choiceIndex:" + ChoiceIndex + "videoRenderList.count:" + videoRenderList.Count);
            Log.Debug("播放:" + host + "sdktype:" + sdktype + "channel:" + channel + "stream:" + stream);

            SecurityCamera securityCamera = videoRenderList[ChoiceIndex].GetComponent<SecurityCamera>();
            if (securityCamera == null)
            {
                return;
            }
            RemovePtzListeners();
            if (securityCamera.player != null)
            {
                securityCamera.OnExitCamera();
            }
            securityCamera.host = host;
            Log.Error("devid:" + devid);
            securityCamera.devid = devid;
            securityCamera.sdk = sdktype;
            securityCamera.channel = channel;
            securityCamera.passward = pwd;
            securityCamera.steamType = stream;
            securityCamera.SetupPlayer();
            securityCamera.PlayReal();

            EventCenter.addlistener<int>(Eventdefine.ptzupdown, CloundBtnUp_MouseDownListener);
            EventCenter.addlistener<int>(Eventdefine.ptzupup, CloundBtnUp_MouseUpListener);
            EventCenter.addlistener<int>(Eventdefine.ptzdownup, CloundBtnDown_MouseUpListener);
            EventCenter.addlistener<int>(Eventdefine.ptzdowndown, CloundBtnDown_MouseDownListener);
            EventCenter.addlistener<int>(Eventdefine.ptzleftdown, CloundBtnLeft_MouseDownListener);
            EventCenter.addlistener<int>(Eventdefine.ptzleftup, CloundBtnLeft_MouseUpListener);
            EventCenter.addlistener<int>(Eventdefine.ptzrightdown, CloundBtnRight_MouseDownListener);
            EventCenter.addlistener<int>(Eventdefine.ptzrightup, CloundBtnRight_MouseUpListener);
            EventCenter.addlistener<int>(Eventdefine.ptzzoomoutup, CloundBtnZoomOut_MouseUpListener);
            EventCenter.addlistener<int>(Eventdefine.ptzzoomoutdown, CloundBtnZoomOut_MouseDownListener);
            EventCenter.addlistener<int>(Eventdefine.ptzzoominup, CloundBtnZoomIn_MouseUpListener);
            EventCenter.addlistener<int>(Eventdefine.ptzzoomindown, CloundBtnZoomIn_MouseDownListener);
        }
        else
        {
            Log.Debug("videoRenderList.count:" + videoRenderList.Count);
            foreach (var item in videoRenderList)
            {
                if (!item.transform.GetChild(0).GetComponent<VideoRenderer>().IsRendering)
                {
                    item.GetComponent<SecurityCamera>().host = host;

                    item.GetComponent<SecurityCamera>().devid = devid;
                    item.GetComponent<SecurityCamera>().sdk = sdktype;
                    item.GetComponent<SecurityCamera>().channel = channel;
                    item.GetComponent<SecurityCamera>().SetupPlayer();

                    item.GetComponent<SecurityCamera>().steamType = stream;
                    item.GetComponent<SecurityCamera>().passward = pwd;
                    item.GetComponent<SecurityCamera>().PlayReal();
                    break;

                }

            }

        }



    }

    private void RemovePtzListeners()
    {
        EventCenter.RemoveListener<int>(Eventdefine.ptzupdown, CloundBtnUp_MouseDownListener);
        EventCenter.RemoveListener<int>(Eventdefine.ptzupup, CloundBtnUp_MouseUpListener);
        EventCenter.RemoveListener<int>(Eventdefine.ptzdownup, CloundBtnDown_MouseUpListener);
        EventCenter.RemoveListener<int>(Eventdefine.ptzdowndown, CloundBtnDown_MouseDownListener);
        EventCenter.RemoveListener<int>(Eventdefine.ptzleftdown, CloundBtnLeft_MouseDownListener);
        EventCenter.RemoveListener<int>(Eventdefine.ptzleftup, CloundBtnLeft_MouseUpListener);
        EventCenter.RemoveListener<int>(Eventdefine.ptzrightdown, CloundBtnRight_MouseDownListener);
        EventCenter.RemoveListener<int>(Eventdefine.ptzrightup, CloundBtnRight_MouseUpListener);
        EventCenter.RemoveListener<int>(Eventdefine.ptzzoomoutup, CloundBtnZoomOut_MouseUpListener);
        EventCenter.RemoveListener<int>(Eventdefine.ptzzoomoutdown, CloundBtnZoomOut_MouseDownListener);
        EventCenter.RemoveListener<int>(Eventdefine.ptzzoominup, CloundBtnZoomIn_MouseUpListener);
        EventCenter.RemoveListener<int>(Eventdefine.ptzzoomindown, CloundBtnZoomIn_MouseDownListener);
    }

    /// <summary>
    /// 轮巡播放
    /// </summary>
    void patrolPlay(string host, SDKTYPE sdktype, int channel, STREAM stream)
    {
        foreach (var item in videoRenderList)
        {
            if (!item.transform.GetChild(0).GetComponent<VideoRenderer>().IsRendering)
            {
                item.GetComponent<SecurityCamera>().host = host;
                item.GetComponent<SecurityCamera>().sdk = sdktype;
                item.GetComponent<SecurityCamera>().channel = channel;
                item.GetComponent<SecurityCamera>().SetupPlayer();
                item.GetComponent<SecurityCamera>().steamType = stream;
                item.GetComponent<SecurityCamera>().PlayReal();
                break;

            }

        }

    }
    /// <summary>
    /// 重置预制体
    /// </summary>
    /// <param name="obj"></param>
    void resetPrefab(GameObject obj)
    {
        obj.transform.localScale = Vector3.one;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localPosition = Vector3.zero;
    }


    public override void OnDisable()
    {
        HttpNetManager.GetInstance().CancelRequestsFor(this);
        RemovePtzListeners();
        for (int i = 0; i < videoRenderList.Count; i++)
        {
            GameObject renderObject = videoRenderList[i];
            if (renderObject != null)
            {
                renderObject.GetComponent<SecurityCamera>()?.OnExitCamera();
            }
        }
        int kind_id = PlayerPrefs.GetInt("Kinds");
        if (kind_id == 1)
            _mainPanel._1_tog.isOn = false;
        if (kind_id == 4)
            _mainPanel._4_tog.isOn = false;
        if (kind_id == 6)
            _mainPanel._6_tog.isOn = false;
        if (kind_id == 8)
            _mainPanel._8_tog.isOn = false;
        if (kind_id == 9)
            _mainPanel._9_tog.isOn = false;
        if (kind_id == 13)
            _mainPanel._13_tog.isOn = false;
        if (kind_id == 16)
            _mainPanel._16_tog.isOn = false;
        if (kind_id == 25)
            _mainPanel._25_tog.isOn = false;
        if (kind_id == 32)
            _mainPanel._32_tog.isOn = false;
        if (kind_id == 36)
            _mainPanel._36_tog.isOn = false;
        if (kind_id == 64)
            _mainPanel._64_tog.isOn = false;

        _mainPanel.loadingani.gameObject.SetActive(false);
        //NVRController.Instance().Logout();


        //如果算法推流视频在开启则退出
        Ondisabeclosesuanfa();
        EventCenter.RemoveListener<NVRInformation>(Eventdefine.CameraPlayEvent, CameraPlayEvent);
        RemoveAllToggleListener();
        RemoveAllButtonListener();


        //for (int i = 0; i < _mainPanel.Grid.transform.childCount; i++)
        //{
        //    if (_mainPanel.Grid.transform.GetChild(i).GetComponent<SecurityCamera>().player != null && _mainPanel.Grid.transform.GetChild(i).GetComponent<SecurityCamera>().player.IsRealPlaying)
        //    {
        //        _mainPanel.Grid.transform.GetChild(i).GetComponent<SecurityCamera>().OnExitCamera();
        //        //_mainPanel.Grid.transform.GetChild(i).GetComponent<SecurityCamera>().monitor = null;
        //        GameObject.Destroy(_mainPanel.Grid.transform.GetChild(i).gameObject);
        //    }
        //}

    }

    public override void OnClose()
    {
        videoRenderList.Clear();
        cameraPlayer = null;
        ChoiceIndex = 0;
        IsMax = false;
        base.OnClose();
    }
    private void Ondisabeclosesuanfa()
    {


        if (_mainPanel.PlayfaceImage.gameObject.activeInHierarchy)
        {
            _mainPanel.PlayfaceImage.gameObject.SetActive(false);
            FaceData data = new FaceData();
            if (!string.IsNullOrEmpty(DType))
                data.dtype = DType;
            data.state = 0;
            data.uuid = devid.ToString();
            data.rtsp = facertsp;
            ReqFaceSevData reqFaceSevData = new ReqFaceSevData();
            reqFaceSevData.requestMgrHostIp = "192.168.110.2";
            reqFaceSevData.requestMgrHostPort = "77111";
            reqFaceSevData.requestFaceRecInfoList.Add(data);
            string reqstr = JsonUtility.ToJson(reqFaceSevData, true);
            Log.Debug("关闭人脸检测算法信息：" + reqstr);
            HttpNetManager.GetInstance().SendDataStr(AppRuntimeConfig.Settings.faceServiceUrl, RequestCloseFaceSevCallback, true, true, false, reqstr);
        }

    }

    /// <summary>
    /// 添加关闭所有播放监听
    /// </summary>
    void AddShutdownListener(bool ison)
    {
        try
        {
            Log.Error("关闭播放窗口:" + cameraPlayer);
            if (cameraPlayer.GetComponent<SecurityCamera>().player != null && cameraPlayer.GetComponent<SecurityCamera>().player.IsRealPlaying)
            {
                cameraPlayer.GetComponent<SecurityCamera>().OnExitCamera();
                string host = cameraPlayer.GetComponent<SecurityCamera>().host;
                cameraPlayer.GetComponent<SecurityCamera>().host = "";
                NVRController.Instance().Logout(host);//新增关闭当前播放窗口时退出登录
            }
            else
            {
                cameraPlayer.GetComponent<SecurityCamera>().host = "";
                GameStart.Instance.ShowTip("当前窗口没有播放任何监控画面!");
            }
        }
        catch (Exception)
        {

            throw;
        }
       
        //}

    }


    void HFullScreenListener(bool ison)
    {

        if (ison)
            IsHFullScreen = true;
        else
            IsHFullScreen = false;
        switch (PlayerPrefs.GetInt("Kinds"))
        {
            case 1:
                SetHfullScreen(ison, new Vector2(1900, 933), new Vector2(1553f, 933f), 1);
                break;
            case 4:
                SetHfullScreen(ison, new Vector2(950.8f, 465.5f), new Vector2(778.5f, 465.5f), 2);
                break;
            case 6:
                ExpandSix(ison);
                break;
            case 8:
                ExpandEight(ison);
                break;
            case 9:
                SetHfullScreen(ison, new Vector2(634.2f, 310f), new Vector2(518.7f, 310f), 3);
                break;
            case 13:
                ExpandThirteen(ison);
                break;
            case 16:
                SetHfullScreen(ison, new Vector2(475.3f, 232.25f), new Vector2(388.75f, 232.25f), 4);
                break;
            case 25:
                SetHfullScreen(ison, new Vector2(380f, 185.6f), new Vector2(310.8f, 185.6f), 5);
                break;
            case 32:
                ExpandThirtytwo(ison);
                break;
            case 36:
                SetHfullScreen(ison, new Vector2(316.8f, 154.5f), new Vector2(258.83f, 154.5f), 6);
                break;
            case 64:
                SetHfullScreen(ison, new Vector2(237.1f, 115.625f), new Vector2(193.875f, 115.625f), 8);
                break;
            default:
                break;
        }

    }


    /// <summary>
    /// 六视图横向拓展
    /// </summary>
    /// <param name="ison"></param>
    void ExpandSix(bool ison)
    {
        if (ison)
        {

            _mainPanel.hfullscreenTog.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/视频预览/放-1");
            _mainPanel.hfullscreenTog.transform.DOLocalMoveX(-175, 0f);
            Container6.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1905, 932), 0f);
            Container6.DOLocalMoveX(12, 0f);
            Container6.GetChild(0).GetComponent<RectTransform>().DOSizeDelta(new Vector2(1268.4f, 623.6f), 0f);
            Container6.GetChild(0).DOLocalMoveX(-315f, 0f);

            Container6.GetChild(1).GetComponent<RectTransform>().DOSizeDelta(new Vector2(628.4f, 309.5f), 0f);
            Container6.GetChild(1).DOLocalMoveX(637f, 0f);

            Container6.GetChild(2).GetComponent<RectTransform>().DOSizeDelta(new Vector2(628.4f, 312.1f), 0f);
            Container6.GetChild(2).DOLocalMoveX(637f, 0f);

            Container6.GetChild(3).GetComponent<RectTransform>().DOSizeDelta(new Vector2(632.8f, 306.8f), 0f);
            Container6.GetChild(3).DOLocalMoveX(-633.2f, 0f);

            Container6.GetChild(4).GetComponent<RectTransform>().DOSizeDelta(new Vector2(632.8f, 306.8f), 0f);
            Container6.GetChild(4).DOLocalMoveX(3f, 0f);

            Container6.GetChild(5).GetComponent<RectTransform>().DOSizeDelta(new Vector2(628.4f, 306.8f), 0f);
            Container6.GetChild(5).DOLocalMoveX(637f, 0f);
            Container6.SetAsLastSibling();
        }
        else
        {

            _mainPanel.hfullscreenTog.transform.DOLocalMoveX(175, 0f);
            _mainPanel.hfullscreenTog.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/视频预览/收-1");
            Container6.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1554, 932), 0f);
            Container6.DOLocalMoveX(183, 0f);
            Container6.GetChild(0).GetComponent<RectTransform>().DOSizeDelta(new Vector2(1038.85f, 623.6f), 0f);
            Container6.GetChild(0).DOLocalMoveX(-258.2f, 0f);

            Container6.GetChild(1).GetComponent<RectTransform>().DOSizeDelta(new Vector2(514f, 310f), 0f);
            Container6.GetChild(1).DOLocalMoveX(519.8f, 0f);

            Container6.GetChild(2).GetComponent<RectTransform>().DOSizeDelta(new Vector2(514f, 312f), 0f);
            Container6.GetChild(2).DOLocalMoveX(519.8f, 0f);

            Container6.GetChild(3).GetComponent<RectTransform>().DOSizeDelta(new Vector2(519f, 306.8f), 0f);
            Container6.GetChild(3).DOLocalMoveX(-518.2f, 0f);

            Container6.GetChild(4).GetComponent<RectTransform>().DOSizeDelta(new Vector2(519f, 306.8f), 0f);
            Container6.GetChild(4).DOLocalMoveX(1.7f, 0f);

            Container6.GetChild(5).GetComponent<RectTransform>().DOSizeDelta(new Vector2(514f, 306.8f), 0f);
            Container6.GetChild(5).DOLocalMoveX(519.8f, 0f);
            Container6.SetAsFirstSibling();

        }

    }

    /// <summary>
    /// 八视图横向扩展
    /// </summary>
    /// <param name="ison"></param>
    void ExpandEight(bool ison)
    {
        if (ison)
        {

            _mainPanel.hfullscreenTog.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/视频预览/放-1");
            _mainPanel.hfullscreenTog.transform.DOLocalMoveX(-175, 0f);
            Container8.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1905, 932), 0f);
            Container8.DOLocalMoveX(12, 0f);
            Container8.GetChild(0).GetComponent<RectTransform>().DOSizeDelta(new Vector2(1431.3f, 704f), 0f);
            Container8.GetChild(0).DOLocalMoveX(-235.9f, 0f);

            Container8.GetChild(1).GetComponent<RectTransform>().DOSizeDelta(new Vector2(465.5f, 233.3f), 0f);
            Container8.GetChild(1).DOLocalMoveX(715.3f, 0f);

            Container8.GetChild(2).GetComponent<RectTransform>().DOSizeDelta(new Vector2(464.5f, 233.3f), 0f);
            Container8.GetChild(2).DOLocalMoveX(714.8f, 0f);

            Container8.GetChild(3).GetComponent<RectTransform>().DOSizeDelta(new Vector2(464.5f, 233.3f), 0f);
            Container8.GetChild(3).DOLocalMoveX(714.8f, 0f);

            Container8.GetChild(4).GetComponent<RectTransform>().DOSizeDelta(new Vector2(475.3f, 225.8f), 0f);
            Container8.GetChild(4).DOLocalMoveX(-713.9f, 0f);

            Container8.GetChild(5).GetComponent<RectTransform>().DOSizeDelta(new Vector2(475.3f, 225.8f), 0f);
            Container8.GetChild(5).DOLocalMoveX(-235.9f, 0f);

            Container8.GetChild(6).GetComponent<RectTransform>().DOSizeDelta(new Vector2(475.3f, 225.8f), 0f);
            Container8.GetChild(6).DOLocalMoveX(242.1f, 0f);

            Container8.GetChild(7).GetComponent<RectTransform>().DOSizeDelta(new Vector2(464.5f, 225.8f), 0f);
            Container8.GetChild(7).DOLocalMoveX(714.8f, 0f);
            Container8.SetAsLastSibling();
        }
        else
        {

            _mainPanel.hfullscreenTog.transform.DOLocalMoveX(175, 0f);
            _mainPanel.hfullscreenTog.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/视频预览/收-1");
            Container8.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1554, 932), 0f);
            Container8.DOLocalMoveX(183, 0f);
            Container8.GetChild(0).GetComponent<RectTransform>().DOSizeDelta(new Vector2(1171.5f, 704f), 0f);
            Container8.GetChild(0).DOLocalMoveX(-191.2f, 0f);

            Container8.GetChild(1).GetComponent<RectTransform>().DOSizeDelta(new Vector2(381f, 233.3f), 0f);
            Container8.GetChild(1).DOLocalMoveX(587.3f, 0f);

            Container8.GetChild(2).GetComponent<RectTransform>().DOSizeDelta(new Vector2(380.2f, 233.3f), 0f);
            Container8.GetChild(2).DOLocalMoveX(586.9f, 0f);

            Container8.GetChild(3).GetComponent<RectTransform>().DOSizeDelta(new Vector2(380.2f, 233.3f), 0f);
            Container8.GetChild(3).DOLocalMoveX(586.9f, 0f);

            Container8.GetChild(4).GetComponent<RectTransform>().DOSizeDelta(new Vector2(389f, 225.8f), 0f);
            Container8.GetChild(4).DOLocalMoveX(-582.5f, 0f);

            Container8.GetChild(5).GetComponent<RectTransform>().DOSizeDelta(new Vector2(389f, 225.8f), 0f);
            Container8.GetChild(5).DOLocalMoveX(-191.2f, 0f);

            Container8.GetChild(6).GetComponent<RectTransform>().DOSizeDelta(new Vector2(389f, 225.8f), 0f);
            Container8.GetChild(6).DOLocalMoveX(200f, 0f);

            Container8.GetChild(7).GetComponent<RectTransform>().DOSizeDelta(new Vector2(380.2f, 225.8f), 0f);
            Container8.GetChild(7).DOLocalMoveX(586.9f, 0f);
            Container8.SetAsFirstSibling();
        }
    }
    /// <summary>
    /// 13视图扩展
    /// </summary>
    /// <param name="ison"></param>
    void ExpandThirteen(bool ison)
    {
        if (ison)
        {

            _mainPanel.hfullscreenTog.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/视频预览/放-1");
            _mainPanel.hfullscreenTog.transform.DOLocalMoveX(-175, 0f);
            Container13.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1905, 932), 0f);
            Container13.DOLocalMoveX(12, 0f);
            Container13.GetChild(0).GetComponent<RectTransform>().DOSizeDelta(new Vector2(474.74f, 233.3f), 0f);
            Container13.GetChild(0).DOLocalMoveX(-714.67f, 0f);

            Container13.GetChild(1).GetComponent<RectTransform>().DOSizeDelta(new Vector2(474.74f, 233.3f), 0f);
            Container13.GetChild(1).DOLocalMoveX(-238.37f, 0f);

            Container13.GetChild(2).GetComponent<RectTransform>().DOSizeDelta(new Vector2(474.74f, 233.3f), 0f);
            Container13.GetChild(2).DOLocalMoveX(237.59f, 0f);

            Container13.GetChild(3).GetComponent<RectTransform>().DOSizeDelta(new Vector2(950.68f, 467.35f), 0f);
            Container13.GetChild(3).DOLocalMoveX(-0.38f, 0f);

            Container13.GetChild(4).GetComponent<RectTransform>().DOSizeDelta(new Vector2(474.74f, 233.3f), 0f);
            Container13.GetChild(4).DOLocalMoveX(713.57f, 0f);

            Container13.GetChild(5).GetComponent<RectTransform>().DOSizeDelta(new Vector2(474.74f, 233.3f), 0f);
            Container13.GetChild(5).DOLocalMoveX(713f, 0f);

            Container13.GetChild(6).GetComponent<RectTransform>().DOSizeDelta(new Vector2(474.74f, 233.3f), 0f);
            Container13.GetChild(6).DOLocalMoveX(-714.41f, 0f);

            Container13.GetChild(7).GetComponent<RectTransform>().DOSizeDelta(new Vector2(474.74f, 233.3f), 0f);
            Container13.GetChild(7).DOLocalMoveX(-714.33f, 0f);

            Container13.GetChild(8).GetComponent<RectTransform>().DOSizeDelta(new Vector2(474.74f, 233.3f), 0f);
            Container13.GetChild(8).DOLocalMoveX(713f, 0f);

            Container13.GetChild(9).GetComponent<RectTransform>().DOSizeDelta(new Vector2(474.74f, 233.3f), 0f);
            Container13.GetChild(9).DOLocalMoveX(-714.41f, 0f);

            Container13.GetChild(10).GetComponent<RectTransform>().DOSizeDelta(new Vector2(474.74f, 233.3f), 0f);
            Container13.GetChild(10).DOLocalMoveX(-238.37f, 0f);

            Container13.GetChild(11).GetComponent<RectTransform>().DOSizeDelta(new Vector2(475.4f, 233.054f), 0f);
            Container13.GetChild(11).DOLocalMoveX(237.26f, 0f);

            Container13.GetChild(12).GetComponent<RectTransform>().DOSizeDelta(new Vector2(474.74f, 233.3f), 0f);
            Container13.GetChild(12).DOLocalMoveX(713.57f, 0f);
            Container13.SetAsLastSibling();
        }
        else
        {

            _mainPanel.hfullscreenTog.transform.DOLocalMoveX(175, 0f);
            _mainPanel.hfullscreenTog.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/视频预览/收-1");
            Container13.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1554, 932.7f), 0f);
            Container13.DOLocalMoveX(183, 0f);
            Container13.GetChild(0).GetComponent<RectTransform>().DOSizeDelta(new Vector2(387.75f, 233.3f), 0f);
            Container13.GetChild(0).DOLocalMoveX(-583.13f, 0f);

            Container13.GetChild(1).GetComponent<RectTransform>().DOSizeDelta(new Vector2(387.75f, 233.3f), 0f);
            Container13.GetChild(1).DOLocalMoveX(-194.38f, 0f);

            Container13.GetChild(2).GetComponent<RectTransform>().DOSizeDelta(new Vector2(387.75f, 233.3f), 0f);
            Container13.GetChild(2).DOLocalMoveX(194.37f, 0f);

            Container13.GetChild(3).GetComponent<RectTransform>().DOSizeDelta(new Vector2(775.956f, 467.35f), 0f);
            Container13.GetChild(3).DOLocalMoveX(-0.27292f, 0f);

            Container13.GetChild(4).GetComponent<RectTransform>().DOSizeDelta(new Vector2(387.75f, 233.3f), 0f);
            Container13.GetChild(4).DOLocalMoveX(583.13f, 0f);

            Container13.GetChild(5).GetComponent<RectTransform>().DOSizeDelta(new Vector2(387.75f, 233.3f), 0f);
            Container13.GetChild(5).DOLocalMoveX(582.67f, 0f);

            Container13.GetChild(6).GetComponent<RectTransform>().DOSizeDelta(new Vector2(387.75f, 233.3f), 0f);
            Container13.GetChild(6).DOLocalMoveX(-583.2f, 0f);

            Container13.GetChild(7).GetComponent<RectTransform>().DOSizeDelta(new Vector2(387.75f, 233.3f), 0f);
            Container13.GetChild(7).DOLocalMoveX(-583.13f, 0f);

            Container13.GetChild(8).GetComponent<RectTransform>().DOSizeDelta(new Vector2(387.75f, 233.3f), 0f);
            Container13.GetChild(8).DOLocalMoveX(582.67f, 0f);

            Container13.GetChild(9).GetComponent<RectTransform>().DOSizeDelta(new Vector2(387.75f, 233.3f), 0f);
            Container13.GetChild(9).DOLocalMoveX(-583.2f, 0f);

            Container13.GetChild(10).GetComponent<RectTransform>().DOSizeDelta(new Vector2(387.75f, 233.3f), 0f);
            Container13.GetChild(10).DOLocalMoveX(-194.38f, 0f);

            Container13.GetChild(11).GetComponent<RectTransform>().DOSizeDelta(new Vector2(387.75f, 233.3f), 0f);
            Container13.GetChild(11).DOLocalMoveX(193.83f, 0f);

            Container13.GetChild(12).GetComponent<RectTransform>().DOSizeDelta(new Vector2(387.75f, 233.3f), 0f);
            Container13.GetChild(12).DOLocalMoveX(583.13f, 0f);
            Container13.SetAsFirstSibling();

        }
    }

    void ExpandThirtytwo(bool ison)
    {
        if (ison)
        {

            _mainPanel.hfullscreenTog.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/视频预览/放-1");
            _mainPanel.hfullscreenTog.transform.DOLocalMoveX(-175, 0f);
            Container32.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1905, 932), 0f);
            Container32.DOLocalMoveX(12, 0f);
            Container32.GetChild(0).GetComponent<RectTransform>().DOSizeDelta(new Vector2(542f, 265.5f), 0f);
            Container32.GetChild(0).DOLocalMoveX(-681f, 0f);

            Container32.GetChild(1).GetComponent<RectTransform>().DOSizeDelta(new Vector2(542f, 265.5f), 0f);
            Container32.GetChild(1).DOLocalMoveX(-138f, 0f);

            Container32.GetChild(2).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(2).DOLocalMoveX(269.4f, 0f);

            Container32.GetChild(3).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(3).DOLocalMoveX(540.8f, 0f);

            Container32.GetChild(4).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(4).DOLocalMoveX(812.3f, 0f);

            Container32.GetChild(5).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(5).DOLocalMoveX(269.4f, 0f);

            Container32.GetChild(6).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(6).DOLocalMoveX(540.8f, 0f);

            Container32.GetChild(7).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(7).DOLocalMoveX(812.3f, 0f);

            Container32.GetChild(8).GetComponent<RectTransform>().DOSizeDelta(new Vector2(541.2f, 265.8f), 0f);
            Container32.GetChild(8).DOLocalMoveX(-681f, 0f);

            Container32.GetChild(9).GetComponent<RectTransform>().DOSizeDelta(new Vector2(813.1f, 399.2f), 0f);
            Container32.GetChild(9).DOLocalMoveX(-2f, 0f);

            Container32.GetChild(10).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(10).DOLocalMoveX(540.8f, 0f);

            Container32.GetChild(11).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(11).DOLocalMoveX(812.3f, 0f);

            Container32.GetChild(12).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(12).DOLocalMoveX(540.8f, 0f);

            Container32.GetChild(13).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(13).DOLocalMoveX(812.3f, 0f);

            Container32.GetChild(14).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(14).DOLocalMoveX(-816.4f, 0f);

            Container32.GetChild(15).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(15).DOLocalMoveX(-545f, 0f);

            Container32.GetChild(16).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(16).DOLocalMoveX(540.8f, 0f);

            Container32.GetChild(17).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(17).DOLocalMoveX(812.3f, 0f);

            Container32.GetChild(18).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(18).DOLocalMoveX(-816.4f, 0f);

            Container32.GetChild(19).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(19).DOLocalMoveX(-545f, 0f);

            Container32.GetChild(20).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(20).DOLocalMoveX(-273.5f, 0f);

            Container32.GetChild(21).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(21).DOLocalMoveX(-2f, 0f);

            Container32.GetChild(22).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(22).DOLocalMoveX(269.4f, 0f);

            Container32.GetChild(23).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(23).DOLocalMoveX(540.8f, 0f);

            Container32.GetChild(24).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(24).DOLocalMoveX(812.3f, 0f);

            Container32.GetChild(25).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(25).DOLocalMoveX(-816.4f, 0f);

            Container32.GetChild(26).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(26).DOLocalMoveX(-545f, 0f);

            Container32.GetChild(27).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(27).DOLocalMoveX(-273.5f, 0f);

            Container32.GetChild(28).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(28).DOLocalMoveX(-2f, 0f);

            Container32.GetChild(29).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(29).DOLocalMoveX(269.4f, 0f);

            Container32.GetChild(30).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(30).DOLocalMoveX(540.8f, 0f);

            Container32.GetChild(31).GetComponent<RectTransform>().DOSizeDelta(new Vector2(270.2f, 132.4f), 0f);
            Container32.GetChild(31).DOLocalMoveX(812.3f, 0f);
            Container32.SetAsLastSibling();
        }
        else
        {

            _mainPanel.hfullscreenTog.transform.DOLocalMoveX(175, 0f);
            _mainPanel.hfullscreenTog.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/视频预览/收-1");
            Container32.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1554, 932.7f), 0f);
            Container32.DOLocalMoveX(183, 0f);
            Container32.GetChild(0).GetComponent<RectTransform>().DOSizeDelta(new Vector2(442.85f, 265.27f), 0f);
            Container32.GetChild(0).DOLocalMoveX(-555.56f, 0f);

            Container32.GetChild(1).GetComponent<RectTransform>().DOSizeDelta(new Vector2(443.571f, 265.468f), 0f);
            Container32.GetChild(1).DOLocalMoveX(-111.216f, 0f);

            Container32.GetChild(2).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(2).DOLocalMoveX(222.14f, 0f);

            Container32.GetChild(3).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(3).DOLocalMoveX(444.28f, 0f);

            Container32.GetChild(4).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(4).DOLocalMoveX(666.42f, 0f);

            Container32.GetChild(5).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(5).DOLocalMoveX(222.14f, 0f);

            Container32.GetChild(6).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(6).DOLocalMoveX(444.28f, 0f);

            Container32.GetChild(7).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(7).DOLocalMoveX(666.42f, 0f);

            Container32.GetChild(8).GetComponent<RectTransform>().DOSizeDelta(new Vector2(442.85f, 265.8f), 0f);
            Container32.GetChild(8).DOLocalMoveX(-555.56f, 0f);

            Container32.GetChild(9).GetComponent<RectTransform>().DOSizeDelta(new Vector2(665.4f, 399.2f), 0f);
            Container32.GetChild(9).DOLocalMoveX(0f, 0f);

            Container32.GetChild(10).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(10).DOLocalMoveX(444.28f, 0f);

            Container32.GetChild(11).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(11).DOLocalMoveX(666.42f, 0f);

            Container32.GetChild(12).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(12).DOLocalMoveX(444.28f, 0f);

            Container32.GetChild(13).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(13).DOLocalMoveX(666.42f, 0f);

            Container32.GetChild(14).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(14).DOLocalMoveX(-666.42f, 0f);

            Container32.GetChild(15).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(15).DOLocalMoveX(-444.28f, 0f);

            Container32.GetChild(16).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(16).DOLocalMoveX(444.28f, 0f);

            Container32.GetChild(17).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(17).DOLocalMoveX(666.42f, 0f);

            Container32.GetChild(18).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(18).DOLocalMoveX(-666.42f, 0f);

            Container32.GetChild(19).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(19).DOLocalMoveX(-444.28f, 0f);

            Container32.GetChild(20).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(20).DOLocalMoveX(-222.14f, 0f);

            Container32.GetChild(21).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(21).DOLocalMoveX(0f, 0f);

            Container32.GetChild(22).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(22).DOLocalMoveX(222.14f, 0f);

            Container32.GetChild(23).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(23).DOLocalMoveX(444.28f, 0f);

            Container32.GetChild(24).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(24).DOLocalMoveX(666.42f, 0f);

            Container32.GetChild(25).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(25).DOLocalMoveX(-666.42f, 0f);

            Container32.GetChild(26).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(26).DOLocalMoveX(-444.28f, 0f);

            Container32.GetChild(27).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(27).DOLocalMoveX(-222.14f, 0f);

            Container32.GetChild(28).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(28).DOLocalMoveX(0f, 0f);

            Container32.GetChild(29).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(29).DOLocalMoveX(222.14f, 0f);

            Container32.GetChild(30).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(30).DOLocalMoveX(444.28f, 0f);

            Container32.GetChild(31).GetComponent<RectTransform>().DOSizeDelta(new Vector2(221.14f, 132.4f), 0f);
            Container32.GetChild(31).DOLocalMoveX(666.42f, 0f);
            Container32.SetAsFirstSibling();


        }
    }

    /// <summary>
    /// 
    /// </summary>
    void SetHfullScreen(bool ison, Vector2 ToCellSize, Vector2 Cellsize, int Constcount)
    {
        if (ison)
        {
            _mainPanel.hfullscreenTog.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/视频预览/放-1");
            _mainPanel.hfullscreenTog.transform.DOLocalMoveX(-175, 0.2f);
            _mainPanel.Grid.GetComponent<GridLayoutGroup>().enabled = false;
            _mainPanel.Grid.transform.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1905, 932), 0.2f);
            _mainPanel.Grid.transform.DOLocalMoveX(10, 0.2f);

            _mainPanel.Grid.cellSize = ToCellSize;
            _mainPanel.Grid.GetComponent<GridLayoutGroup>().enabled = true;
            _mainPanel.Grid.GetComponent<GridLayoutGroup>().constraintCount = Constcount;
            _mainPanel.Grid.transform.SetAsLastSibling();
        }
        else
        {
            _mainPanel.hfullscreenTog.transform.DOLocalMoveX(175, 0f);
            _mainPanel.hfullscreenTog.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/视频预览/收-1");
            _mainPanel.Grid.GetComponent<GridLayoutGroup>().enabled = false;
            _mainPanel.Grid.transform.DOLocalMoveX(185, 0f);
            _mainPanel.Grid.transform.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1558, 932), 0f);
            _mainPanel.Grid.cellSize = Cellsize;
            _mainPanel.Grid.GetComponent<GridLayoutGroup>().enabled = true;
            _mainPanel.Grid.transform.SetAsFirstSibling();
        }


    }


    /// <summary>
    /// 添加切换不同比例的视频播放窗口监听
    /// </summary>
    void AddPlayViewListener()
    {
        #region 不同比例分割
        _mainPanel.ScreenGrid_tog.onValueChanged.RemoveAllListeners();
        _mainPanel.ScreenGrid_tog.onValueChanged.AddListener((ison) => { _mainPanel._screengrid_bg.SetActive(ison); });
        _mainPanel._1_tog.onValueChanged.RemoveAllListeners();
        _mainPanel._1_tog.onValueChanged.AddListener((ison) =>
        {
            if (ison)
            {
                PlayerPrefs.SetInt("Kinds", 1); PlayerPrefs.Save(); _mainPanel.ScreenGrid_tog.isOn = false;
                if (IsHFullScreen)
                {
                    HFullScreenListener(true);
                    RecycleORLoadRendervideo(1);
                }
                else
                {
                    HFullScreenListener(false);
                    SetGridParts(1);
                }
            }
            //else
            //{
            //    for (int i = 0; i < _mainPanel.Grid.transform.childCount; i++)
            //    {
            //        ObjectManager.Instance.ReleaseObject(_mainPanel.Grid.transform.GetChild(i).gameObject);
            //    }
            //    videoRenderList.Clear();
            //}

        });
        _mainPanel._4_tog.onValueChanged.RemoveAllListeners();
        _mainPanel._4_tog.onValueChanged.AddListener((ison) =>
        {
            if (ison)
            {
                PlayerPrefs.SetInt("Kinds", 4); PlayerPrefs.Save(); _mainPanel.ScreenGrid_tog.isOn = false;
                if (IsHFullScreen)
                {
                    HFullScreenListener(true);
                    RecycleORLoadRendervideo(2);
                }
                else
                {
                    HFullScreenListener(false);
                    SetGridParts(2);
                }
            }


        });
        _mainPanel._6_tog.onValueChanged.RemoveAllListeners();
        _mainPanel._6_tog.onValueChanged.AddListener((ison) =>
        {

            if (ison)
            {
                if (IsHFullScreen)
                    ExpandSix(true);
                else
                    ExpandSix(false);
                PlayerPrefs.SetInt("Kinds", 6); PlayerPrefs.Save();
                ResetGridVideoRender();
                Container6.gameObject.SetActive(true);
                Container6.transform.localScale = Vector3.one;
                PlayChangeVideo(Container6);
                for (int i = 0; i < Container6.childCount; i++)
                {
                    //int m = i;
                    //Container6.GetChild(m).GetComponent<Toggle>().onValueChanged.AddListener((value) =>
                    //{
                    //    if (value)
                    //    {
                    //        Log.Debug("点击了:" + m);
                    //    }
                    //});
                    //Container6.GetChild(m).GetComponent<UIButtonDoubleClick>().OnDoubleClick.RemoveAllListeners();
                    //Container6.GetChild(m).GetComponent<UIButtonDoubleClick>().OnDoubleClick.AddListener(() =>
                    //{

                    //    Log.Debug("双击了:" + m);
                    //    Container6.GetChild(m).DOLocalMove(new Vector3(-0.5f, 0.5f, 0), 0.2f);
                    //    Container6.GetChild(m).GetComponent<RectTransform>().DOSizeDelta(new Vector2(1557, 931), 0.2f);
                    //    Container6.GetChild(m).SetAsLastSibling();
                    //});
                    videoRenderList.Add(Container6.GetChild(i).gameObject);
                }
            }
            else
            {

                for (int i = 0; i < Container6.childCount; i++)
                {
                    if (Container6.GetChild(i).GetComponent<SecurityCamera>().player != null && Container6.GetChild(i).GetComponent<SecurityCamera>().player.IsRealPlaying)
                    {
                        Container6.GetChild(i).GetComponent<SecurityCamera>().Pause();

                    }
                }
                Container6.transform.localScale = Vector3.zero;
                Container6.gameObject.SetActive(false);
                _mainPanel.Grid.gameObject.SetActive(true);
                //videoRenderList.Clear();
            }

            _mainPanel.ScreenGrid_tog.isOn = false;
        });
        _mainPanel._8_tog.onValueChanged.RemoveAllListeners();
        _mainPanel._8_tog.onValueChanged.AddListener((ison) =>
        {
            if (ison)
            {
                Log.Error("8 true");
                if (IsHFullScreen)
                    ExpandEight(true);
                else
                    ExpandEight(false);
                PlayerPrefs.SetInt("Kinds", 8); PlayerPrefs.Save();
                ResetGridVideoRender();
                Container8.gameObject.SetActive(true);
                Container8.transform.localScale = Vector3.one;
                PlayChangeVideo(Container8);
                for (int i = 0; i < Container8.childCount; i++)
                {
                    videoRenderList.Add(Container8.GetChild(i).gameObject);
                }
            }
            else
            {
                Log.Error("8 false");
                for (int i = 0; i < Container8.childCount; i++)
                {
                    if (Container8.GetChild(i).GetComponent<SecurityCamera>().player != null && Container8.GetChild(i).GetComponent<SecurityCamera>().player.IsRealPlaying)
                    {
                        Container8.GetChild(i).GetComponent<SecurityCamera>().Pause();

                    }
                }
                Container8.transform.localScale = Vector3.zero;
                Container8.gameObject.SetActive(false);
                _mainPanel.Grid.gameObject.SetActive(true);
                //videoRenderList.Clear();
            }
            _mainPanel.ScreenGrid_tog.isOn = false;

        });
        _mainPanel._13_tog.onValueChanged.RemoveAllListeners();
        _mainPanel._13_tog.onValueChanged.AddListener((ison) =>
        {
            if (ison)
            {
                if (IsHFullScreen)
                    ExpandThirteen(true);
                else
                    ExpandThirteen(false);
                PlayerPrefs.SetInt("Kinds", 13); PlayerPrefs.Save();
                ResetGridVideoRender();
                Container13.gameObject.SetActive(true);
                Container13.transform.localScale = Vector3.one;
                PlayChangeVideo(Container13);
                for (int i = 0; i < Container13.childCount; i++)
                {
                    videoRenderList.Add(Container13.GetChild(i).gameObject);
                }
            }
            else
            {
                for (int i = 0; i < Container13.childCount; i++)
                {
                    if (Container13.GetChild(i).GetComponent<SecurityCamera>().player != null && Container13.GetChild(i).GetComponent<SecurityCamera>().player.IsRealPlaying)
                    {
                        Container13.GetChild(i).GetComponent<SecurityCamera>().Pause();

                    }
                }
                Container13.transform.localScale = Vector3.zero;
                Container13.gameObject.SetActive(false);
                _mainPanel.Grid.gameObject.SetActive(true);
                //videoRenderList.Clear();
            }
            _mainPanel.ScreenGrid_tog.isOn = false;
        });
        _mainPanel._9_tog.onValueChanged.RemoveAllListeners();
        _mainPanel._9_tog.onValueChanged.AddListener((ison) =>
        {
            if (ison)
            {
                PlayerPrefs.SetInt("Kinds", 9); PlayerPrefs.Save(); _mainPanel.ScreenGrid_tog.isOn = false;
                if (IsHFullScreen)
                {
                    Log.Debug("9");
                    HFullScreenListener(true);
                    RecycleORLoadRendervideo(3);
                }
                else
                {

                    SetGridParts(3);
                    HFullScreenListener(false);
                }
            }
            //else
            //{
            //    for (int i = 0; i < _mainPanel.Grid.transform.childCount; i++)
            //    {
            //        ObjectManager.Instance.ReleaseObject(_mainPanel.Grid.transform.GetChild(i).gameObject);
            //    }
            //    //videoRenderList.Clear();
            //}


        });
        _mainPanel._16_tog.onValueChanged.RemoveAllListeners();
        _mainPanel._16_tog.onValueChanged.AddListener((ison) =>
        {
            if (ison)
            {
                PlayerPrefs.SetInt("Kinds", 16); PlayerPrefs.Save(); _mainPanel.ScreenGrid_tog.isOn = false;
                if (IsHFullScreen)
                {
                    HFullScreenListener(true);
                    RecycleORLoadRendervideo(4);
                }
                else
                {
                    HFullScreenListener(false);
                    SetGridParts(4);
                }
            }
            //else
            //{
            //    for (int i = 0; i < _mainPanel.Grid.transform.childCount; i++)
            //    {
            //        ObjectManager.Instance.ReleaseObject(_mainPanel.Grid.transform.GetChild(i).gameObject);
            //    }
            //    //videoRenderList.Clear();
            //}

        });
        _mainPanel._25_tog.onValueChanged.RemoveAllListeners();
        _mainPanel._25_tog.onValueChanged.AddListener((ison) =>
        {
            if (ison)
            {
                PlayerPrefs.SetInt("Kinds", 25); PlayerPrefs.Save(); _mainPanel.ScreenGrid_tog.isOn = false;
                if (IsHFullScreen)
                {
                    HFullScreenListener(true);
                    RecycleORLoadRendervideo(5);
                }
                else
                {
                    HFullScreenListener(false);
                    SetGridParts(5);
                }
            }
            //else
            //{
            //    for (int i = 0; i < _mainPanel.Grid.transform.childCount; i++)
            //    {
            //        ObjectManager.Instance.ReleaseObject(_mainPanel.Grid.transform.GetChild(i).gameObject);
            //    }
            //    //videoRenderList.Clear();
            //}

        });

        _mainPanel._32_tog.onValueChanged.RemoveAllListeners();
        _mainPanel._32_tog.onValueChanged.AddListener((ison) =>
        {
            if (ison)
            {
                if (IsHFullScreen)
                    ExpandThirtytwo(true);
                else
                    ExpandThirtytwo(false);
                PlayerPrefs.SetInt("Kinds", 32); PlayerPrefs.Save();
                ResetGridVideoRender();
                Container32.gameObject.SetActive(true);
                Container32.transform.localScale = Vector3.one;
                PlayChangeVideo(Container32);
                for (int i = 0; i < Container32.childCount; i++)
                {
                    videoRenderList.Add(Container32.GetChild(i).gameObject);
                }
            }
            else
            {
                for (int i = 0; i < Container32.childCount; i++)
                {
                    if (Container32.GetChild(i).GetComponent<SecurityCamera>().player != null && Container32.GetChild(i).GetComponent<SecurityCamera>().player.IsRealPlaying)
                    {
                        Container32.GetChild(i).GetComponent<SecurityCamera>().Pause();

                    }
                }
                Container32.transform.localScale = Vector3.zero;
                Container32.gameObject.SetActive(false);
                _mainPanel.Grid.gameObject.SetActive(true);
                //videoRenderList.Clear();
            }
            _mainPanel.ScreenGrid_tog.isOn = false;

        });
        _mainPanel._36_tog.onValueChanged.RemoveAllListeners();
        _mainPanel._36_tog.onValueChanged.AddListener((ison) =>
        {
            if (ison)
            {
                PlayerPrefs.SetInt("Kinds", 36); PlayerPrefs.Save(); _mainPanel.ScreenGrid_tog.isOn = false;
                if (IsHFullScreen)
                {
                    HFullScreenListener(true);
                    RecycleORLoadRendervideo(6);
                }
                else
                {
                    HFullScreenListener(false);
                    SetGridParts(6);
                }
            }
            //else
            //{
            //    for (int i = 0; i < _mainPanel.Grid.transform.childCount; i++)
            //    {
            //        ObjectManager.Instance.ReleaseObject(_mainPanel.Grid.transform.GetChild(i).gameObject);
            //    }
            //    //videoRenderList.Clear();
            //}

        });
        _mainPanel._64_tog.onValueChanged.RemoveAllListeners();
        _mainPanel._64_tog.onValueChanged.AddListener((ison) =>
        {
            if (ison)
            {
                PlayerPrefs.SetInt("Kinds", 64); PlayerPrefs.Save(); _mainPanel.ScreenGrid_tog.isOn = false;
                if (IsHFullScreen)
                {
                    HFullScreenListener(true);
                    RecycleORLoadRendervideo(8);


                }
                else
                {
                    HFullScreenListener(false);
                    SetGridParts(8);
                }
            }
            //else
            //{
            //    for (int i = 0; i < _mainPanel.Grid.transform.childCount; i++)
            //    {
            //        ObjectManager.Instance.ReleaseObject(_mainPanel.Grid.transform.GetChild(i).gameObject);
            //    }
            //    //videoRenderList.Clear();
            //}
        });
        #endregion
    }
    /// <summary>
    /// 切换特殊比例后Grid下Cameraplayer置空
    /// </summary>
    void ResetGridVideoRender()
    {
        //ClearGridMask();
        int count = _mainPanel.Grid.transform.childCount;
        for (int i = count - 1; i >= 0; i--)
        {
            if (_mainPanel.Grid.transform.GetChild(i).GetComponent<SecurityCamera>().player != null && _mainPanel.Grid.transform.GetChild(i).GetComponent<SecurityCamera>().player.IsRealPlaying)
            {
                _mainPanel.Grid.transform.GetChild(i).GetComponent<SecurityCamera>().OnExitCamera();
            }
            ObjectManager.Instance.ReleaseObject(_mainPanel.Grid.transform.GetChild(i).gameObject);
            //ObjectManager.Instance.ReleaseObject(_mainPanel.GridMask.transform.GetChild(i).gameObject);

        }
        videoRenderList.Clear();
        _mainPanel.Grid.gameObject.SetActive(false);
    }
    //读取本地保存的屏幕分割数据
    void ReadPrefsData()
    {
        Log.Error("读取分割数据存储：" + PlayerPrefs.GetInt("Kinds"));
        int kind_id = PlayerPrefs.GetInt("Kinds");
        if (kind_id != 0)
        {
            switch (kind_id)
            {
                case 1:
                    _mainPanel._1_tog.isOn = true;
                    break;
                case 4:
                    _mainPanel._4_tog.isOn = true;
                    break;
                case 6:
                    _mainPanel._6_tog.isOn = true;
                    break;
                case 8:
                    _mainPanel._8_tog.isOn = true;
                    break;
                case 9:
                    _mainPanel._9_tog.isOn = true;
                    break;
                case 13:
                    _mainPanel._13_tog.isOn = true;
                    break;
                case 16:
                    _mainPanel._16_tog.isOn = true;
                    break;
                case 25:
                    _mainPanel._25_tog.isOn = true;
                    break;
                case 32:
                    _mainPanel._32_tog.isOn = true;
                    break;
                case 36:
                    _mainPanel._36_tog.isOn = true;
                    break;
                case 64:
                    _mainPanel._64_tog.isOn = true;
                    break;
                default:
                    break;
            }

        }
        else
        {
            _mainPanel._1_tog.onValueChanged.Invoke(true);
        }
    }

    #region 比例分割
    /// <summary>
    /// 设置不同比例下的Gridlayout
    /// </summary>
    /// <param name="row"></param>
    void SetGridParts(int row)
    {
        _mainPanel.Grid.gameObject.SetActive(true);
        RecycleORLoadRendervideo(row);
        switch (row)
        {
            case 1:
                _mainPanel.Grid.cellSize = new Vector2(1553, 933);
                _mainPanel.Grid.constraintCount = 1;
                //_mainPanel.GridMask.cellSize = new Vector2(1553, 933);
                //_mainPanel.GridMask.constraintCount = 1;
                break;
            case 2:
                _mainPanel.Grid.cellSize = new Vector2(778.5f, 465.5f);
                _mainPanel.Grid.constraintCount = 2;
                //_mainPanel.GridMask.cellSize = new Vector2(778.5f, 465.5f);
                //_mainPanel.GridMask.constraintCount = 2;
                break;
            case 3:
                _mainPanel.Grid.cellSize = new Vector2(518.7f, 310);
                _mainPanel.Grid.constraintCount = 3;
                //_mainPanel.GridMask.cellSize = new Vector2(518.7f, 310);
                //_mainPanel.GridMask.constraintCount = 3;
                break;
            case 4:
                _mainPanel.Grid.cellSize = new Vector2(388.75f, 232.25f);
                _mainPanel.Grid.constraintCount = 4;
                //_mainPanel.GridMask.cellSize = new Vector2(388.75f, 232.25f);
                //_mainPanel.GridMask.constraintCount = 4;
                break;
            case 5:
                _mainPanel.Grid.cellSize = new Vector2(310.8f, 185.6f);
                _mainPanel.Grid.constraintCount = 5;
                //_mainPanel.GridMask.cellSize = new Vector2(310.8f, 185.6f);
                //_mainPanel.GridMask.constraintCount = 5;
                break;
            case 6:
                _mainPanel.Grid.cellSize = new Vector2(258.83f, 154.5f);
                _mainPanel.Grid.constraintCount = 6;
                //_mainPanel.GridMask.cellSize = new Vector2(258.83f, 154.5f);
                //_mainPanel.GridMask.constraintCount = 6;
                break;
            case 8:
                _mainPanel.Grid.cellSize = new Vector2(193.875f, 115.625f);
                _mainPanel.Grid.constraintCount = 8;
                //_mainPanel.GridMask.cellSize = new Vector2(193.875f, 115.625f);
                //_mainPanel.GridMask.constraintCount = 8;
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 回收到对象池
    /// </summary>
    private void RecycleORLoadRendervideo(int count)
    {
        int childcount = _mainPanel.Grid.transform.childCount;
        int totalcount = count * count;
        if (childcount > 0)
        {
            int needLoadcount = childcount - totalcount;
            Log.Error("childcount:" + childcount + " totalcount:" + totalcount + "value:" + (needLoadcount > 0));
            if (needLoadcount > 0)//回收
            {
                for (int i = needLoadcount; i > 0; i--)
                {
                    if (_mainPanel.Grid.transform.GetChild(i).GetComponent<SecurityCamera>().player != null && _mainPanel.Grid.transform.GetChild(i).GetComponent<SecurityCamera>().player.IsRealPlaying)
                    {
                        _mainPanel.Grid.transform.GetChild(i).GetComponent<SecurityCamera>().OnExitCamera();
                        //_mainPanel.Grid.transform.GetChild(i).GetComponent<SecurityCamera>().monitor = null;
                    }

                    ObjectManager.Instance.ReleaseObject(_mainPanel.Grid.transform.GetChild(i).gameObject);
                    //ObjectManager.Instance.ReleaseObject(_mainPanel.GridMask.transform.GetChild(i).gameObject);
                    //if (i < videoRenderList.Count)
                    //{
                    //Log.Error("videoRenderList.Count:"+videoRenderList.Count+"   回收i:"+i);
                    videoRenderList.RemoveAt(i);
                    //}


                }

            }
            else //加载 
            {
                Log.Error("切换的是正常比例");
                for (int i = 0; i < Mathf.Abs(needLoadcount); i++)
                {

                    ObjectManager.Instance.InstantiateObjectAsync(ConStr.RENDERVIDEO, RenderFinish, LoadResPriority.RES_MIDDLE);

                }

            }

        }
        else
        {
            Log.Error("切换的是异形比例");
            videoRenderList.Clear();
            for (int i = 0; i < totalcount; i++)
            {
                ObjectManager.Instance.InstantiateObjectAsync(ConStr.RENDERVIDEO, RenderFinish, LoadResPriority.RES_MIDDLE);
            }

        }
        PlayChangeVideo(_mainPanel.Grid.transform);

    }
    /// <summary>
    /// 切换特殊屏幕分割比例后继承视频播放功能。
    /// </summary>
    void PlayChangeVideo(Transform videoContainer)
    {

        if (_mainPanel.SaveCameraRenderDic.Count != 0)
        {

            for (int i = 0; i < _mainPanel.SaveCameraRenderDic.Count; i++)
            {

                if (videoContainer.childCount > i && videoContainer.GetChild(i).GetComponent<SecurityCamera>().player == null)
                {
                    videoContainer.GetChild(i).GetComponent<SecurityCamera>().host = _mainPanel.SaveCameraRenderDic.ElementAt(i).Value.host;
                    videoContainer.GetChild(i).GetComponent<SecurityCamera>().sdk = _mainPanel.SaveCameraRenderDic.ElementAt(i).Value.type;
                    videoContainer.GetChild(i).GetComponent<SecurityCamera>().channel = _mainPanel.SaveCameraRenderDic.ElementAt(i).Value.channel;
                    videoContainer.GetChild(i).GetComponent<SecurityCamera>().steamType = STREAM.EXTRA;
                    videoContainer.GetChild(i).GetComponent<SecurityCamera>().passward = _mainPanel.SaveCameraRenderDic.ElementAt(i).Value.password;
                    videoContainer.GetChild(i).GetComponent<SecurityCamera>().SetupPlayer();
                    videoContainer.GetChild(i).GetComponent<SecurityCamera>().devid = _mainPanel.SaveCameraRenderDic.ElementAt(i).Value.id;
                    videoContainer.GetChild(i).GetComponent<SecurityCamera>().PlayReal();

                }
                else
                {
                    PlayChangeVideoReplay(videoContainer);
                }


            }
        }


    }
    /// <summary>
    /// 如果player不为空表示该videorender已经被初始化，不能进行隐藏再显示操作
    /// </summary>
    /// <param name="videoContainer"></param>
    void PlayChangeVideoReplay(Transform videoContainer)
    {
        for (int i = 0; i < videoContainer.childCount; i++)
        {
            if (videoContainer.childCount > i && videoContainer.GetChild(i).GetComponent<SecurityCamera>().player != null)
                videoContainer.GetChild(i).GetComponent<SecurityCamera>().Resume();
        }
    }
    /// <summary>
    /// 异步加载rendervideo回调
    /// </summary>
    /// <param name="path"></param>
    /// <param name="obj"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    GameObject render_preright;
    //获取该视频窗口对应的设备id
    public static int devid = 0;
    //选择配置监控人脸检测框算法的rtsp视频流
    string facertsp;
    private void RenderFinish(string path, UnityEngine.Object obj, object param1, object param2, object param3)
    {
        GameObject render_pre = (obj as GameObject);
        render_pre.transform.SetParent(_mainPanel.Grid.transform);
        render_pre.transform.localScale = new Vector3(1, 1, 1);
        render_pre.transform.Find("mask").GetComponent<Text>().text = LoginManager.UserName;
        videoRenderList.Add(render_pre);
        render_pre.transform.Find("mousepanel/facerec").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        render_pre.transform.Find("mousepanel/facerec").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
        {

            render_pre.transform.Find("mousepanel").gameObject.SetActive(false);
            Log.Debug("播放开启人脸识别算法的视频窗口");

            //向算法发送请求Rtsp协议


            if (render_pre.GetComponent<SecurityCamera>().devid != -1)
            {
                FaceData data = new FaceData();
                data.dtype = "face_rec";
                data.state = 1;
                data.uuid = render_pre.GetComponent<SecurityCamera>().devid.ToString();
                if (render_pre.GetComponent<SecurityCamera>().sdk == SDKTYPE.HK)
                {
                    //if (render_pre.GetComponent<SecurityCamera>().host.Split(':')[0].Contains(".26.")|| render_pre.GetComponent<SecurityCamera>().host.Split(':')[0].Contains(".10.")) 
                    //{
                    //    data.rtsp = "rtsp://admin:abcd1234@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/h264/ch1/main/av_stream";
                    //    facertsp = data.rtsp;
                    //}
                    //else 
                    //{
                    //    data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/h264/ch1/main/av_stream";
                    //    facertsp = data.rtsp;
                    //}
                    data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/h264/ch1/main/av_stream";
                    facertsp = data.rtsp;
                }
                else if (render_pre.GetComponent<SecurityCamera>().sdk == SDKTYPE.DH)
                {
                    //data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/cam/realmonitor?channel=1&subtype=0";
                    data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/cam/realmonitor?channel=1&subtype=0";
                    facertsp = data.rtsp;
                }
                else if (render_pre.GetComponent<SecurityCamera>().sdk == SDKTYPE.YS)
                {
                    //data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/media/video1";
                    data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/media/video1";
                    facertsp = data.rtsp;
                }
                ReqFaceSevData reqFaceSevData = new ReqFaceSevData();
                reqFaceSevData.requestMgrHostIp = "192.168.110.2";
                reqFaceSevData.requestMgrHostPort = "77111";
                reqFaceSevData.requestFaceRecInfoList.Add(data);
                string reqstr = JsonUtility.ToJson(reqFaceSevData, true);
                Log.Debug("向算法服务发送开启人脸检测算法：" + reqstr);
                HttpNetManager.GetInstance().SendDataStr(AppRuntimeConfig.Settings.faceServiceUrl, RequestFaceSevCallback, true, true, false, reqstr);
            }


        });
        render_pre.transform.Find("mousepanel/peoplegroup").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        render_pre.transform.Find("mousepanel/peoplegroup").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
        {

            render_pre.transform.Find("mousepanel").gameObject.SetActive(false);
            Log.Debug("播放开启人员聚集算法的视频窗口");
            _mainPanel.PlayfaceImage.gameObject.SetActive(true);
            _mainPanel.loadingani.gameObject.SetActive(true);
            _mainPanel.loadingani.Play("loadani");
            //向算法发送请求Rtsp协议



            if (render_pre.GetComponent<SecurityCamera>().devid != -1)
            {
                FaceData data = new FaceData();
                data.dtype = "person_group";
                data.state = 1;
                data.uuid = render_pre.GetComponent<SecurityCamera>().devid.ToString();
                if (render_pre.GetComponent<SecurityCamera>().sdk == SDKTYPE.HK)
                {
                    data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/h264/ch1/main/av_stream";
                    facertsp = data.rtsp;
                }
                else if (render_pre.GetComponent<SecurityCamera>().sdk == SDKTYPE.DH)
                {
                    data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/cam/realmonitor?channel=1&subtype=0";
                    facertsp = data.rtsp;
                }
                else if (render_pre.GetComponent<SecurityCamera>().sdk == SDKTYPE.YS)
                {
                    data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/media/video1";
                    facertsp = data.rtsp;
                }
                ReqFaceSevData reqFaceSevData = new ReqFaceSevData();
                reqFaceSevData.requestMgrHostIp = "192.168.110.2";
                reqFaceSevData.requestMgrHostPort = "77111";
                reqFaceSevData.requestFaceRecInfoList.Add(data);
                string reqstr = JsonUtility.ToJson(reqFaceSevData, true);
                Log.Debug("向算法服务发送开启人员聚集算法：" + reqstr);
                HttpNetManager.GetInstance().SendDataStr(AppRuntimeConfig.Settings.faceServiceUrl, RequestpersonGroupCallback, true, true, false, reqstr);
            }


        });
        render_pre.transform.Find("mousepanel/peopleru").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        render_pre.transform.Find("mousepanel/peopleru").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
        {

            render_pre.transform.Find("mousepanel").gameObject.SetActive(false);
            Log.Debug("播放开启人员闯入算法的视频窗口");
            _mainPanel.PlayfaceImage.gameObject.SetActive(true);
            _mainPanel.loadingani.gameObject.SetActive(true);
            _mainPanel.loadingani.Play("loadani");
            //向算法发送请求Rtsp协议


            if (render_pre.GetComponent<SecurityCamera>().devid != -1)
            {
                FaceData data = new FaceData();
                data.dtype = "person_brust";
                data.state = 1;
                data.uuid = render_pre.GetComponent<SecurityCamera>().devid.ToString();
                if (render_pre.GetComponent<SecurityCamera>().sdk == SDKTYPE.HK)
                {
                    data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/h264/ch1/main/av_stream";
                    facertsp = data.rtsp;
                }
                else if (render_pre.GetComponent<SecurityCamera>().sdk == SDKTYPE.DH)
                {
                    data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/cam/realmonitor?channel=1&subtype=0";
                    facertsp = data.rtsp;
                }
                else if (render_pre.GetComponent<SecurityCamera>().sdk == SDKTYPE.YS)
                {
                    data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/media/video1";
                    facertsp = data.rtsp;
                }
                ReqFaceSevData reqFaceSevData = new ReqFaceSevData();
                reqFaceSevData.requestMgrHostIp = "192.168.110.2";
                reqFaceSevData.requestMgrHostPort = "77111";
                reqFaceSevData.requestFaceRecInfoList.Add(data);
                string reqstr = JsonUtility.ToJson(reqFaceSevData, true);
                Log.Debug("向算法服务发送开启人员闯入检测算法：" + reqstr);
                HttpNetManager.GetInstance().SendDataStr(AppRuntimeConfig.Settings.faceServiceUrl, RequestpersonBrustCallback, true, true, false, reqstr);
            }


        });

        render_pre.transform.Find("mousepanel/peoplefall").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        render_pre.transform.Find("mousepanel/peoplefall").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
        {

            render_pre.transform.Find("mousepanel").gameObject.SetActive(false);
            Log.Debug("播放开启人员跌倒的视频窗口");
            _mainPanel.PlayfaceImage.gameObject.SetActive(true);
            _mainPanel.loadingani.gameObject.SetActive(true);
            _mainPanel.loadingani.Play("loadani");
            //向算法发送请求Rtsp协议


            if (render_pre.GetComponent<SecurityCamera>().devid != -1)
            {
                FaceData data = new FaceData();
                data.dtype = "person_fall";
                data.state = 1;
                data.uuid = render_pre.GetComponent<SecurityCamera>().devid.ToString();
                if (render_pre.GetComponent<SecurityCamera>().sdk == SDKTYPE.HK)
                {
                    data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/h264/ch1/main/av_stream";
                    facertsp = data.rtsp;
                }
                else if (render_pre.GetComponent<SecurityCamera>().sdk == SDKTYPE.DH)
                {
                    data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/cam/realmonitor?channel=1&subtype=0";
                    facertsp = data.rtsp;
                }
                else if (render_pre.GetComponent<SecurityCamera>().sdk == SDKTYPE.YS)
                {
                    data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/media/video1";
                    facertsp = data.rtsp;
                }
                ReqFaceSevData reqFaceSevData = new ReqFaceSevData();
                reqFaceSevData.requestMgrHostIp = "192.168.110.2";
                reqFaceSevData.requestMgrHostPort = "77111";
                reqFaceSevData.requestFaceRecInfoList.Add(data);
                string reqstr = JsonUtility.ToJson(reqFaceSevData, true);
                Log.Debug("向算法服务发送开启人员跌倒检测算法：" + reqstr);
                HttpNetManager.GetInstance().SendDataStr(AppRuntimeConfig.Settings.faceServiceUrl, RequestpersonFallCallback, true, true, false, reqstr);
            }


        });

        render_pre.transform.Find("mousepanel/carplate").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        render_pre.transform.Find("mousepanel/carplate").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
        {

            render_pre.transform.Find("mousepanel").gameObject.SetActive(false);
            Log.Debug("播放开启车牌检测的视频窗口");
            _mainPanel.PlayfaceImage.gameObject.SetActive(true);
            _mainPanel.loadingani.gameObject.SetActive(true);
            _mainPanel.loadingani.Play("loadani");
            //向算法发送请求Rtsp协议


            if (render_pre.GetComponent<SecurityCamera>().devid != -1)
            {
                FaceData data = new FaceData();
                data.dtype = "car_plate";
                data.state = 1;
                data.uuid = render_pre.GetComponent<SecurityCamera>().devid.ToString();
                if (render_pre.GetComponent<SecurityCamera>().sdk == SDKTYPE.HK)
                {
                    data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/h264/ch1/main/av_stream";
                    facertsp = data.rtsp;
                }
                else if (render_pre.GetComponent<SecurityCamera>().sdk == SDKTYPE.DH)
                {
                    data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/cam/realmonitor?channel=1&subtype=0";
                    facertsp = data.rtsp;
                }
                else if (render_pre.GetComponent<SecurityCamera>().sdk == SDKTYPE.YS)
                {
                    data.rtsp = "rtsp://admin:" + render_pre.GetComponent<SecurityCamera>().passward + "@" + render_pre.GetComponent<SecurityCamera>().host.Split(':')[0] + ":554/media/video1";
                    facertsp = data.rtsp;
                }
                ReqFaceSevData reqFaceSevData = new ReqFaceSevData();
                reqFaceSevData.requestMgrHostIp = "192.168.110.2";
                reqFaceSevData.requestMgrHostPort = "77111";
                reqFaceSevData.requestFaceRecInfoList.Add(data);
                string reqstr = JsonUtility.ToJson(reqFaceSevData, true);
                Log.Debug("向算法服务发送开启车牌检测算法：" + reqstr);
                HttpNetManager.GetInstance().SendDataStr(AppRuntimeConfig.Settings.faceServiceUrl, RequestcarPlateCallback, true, true, false, reqstr);
            }


        });


        UIButtonDoubleClick toggle = render_pre.GetComponent<UIButtonDoubleClick>();
        toggle.group = _mainPanel.Grid.GetComponent<ToggleGroup>();
        toggle.OnDoubleClick.RemoveAllListeners();
        int libindex = 0;
        toggle.OnDoubleClick.AddListener(() =>
        {

            if (render_pre.transform.parent.GetComponent<GridLayoutGroup>().enabled)
            {
                render_pre.transform.parent.GetComponent<GridLayoutGroup>().enabled = false;
                render_pre.transform.DOLocalMove(new Vector3(-0.5f, 0.5f, 0), 0.2f);
                render_pre.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1557, 931), 0.2f);
                libindex = render_pre.transform.GetSiblingIndex();
                videoRenderList.RemoveAt(libindex);
                render_pre.transform.SetAsLastSibling();
                videoRenderList.Add(render_pre);
                if (IsConfiguredCamera(render_pre.GetComponent<SecurityCamera>()))
                    CameraPlay(render_pre.GetComponent<SecurityCamera>().host, render_pre.GetComponent<SecurityCamera>().sdk, render_pre.GetComponent<SecurityCamera>().channel, STREAM.MAIN, videoRenderList.Count - 1, render_pre.GetComponent<SecurityCamera>().devid, render_pre.GetComponent<SecurityCamera>().passward);
                _mainPanel.ScreenGrid_tog.interactable = false;
                IsMax = true;
                Log.Debug("双击了   ,IsMax: " + IsMax);
            }
            else
            {
                render_pre.transform.SetSiblingIndex(libindex);
                videoRenderList.Remove(render_pre);
                videoRenderList.Insert(libindex, render_pre);
                render_pre.transform.parent.GetComponent<GridLayoutGroup>().enabled = true;
                Timer.Register(0.2f, () =>
                {
                    if (IsConfiguredCamera(render_pre.GetComponent<SecurityCamera>()))
                        CameraPlay(render_pre.GetComponent<SecurityCamera>().host, render_pre.GetComponent<SecurityCamera>().sdk, render_pre.GetComponent<SecurityCamera>().channel, STREAM.EXTRA, libindex, render_pre.GetComponent<SecurityCamera>().devid, render_pre.GetComponent<SecurityCamera>().passward);

                });
                IsMax = false;
                Log.Debug("双击了   ,IsMax: " + IsMax);
                _mainPanel.ScreenGrid_tog.interactable = true;
                render_pre.transform.GetChild(0).localScale = new Vector3(1, 1, 1);
            }


        });
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener((ison) =>
        {

            render_pre.transform.Find("border").gameObject.SetActive(ison);
            if (ison)
            {
                if (render_pre.transform.GetChild(0).GetComponent<VideoRenderer>().IsRendering)
                {
                    devid = render_pre.GetComponent<SecurityCamera>().devid;

                }
                else
                {
                    devid = -1;
                }
                ChoiceIndex = render_pre.transform.GetSiblingIndex();
                cameraPlayer = render_pre;
                Log.Debug("选择的是:" + ChoiceIndex + "    设备id:" + devid);

                //Log.Error("监控播放Player播放状态："+cameraPlayer.GetComponent<SecurityCamera>().player.IsRealPlaying);


                for (int i = 0; i < allCameraTreeViewItemList.Count; i++)
                {
                    //Debug.LogError("监控层级目录对应设备ID："+ allCameraTreeViewItemList[i].GetComponent<ItemScript>().nvr.id);
                    if (allCameraTreeViewItemList[i].GetComponent<ItemScript>().nvr.id == devid)
                    {
                        allCameraTreeViewItemList[i].GetComponent<ItemScript>().selectImg.gameObject.SetActive(true);
                        Log.Error("选择的监控树级索引:" + i + "count:" + allCameraTreeViewItemList.Count + "    Value: " + 1 % allCameraTreeViewItemList.Count);
                        _mainPanel.MainPreviewTreeView.transform.parent.parent.Find("Scrollbar Vertical").GetComponent<Scrollbar>().value = 1 - i / (float)allCameraTreeViewItemList.Count;
                    }
                    else
                    {
                        allCameraTreeViewItemList[i].GetComponent<ItemScript>().selectImg.gameObject.SetActive(false);
                    }
                }

            }


        });
        toggle.OnRightClick.RemoveAllListeners();
        toggle.OnRightClick.AddListener(() =>
        {

            Log.Debug(render_preright + "     " + render_pre.transform.Find("border").gameObject.activeInHierarchy);
            if (render_preright != null && render_pre.transform.Find("border").gameObject.activeInHierarchy && render_pre.transform.Find("VideoRender").GetComponent<VideoRenderer>().IsRendering)
            {
                render_pre.transform.Find("mousepanel").gameObject.SetActive(true);
                //render_preright.transform.Find("mousepanel").gameObject.SetActive(false);
                render_preright = render_pre;
            }
            else if (render_pre.transform.Find("border").gameObject.activeInHierarchy && render_pre.transform.Find("VideoRender").GetComponent<VideoRenderer>().IsRendering)
            {
                render_pre.transform.Find("mousepanel").gameObject.SetActive(true);
                render_preright = render_pre;
            }

        });
        toggle.OnscrollWheel.RemoveAllListeners();
        toggle.OnscrollWheel.AddListener(() =>
        {
            //Log.Debug("附着UI："+getOverUI());
            if (!render_pre.transform.parent.GetComponent<GridLayoutGroup>().enabled && getOverUI() == "border")
            {
                float mouseCenter = Input.GetAxis("Mouse ScrollWheel");
                if (render_pre.transform.Find("border").gameObject.activeInHierarchy)
                    //Scale(mouseCenter, Input.mousePosition, render_pre.transform.GetChild(0) as RectTransform);
                    SetMouseChangeIamge(render_pre.transform.GetChild(0), mouseCenter);

            }


        });


    }
    public string getOverUI()
    {
        GraphicRaycaster _raycaster = GameStart.FindObjectOfType<GraphicRaycaster>();
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.pressPosition = Input.mousePosition;
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        _raycaster.Raycast(eventData, results);
        if (results.Count > 0)
            return results[0].gameObject.name;
        return "null";
    }

    /// <summary>
    /// 开启人脸检测算法请求数据结构
    /// </summary>
    [Serializable]
    public class ReqFaceSevData
    {
        public string requestMgrHostIp = string.Empty;
        public string requestMgrHostPort = string.Empty;
        public List<FaceData> requestFaceRecInfoList = new List<FaceData>();
    }
    [Serializable]
    public class FaceData
    {
        public string rtsp = string.Empty;
        public string uuid = string.Empty;
        public int state = 0;
        public string dtype = string.Empty;

    }

    /// <summary>
    /// 人脸服务请求回答数据模型
    /// </summary>
    [Serializable]
    public class AnsFacerecData
    {
        public int code;
        public int checkValue;
        public string rtspPath = string.Empty;

    }


    public string DType = string.Empty;//当前正在开启的算法
    /// <summary>
    /// 请求开启人脸识别算法请求回调
    /// </summary>
    private void RequestFaceSevCallback(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {
            Log.Debug("接受：" + args.Value);
            AnsFacerecData ansdata = JsonUtility.FromJson<AnsFacerecData>(args.Value);
            if (ansdata.code == 200)
            {


                _mainPanel.PlayfaceImage.gameObject.SetActive(true);
                _mainPanel.loadingani.gameObject.SetActive(true);
                _mainPanel.loadingani.Play("loadani");


                DType = "face_rec";
                if (ansdata.checkValue == 1)
                {
                    GameStart.Instance.ShowTip("开启人脸识别算法成功，正在加载，请稍等...");
                    string faceRtsp = ansdata.rtspPath;
                    PlayVideo(faceRtsp);
                }
                else
                {
                    GameStart.Instance.ShowTip("请求成功,但配置人脸识别算法失败，请尝试先关闭请求再开启一次");
                }
            }
            else
            {
                GameStart.Instance.ShowTip("请求配置人脸识别算法失败,请检查网络!");
                //PlayVideo("rtsp://admin:password@" +"192.168.110.131:554" + "/h264/ch1/main/av_stream");
            }
        }

    }

    private void RequestpersonGroupCallback(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {

            AnsFacerecData ansdata = JsonUtility.FromJson<AnsFacerecData>(args.Value);
            if (ansdata.code == 200)
            {
                DType = "person_group";
                if (ansdata.checkValue == 1)
                {
                    GameStart.Instance.ShowTip("开启人员聚集算法成功，正在加载，请稍等...");
                    string faceRtsp = ansdata.rtspPath;
                    PlayVideo(faceRtsp);
                }
                else
                {
                    GameStart.Instance.ShowTip("请求成功,但配置人员聚集算法失败，请尝试先关闭请求再开启一次");
                }
            }
            else
            {
                GameStart.Instance.ShowTip("请求配置人员聚集算法失败,请检查网络!");
                //PlayVideo("rtsp://admin:password@" +"192.168.110.131:554" + "/h264/ch1/main/av_stream");
            }
        }

    }


    private void RequestpersonBrustCallback(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {

            AnsFacerecData ansdata = JsonUtility.FromJson<AnsFacerecData>(args.Value);
            if (ansdata.code == 200)
            {
                DType = "person_brust";
                if (ansdata.checkValue == 1)
                {
                    GameStart.Instance.ShowTip("开启人员闯入算法成功，正在加载，请稍等...");
                    string faceRtsp = ansdata.rtspPath;
                    PlayVideo(faceRtsp);
                }
                else
                {
                    GameStart.Instance.ShowTip("请求成功,但配置人员闯入算法失败，请尝试先关闭请求再开启一次");
                }
            }
            else
            {
                GameStart.Instance.ShowTip("请求配置人员闯入算法失败,请检查网络!");
                //PlayVideo("rtsp://admin:password@" +"192.168.110.131:554" + "/h264/ch1/main/av_stream");
            }
        }

    }



    private void RequestpersonFallCallback(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {

            AnsFacerecData ansdata = JsonUtility.FromJson<AnsFacerecData>(args.Value);
            if (ansdata.code == 200)
            {
                DType = "person_fall";
                if (ansdata.checkValue == 1)
                {
                    GameStart.Instance.ShowTip("开启人员跌倒算法成功，正在加载，请稍等...");
                    string faceRtsp = ansdata.rtspPath;
                    PlayVideo(faceRtsp);
                }
                else
                {
                    GameStart.Instance.ShowTip("请求成功,但配置人员跌倒算法失败，请尝试先关闭请求再开启一次");
                }
            }
            else
            {
                GameStart.Instance.ShowTip("请求配置人员跌倒算法失败,请检查网络!");
                //PlayVideo("rtsp://admin:password@" +"192.168.110.131:554" + "/h264/ch1/main/av_stream");
            }
        }

    }



    private void RequestcarPlateCallback(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {

            AnsFacerecData ansdata = JsonUtility.FromJson<AnsFacerecData>(args.Value);
            if (ansdata.code == 200)
            {
                DType = "car_plate";
                if (ansdata.checkValue == 1)
                {
                    GameStart.Instance.ShowTip("开启车牌检测算法成功，正在加载，请稍等...");
                    string faceRtsp = ansdata.rtspPath;
                    PlayVideo(faceRtsp);
                }
                else
                {
                    GameStart.Instance.ShowTip("请求成功,但配置车牌检测算法失败，请尝试先关闭请求再开启一次");
                }
            }
            else
            {
                GameStart.Instance.ShowTip("请求配置车牌检测算法失败,请检查网络!");
                //PlayVideo("rtsp://admin:password@" +"192.168.110.131:554" + "/h264/ch1/main/av_stream");
            }
        }

    }

    /// <summary>
    /// 关闭人脸识别算法
    /// </summary>
    void closePlayfaceImageLis()
    {

        _mainPanel.PlayfaceImage.gameObject.SetActive(false);

        FaceData data = new FaceData();
        if (!string.IsNullOrEmpty(DType))
            data.dtype = DType;
        data.state = 0;
        data.uuid = devid.ToString();
        data.rtsp = facertsp;
        ReqFaceSevData reqFaceSevData = new ReqFaceSevData();
        reqFaceSevData.requestMgrHostIp = "192.168.110.2";
        reqFaceSevData.requestMgrHostPort = "77111";
        reqFaceSevData.requestFaceRecInfoList.Add(data);
        string reqstr = JsonUtility.ToJson(reqFaceSevData, true);
        Log.Debug("关闭人脸检测算法信息：" + reqstr);
        HttpNetManager.GetInstance().SendDataStr(AppRuntimeConfig.Settings.faceServiceUrl, RequestCloseFaceSevCallback, true, true, false, reqstr);

    }




    private void RequestCloseFaceSevCallback(HttpCallBackArgs args)
    {


        try
        {
            this._mainPanel.PlayfaceImage.GetComponent<RawImage>().color = new Color(0, 0, 0, 255);
            if (!string.IsNullOrEmpty(args.Value))
            {
                Log.Debug("关闭算法接收：" + args.Value);
                AnsFacerecData ansdata = JsonUtility.FromJson<AnsFacerecData>(args.Value);
                if (ansdata.code == 200)
                {
                    Log.Debug("关闭人脸服务请求成功");
                    if (ansdata.checkValue == 1)
                    {
                        GameStart.Instance.ShowTip("关闭检测算法成功");

                    }
                    else
                    {
                        GameStart.Instance.ShowTip("请求成功,但关闭检测算法失败");
                    }
                }
                else
                {
                    GameStart.Instance.ShowTip("请求关闭算法失败,请检查网络!");
                    //PlayVideo("rtsp://admin:password@" +"192.168.110.131:554" + "/h264/ch1/main/av_stream");
                }
            }

        }
        catch (Exception)
        {
        }



    }
    /// <summary>
    /// 播放带有人脸检测框的算法视频
    /// </summary>
    /// <param name="rtsp"></param>
    public void PlayVideo(string rtsp)
    {

        _mainPanel.PlayfaceImage.GetComponent<UniversalMediaPlayer>().AddPlayingEvent(() =>
        {
            _mainPanel.loadingani.gameObject.SetActive(false);
            _mainPanel.PlayfaceImage.GetComponent<RawImage>().color = new Color(255, 255, 255, 255);
        });

        _mainPanel.PlayfaceImage.GetComponent<UniversalMediaPlayer>().Path = rtsp;
        _mainPanel.PlayfaceImage.GetComponent<UniversalMediaPlayer>().Play();

        _mainPanel.PlayfaceImage.GetComponent<UniversalMediaPlayer>().AddStoppedEvent(() =>
        {
            Log.Debug("触发暂停播放");
            _mainPanel.PlayfaceImage.GetComponent<UniversalMediaPlayer>().Play();
        }
        );


        //_mainPanel.PlayfaceImage.GetComponent<UniversalMediaPlayer>().AddBufferingEvent((t) => {

        //    _mainPanel.PlayfaceImage.GetComponent<UniversalMediaPlayer>().Play();
        //});


    }


    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Input.GetMouseButtonDown(1))
        {
            Log.Debug("选择的是:" + ChoiceIndex);

        }
        else if (Input.GetMouseButtonDown(0) && GetOverUI(GameStart.Instance.WindRoot.parent.gameObject) != null && (GetOverUI(GameStart.Instance.WindRoot.parent.gameObject).name == "border" || GetOverUI(GameStart.Instance.WindRoot.parent.gameObject).name == "VideoRender"))
        {
            if (render_preright != null)
                render_preright.transform.Find("mousepanel").gameObject.SetActive(false);


        }

    }
    /// <summary>
    /// 获取鼠标停留处UI
    /// </summary>
    /// <param name="canvas"></param>
    /// <returns></returns>
    public GameObject GetOverUI(GameObject canvas)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        GraphicRaycaster gr = canvas.GetComponent<GraphicRaycaster>();
        List<RaycastResult> results = new List<RaycastResult>();
        gr.Raycast(pointerEventData, results);
        if (results.Count != 0)
        {
            return results[0].gameObject;
        }
        return null;
    }


    /// <summary>
    /// 清除Mask
    /// </summary>
    private void ClearGridMask()
    {
        for (int i = 0; i < _mainPanel.GridMask.transform.childCount; i++)
        {
            _mainPanel.Destroygame(_mainPanel.GridMask.transform.GetChild(i).gameObject);
        }
    }
    /// <summary>
    /// 异步加载rendermask回调
    /// </summary>
    /// <param name="path"></param>
    /// <param name="obj"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    private void RenderMaskFinish(string path, UnityEngine.Object obj, object param1, object param2, object param3)
    {
        GameObject rendermask_pre = (obj as GameObject);

        rendermask_pre.transform.SetParent(_mainPanel.GridMask.transform);
        rendermask_pre.transform.localScale = new Vector3(1, 1, 1);

    }
    #endregion

    private void SetMouseChangeIamge(Transform transform, float value)
    {
        float delX = Input.mousePosition.x - transform.position.x;
        float delY = Input.mousePosition.y - transform.position.y;


        float scaleX = delX / transform.GetComponent<RectTransform>().rect.width / transform.localScale.x;
        float scaleY = delY / transform.GetComponent<RectTransform>().rect.height / transform.localScale.y;
        //if (transform.GetComponent<RectTransform>().localScale.x<=1&& transform.GetComponent<RectTransform>().localScale.y<=1)
        transform.GetComponent<RectTransform>().localScale += Vector3.one * 0.1f * value;
        if (transform.GetComponent<RectTransform>().localScale.x <= 1)
        {
            transform.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        }

        //Vector3 nowVector3 = transform.GetComponent<RectTransform>().localScale;
        //if (nowVector3.x < 1f)
        //{
        //    transform.GetComponent<RectTransform>().localScale = Vector3.one ;
        //}
        //if (nowVector3.x > 3f)
        //{
        //    transform.GetComponent<RectTransform>().localScale = Vector3.one * 3f;
        //}
        //transform.GetComponent<RectTransform>().pivot += new Vector2(scaleX, scaleY);
        //transform.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(delX, delY, 0);

    }

    public void Scale(float cur_Scare, Vector2 center_ScreenPos, RectTransform rect)

    {

        //缩放中心屏幕坐标转RectTransform下坐标

        Vector2 center_Local;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, center_ScreenPos, Camera.main, out center_Local);

        //计算中心坐标的当前位置

        float old_Scare = rect.localScale.x; //原缩放比例

        float old_px = center_Local.x / old_Scare;

        float old_py = center_Local.y / old_Scare;

        float cur_pxPos = old_px * cur_Scare;

        float cur_pyPos = old_py * cur_Scare;

        //计算偏移量

        float deltaX = center_Local.x - cur_pxPos;

        float deltaY = center_Local.y - cur_pyPos;

        rect.localScale = new Vector3(cur_Scare, cur_Scare, 1);

        rect.localPosition += new Vector3(deltaX, deltaY, 0);

    }



}

public enum MenuType
{
    EditorVesion,
    CameraVesion

}

