using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using zFramework.Media;
using UnityEngine.EventSystems;
using System;
using System.IO;
using SuperTreeView;
using UnityEngine.Playables;
using UIWidgets;

public class MainPreviewPanel : MonoBehaviour
{
    #region 视频分割Tog
    public Toggle ScreenGrid_tog;
    public Toggle Shutdown_tog;
    public Toggle fullscreen_tog;
    public Toggle set_tog;
    public Toggle sound_tog;
    public Toggle save_tog;
    public Toggle _1_tog; //1364X912
    public Toggle _4_tog;// 682X456
    public Toggle _6_tog;//
    public Toggle _8_tog;//
    public Toggle _9_tog;//454X304
    public Toggle _13_tog;
    public Toggle _16_tog;//341X228
    public Toggle _25_tog;//272X182
    public Toggle _32_tog;
    public Toggle _36_tog;//227X152
    public Toggle _64_tog;//170X114
    public GridLayoutGroup Grid;
    public GridLayoutGroup GridMask;//视频点选遮罩
    public Toggle hfullscreenTog;
    public GameObject _screengrid_bg;

    #endregion


    #region UI :left_bg
    public GameObject leftbgmask;
    public Toggle res_tog;
    //public Toggle more_tog;
    public Toggle patrol_tog;
    public GameObject patrolscrollview;
    public TMP_InputField search_input;
    public Button searchBtn;
    public Button clearBtn;
    public Button editorvsion_btn;
    public Button Newvsion_btn;
    public Transform MenuContainer;
    public GameObject ResScrollview;
    public GameObject patrolsgroup;
    //public GameObject patrolallCamera;
    #endregion


    //云台
    public Button upbtn;
    public Button downbtn;
    public Button leftbtn;
    public Button rightbtn;
    public GameObject PTZImg;
    public Button upExpandbtn;
    public Button downExpandbtn;

    //人脸识别算法反馈生成父节点
    public Transform AlthorimP;//人脸识别信息推送
    public Transform AlarmP;
    public Toggle alarm_tog;
    public Toggle face_tog;
    public Transform faceImg;
    public Button closebtn;
    public Animator loadingani;

    public Transform PlayfaceImage;
    public SuperTreeView.TreeView MainPreviewTreeView;
    public  static  bool IstartFaceMesg=false;
    public void Start()
    {
        EventCenter.addlistener<althorimData>(Eventdefine.FacemesgListener, FaceMesgCallback);
        closebtn.onClick.AddListener(() =>
        {
            faceImg.gameObject.SetActive(false);

        });
        alarm_tog.onValueChanged.AddListener((ison)=>
        {
            if (ison)
            {
                alarm_tog.GetComponent<PlayableDirector>().Stop();
                alarm_tog.GetComponent<PlayableDirector>().initialTime = 0;
            }
        
        });
        face_tog.onValueChanged.AddListener((ison) => 
        {
            if (ison)
            {
                IstartFaceMesg = true;


            }
            else 
            {
                IstartFaceMesg = false;
            }
        
        });
      
    }

    public void FaceMesgCallback(althorimData data)
    {



        Log.Error("事件类型："+data.type+"  设备id:"+data.camId+"  当前选择的设备id:"+MainPreviewUi.devid);

        switch (data.type)
        {
            case "face_rec":
                
                if (int.Parse(data.camId) == MainPreviewUi.devid)   //过滤当前选择播放的设备id
                {
                    GameObject Mesg1 = ObjectManager.Instance.InstantiateObject(ConStr.FACEMESG);
                    Mesg1.transform.SetParent(AlthorimP);
                    Mesg1.transform.localScale = Vector3.one;


                    Mesg1.GetComponent<TextMeshProUGUI>().text = "<color=green>提示</color>：" + "<color=white>" + data.userName + "  经过了:  " + data.camName + "</color>";
                    if (!Directory.Exists("c:/faceevent"))
                    {
                        Directory.CreateDirectory("c:/faceevent");
                    }
                    SaveFacePic(data.img, "c:/faceevent", data.idNum);
                    Mesg1.transform.Find("deatlbtn").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        Log.Debug("点击了id为：" + data.idNum + "的详情");
                        faceImg.gameObject.SetActive(true);
                        faceImg.GetComponent<Image>().sprite = GetCoverImg(data.idNum);
                    });
                }
                break;
            //case "car_plate":
            //    if (data.camId.Equals(MainPreviewUi.devid))   //过滤当前选择播放的设备id
            //    {
            //        GameObject Mesg2 = ObjectManager.Instance.InstantiateObject(ConStr.FACEMESG);
            //        Mesg2.transform.SetParent(AlthorimP);
            //        Mesg2.transform.localScale = Vector3.one;


            //        Mesg2.GetComponent<TextMeshProUGUI>().text = "<color=green>提示</color>：" + "<color=white>" + data.userName + "  经过了:  " + data.camName + "</color>";
            //        if (!File.Exists("c:/faceevent"))
            //        {
            //            File.Create("c:/faceevent");
            //        }
            //        SaveFacePic(data.img, "c:/faceevent", data.idNum);
            //        Mesg2.transform.Find("deatlbtn").GetComponent<Button>().onClick.AddListener(() =>
            //        {
            //            Log.Debug("点击了id为：" + data.idNum + "的详情");
            //            faceImg.gameObject.SetActive(true);
            //            faceImg.GetComponent<Image>().sprite = GetCoverImg(data.idNum);
            //        });
            //    }
            //    break;
            case "person_group":
                //闪烁
                alarm_tog.GetComponent<PlayableDirector>().Play();
             

                GameObject Mesg2 = ObjectManager.Instance.InstantiateObject(ConStr.FACEMESG);
                Mesg2.transform.SetParent(AlarmP);
                Mesg2.transform.localScale = Vector3.one;


                Mesg2.GetComponent<TextMeshProUGUI>().text = "<color=red>报警提示</color>：" + "<color=white> 人员聚集报警! ——>地点: " + data.camName + "</color>";
                if (!Directory.Exists("c:/faceevent"))
                {
                    Directory.CreateDirectory("c:/faceevent");
                }
                if (string.IsNullOrEmpty( data.img))
                SaveFacePic(data.img, "c:/faceevent", data.idNum);
                Mesg2.transform.Find("deatlbtn").GetComponent<Button>().onClick.AddListener(() =>
                {
  
                    faceImg.gameObject.SetActive(true);
                    faceImg.GetComponent<Image>().sprite = GetCoverImg(data.idNum);
                });
                break;
            case "person_brust":
                //闪烁
                alarm_tog.GetComponent<PlayableDirector>().Play();

                GameObject Mesg3 = ObjectManager.Instance.InstantiateObject(ConStr.FACEMESG);
                Mesg3.transform.SetParent(AlarmP);
                Mesg3.transform.localScale = Vector3.one;


                Mesg3.GetComponent<TextMeshProUGUI>().text = "<color=red>报警提示</color>：" + "<color=white> 人员闯入报警! ——>地点: " + data.camName + "</color>";
                if (!Directory.Exists("c:/faceevent"))
                {
                    Directory.CreateDirectory("c:/faceevent");
                }
                if (string.IsNullOrEmpty(data.img))
                    SaveFacePic(data.img, "c:/faceevent", data.idNum);
                Mesg3.transform.Find("deatlbtn").GetComponent<Button>().onClick.AddListener(() =>
                {

                    faceImg.gameObject.SetActive(true);
                    faceImg.GetComponent<Image>().sprite = GetCoverImg(data.idNum);
                });
                break;
            case "fire_smoke":
                //闪烁
                alarm_tog.GetComponent<PlayableDirector>().Play();

                GameObject Mesg4 = ObjectManager.Instance.InstantiateObject(ConStr.FACEMESG);
                Mesg4.transform.SetParent(AlarmP);
                Mesg4.transform.localScale = Vector3.one;


                Mesg4.GetComponent<TextMeshProUGUI>().text = "<color=red>报警提示</color>：" + "<color=white> 火灾烟雾报警! ——>地点: " + data.camName + "</color>";
                if (!Directory.Exists("c:/faceevent"))
                {
                    Directory.CreateDirectory("c:/faceevent");
                }
                if (string.IsNullOrEmpty(data.img))
                    SaveFacePic(data.img, "c:/faceevent", data.idNum);
                Mesg4.transform.Find("deatlbtn").GetComponent<Button>().onClick.AddListener(() =>
                {

                    faceImg.gameObject.SetActive(true);
                    faceImg.GetComponent<Image>().sprite = GetCoverImg(data.idNum);
                });
                break;
            case "person_fall":
                //闪烁
                alarm_tog.GetComponent<PlayableDirector>().Play();

                GameObject Mesg5 = ObjectManager.Instance.InstantiateObject(ConStr.FACEMESG);
                Mesg5.transform.SetParent(AlarmP);
                Mesg5.transform.localScale = Vector3.one;


                Mesg5.GetComponent<TextMeshProUGUI>().text = "<color=red>报警提示</color>：" + "<color=white> 人员摔倒! ——>地点: " + data.camName + "</color>";
                if (!Directory.Exists("c:/faceevent"))
                {
                    Directory.CreateDirectory("c:/faceevent");
                }
                if (string.IsNullOrEmpty(data.img))
                    SaveFacePic(data.img, "c:/faceevent", data.idNum);
                Mesg5.transform.Find("deatlbtn").GetComponent<Button>().onClick.AddListener(() =>
                {

                    faceImg.gameObject.SetActive(true);
                    faceImg.GetComponent<Image>().sprite = GetCoverImg(data.idNum);
                });
                break;


            default:
                break;
        }





    }
    public Sprite GetCoverImg(string fileName)
    {
        Texture2D texture2D_1 = new Texture2D(2560, 1440);
        FileStream fs = File.Open(@"c:\faceevent\" + fileName + ".jpg", FileMode.Open, FileAccess.Read);
        byte[] bytes = new byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);
        texture2D_1.LoadImage(bytes);

        Sprite sprite = Sprite.Create(texture2D_1, new Rect(0, 0, texture2D_1.width, texture2D_1.height), new Vector2(0.5f, 0.5f));
        fs.Close();
        return sprite;
    }
    /// <summary>
    /// Base64编码转为人脸图片保存本地
    /// </summary>
    /// <param name="base64Str"></param>
    /// <param name="savePath"></param>
    /// <param name="idnum"></param>
    public void SaveFacePic(string base64Str, string savePath, string idnum)
    {

        try
        {
            Texture2D texture = new Texture2D(2, 2);
            // 将Base64字符串转换为字节数组
            byte[] bytes = Convert.FromBase64String(base64Str);

            // 创建一个新的Texture2D对象并将字节数组加载到其中
            texture.LoadImage(bytes);
            // 将纹理编码为PNG格式的字节数组
            byte[] pngBytes = texture.EncodeToJPG();
            string pname = idnum + ".jpg";
            string path = Path.Combine(savePath, pname);
            File.WriteAllBytes(path, pngBytes);
            Destroy(texture);
             Log.Debug("保存图片到" + path);


        }
        catch (Exception e)
        {
             Log.Debug("保存图片失败: " + e.Message);
        }
        //yield return new WaitForEndOfFrame();

    }

    public void SetContentSizeActive()
    {
        StartCoroutine(HelpSet());//及时触发
    }

    IEnumerator HelpSet()
    {
        MenuContainer.GetComponent<ContentSizeFitter>().enabled = false;
        yield return null;
        MenuContainer.GetComponent<ContentSizeFitter>().enabled = true;
    }
    public Dictionary<string, NVRInformation> SaveCameraRenderDic = new Dictionary<string, NVRInformation>();
    public void Destroygame(GameObject obj)
    {
        Destroy(obj);
    }


    private void OnDestroy()
    {
        SaveCameraRenderDic.Clear();
        EventCenter.RemoveListener<althorimData>(Eventdefine.FacemesgListener, FaceMesgCallback);
    }


}
