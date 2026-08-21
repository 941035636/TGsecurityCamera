using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Vectrosity;
namespace SpringGUI
{
    public class HotspotManger : MonoBehaviour
    {

        string url = "http://192.168.110.2:7711/api/alarm/info?";
        string urldata;
        public List<GameObject> HotspotArr = new List<GameObject>();
        public List<string> ips = new List<string>();
        public InputField inputNumber;//查询编号
        public InputField inputName;//查询姓名
        public Example example;
        public Button btn;
        public Button btnClose;
        public ScrollRect scrollRect;
        public Text camNameText;//右侧信息显示 ，监控位置
        public Vector3 linePosition;
        public bool lineShow=false;
        public static bool ISSHOW = false;

        private void OnEnable()
        {
            EventCenter.addlistener(Eventdefine.ClosePlan, ClosePlan);
            ISSHOW = true;
        }

        void Start()
        {
          
          

            btn.onClick.AddListener(delegate ()
            {
                try
                {
                    urldata = url + "pageNum=0&pageSize=10&userName=" + inputName.text + "&IdNum=" + inputNumber.text + "&eventType=face_rec&cameName=&day=" + "&startTime=" + example.starTimeDate + "&endTime=" + example.endTimeDate;
                    Log.Debug(urldata);
                    InitHotspotArr();
                    StartCoroutine(GetData(urldata));
                    camNameText.text = null;//清空camNameText.text内容

                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                   
                }
              
            });

            btnClose.onClick.AddListener(delegate () {

                ClosePlan();
            });
        }
        private void OnDisable()
        {
            ClosePlan();
            EventCenter.RemoveListener(Eventdefine.ClosePlan, ClosePlan);
          
        }

        /// <summary>
        /// 初始化Hotspot、HoverUIShowName.Trajectory,信息显示面板
        /// </summary>
        public void InitHotspotArr()
        {
            for (int i = 0; i < HotspotArr.Count; i++)
            {
                HotspotArr[i].SetActive(false);//遍历所有对象隐藏
                HotspotArr[i].GetComponent<HoverUIShowName>().Trajectory = null;//遍历父物体下所有子对象的HoverUIShowName.Trajectory的值=null
            }
            scrollRect.content.GetComponentInChildren<Text>().text = null;//清空Text中数据
            this.GetComponent<DrawLines>().SetLine();//初始化线
        }
        /// <summary>
        /// 关闭事件
        /// </summary>
        public void ClosePlan()
        {
            Log.Error("关闭轨迹");
            this.gameObject.SetActive(false);
            InitHotspotArr();
            this.GetComponent<DrawLines>().SetLine();//初始化线
            ISSHOW = false;
        }
        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        IEnumerator GetData(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.isDone)
            {
                if (request.isHttpError || request.isNetworkError)

                     Log.Debug(request.error);
                else
                     Log.Debug(request.downloadHandler.text);
                string response = request.downloadHandler.text;
                HashSet<string> processedParams = new HashSet<string>();
                RecordsData data = JsonUtility.FromJson<RecordsData>(response);
                for (int i = 0; i < data.records.Count; i++)
                {
                    if (data != null && data.records != null && data.records.Count > 0)
                    {
                        MyData record = data.records[i];
                        // Log.Debug("ID: " + record.id);
                        // Log.Debug("CamID: " + record.camId);
                        // Log.Debug("Event Time: " + record.eventTime);
                        // Log.Debug("ID Number: " + record.idNum);
                        // Log.Debug("Person Action: " + record.personAction);
                        // Log.Debug("Is Fire: " + record.isFire);
                        // Log.Debug("Car Number: " + record.carNum);
                        // Log.Debug("Type: " + record.type);
                        // Log.Debug("Name:" + record.username);
                        // Log.Debug("监控地址:" + record.camName);
                        //camNameText.text += "位置："+data.records[i].camName+"\n时间："+data.records[i].eventTime+"\n";

                        //list<string> id=newe list<string>()
                        //list<gameobject> hotspot=newe list<gameobject>();

                        //时间戳转换
                        long timestamp;
                        timestamp = long.Parse(data.records[i].eventTime);
                        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        dateTime = dateTime.AddSeconds(timestamp).ToLocalTime();
                        string formattedDate = dateTime.ToString("yyyy - MM - dd HH: mm:ss");
                        camNameText.text += formattedDate + data.records[i].camName + "\n--->";
                        string paramName = record.camId;
                        

                        GameObject obj = HotspotArr.Find(t => t.name == record.camId);
                        //Text text = obj.GetComponentInChildren<Text>();
                       
                        if (obj != null)
                        {
                            
                            //obj.GetComponentInChildren<Text>().text += "," + (i+1);
                            obj.GetComponent<HoverUIShowName>().Trajectory +=   (i + 1)+",";
                            obj.SetActive(true);
                            //去重
                            if (!processedParams.Contains(paramName))
                            {
                                // 添加参数到已处理集合
                                processedParams.Add(paramName);

                                 Log.Debug(paramName);
                                linePosition = obj.transform.localPosition;
                                lineShow = true;  
                            }
                            yield return new WaitForSeconds(1f);//延迟时间
                                                       
                        }
                        
                    }
                    //将请求的参数保存为txt
                    //SaveTextToFile(response);
                }


            }
        }

        /// <summary>
        /// 请求的Txt文件存储地址
        /// </summary>
        /// <param name="txt"></param>
        //private void SaveTextToFile(string txt)
        //{
        //    string filePath = Path.Combine(Application.persistentDataPath, "response.txt");
        //    File.WriteAllText(filePath, txt);

        //     Log.Debug("txt文本保存地址: " + filePath);
        //}

        /// <summary>
        /// 查询轨迹存储
        /// </summary>
        //public void SaveFile()
        //{
        //    string filePath = Path.Combine(Application.persistentDataPath, "人员轨迹.txt");
        //    File.WriteAllText(filePath, scrollRect.content.GetComponent<Text>().text);
        //}
    }

}
