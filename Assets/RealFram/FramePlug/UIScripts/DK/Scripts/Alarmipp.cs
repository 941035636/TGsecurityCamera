using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static zFramework.Media.CHCNetSDK;
namespace zFramework.Media
{
    public class Alarmipp : MonoBehaviour
    {
                private Int32 m_lUserID = -1;
        private Int32[] m_lAlarmHandle = new Int32[200];
        private Int32 iListenHandle = -1;
        private int iDeviceNumber = 0; //添加设备个数
        private int iFileNumber = 0; //保存的文件个数
        private uint iLastErr = 0;
        private string strErr;


        public CHCNetSDK.LOGINRESULTCALLBACK LoginCallBack = null;
        private CHCNetSDK.EXCEPYIONCALLBACK m_fExceptionCB = null;
        private CHCNetSDK.MSGCallBack_V31 m_falarmData_V31 = null;//报警回调函数
        private CHCNetSDK.MSGCallBack m_falarmData = null;
        private CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo;



        private bool m_bInitSDK = false;

        private CHCNetSDK.NET_DVR_ACS_EVENT_INFO_EXTEND struAcsEventInfoExtend;





        string ip = "192.168.26.175";
        int port = 8000;
        string userName = "admin";
        string password = "";

        public GameObject prefab;//

        public RectTransform contentTransform;//实例化对象的父类

        private Transform temp;

       
        byte[] faceby;
        
        int index = 0;
        private string data;
        void Start()
        {

            iLastErr = NET_DVR_GetLastError();
            InitSDKs();

        }

        // Update is called once per frame
        void Update()
        {
            //登录
            if (Input.GetKeyDown(KeyCode.A))
            {
                Login();


            }
            //布防
            if (Input.GetKeyDown(KeyCode.S))
            {
                SetAlarm();
            }

            //撤防
            if (Input.GetKeyDown(KeyCode.X))
            {
                CloseAlarm();
            }
            //启动监听
            if (Input.GetKeyDown(KeyCode.D))
            {
                StartListen();
            }
            //注销
            if (Input.GetKeyDown(KeyCode.E))
            {
                LogOut();

            }



        }


        private void InitSDKs()
        {
            //初始化  开始时调用，一个程序只能调用一次
            m_bInitSDK = CHCNetSDK.NET_DVR_Init();

            if (m_bInitSDK == false)
            {
                 Log.Debug("初始化失败");
                return;
            }
            else
            {

                byte[] strIP = new byte[16 * 16];
                uint dwValidNum = 0;
                Boolean bEnableBind = false;
                //获取本地PC网卡IP信息
                if (CHCNetSDK.NET_DVR_GetLocalIP(strIP, ref dwValidNum, ref bEnableBind))
                {
                    if (dwValidNum > 0)
                    {
                        //取第一张网卡的IP地址为默认监听端口
                        print(System.Text.Encoding.UTF8.GetString(strIP, 0, 16));
                    }
                }

                ////保存SDK日志 To save the SDK log
                CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog002\\", true);
                 Log.Debug("初始化成功" + m_bInitSDK);

                Login();

                //设置透传报警信息类型
                CHCNetSDK.NET_DVR_LOCAL_GENERAL_CFG struLocalCfg = new CHCNetSDK.NET_DVR_LOCAL_GENERAL_CFG();
                struLocalCfg.byAlarmJsonPictureSeparate = 1;//控制JSON透传报警数据和图片是否分离，0-不分离(COMM_VCA_ALARM返回)，1-分离（分离后走COMM_ISAPI_ALARM回调返回)


                Int32 nSize = Marshal.SizeOf(struLocalCfg);
                IntPtr ptrLocalCfg = Marshal.AllocHGlobal(nSize);
                Marshal.StructureToPtr(struLocalCfg, ptrLocalCfg, false);

                //设置SDK本地参数
                if (!CHCNetSDK.NET_DVR_SetSDKLocalCfg(17, ptrLocalCfg))  //NET_DVR_LOCAL_CFG_TYPE_GENERAL
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    strErr = "NET_DVR_SetSDKLocalCfg failed, error code= " + iLastErr;
                    print(strErr);
                }


                Marshal.FreeHGlobal(ptrLocalCfg);

                for (int i = 0; i < 200; i++)
                {
                    m_lAlarmHandle[i] = -1;
                }


                //设置异常消息回调函数
                //if (m_fExceptionCB == null)
                //{
                //    m_fExceptionCB = new CHCNetSDK.EXCEPYIONCALLBACK(cbExceptionCB);
                //}
                //CHCNetSDK.NET_DVR_SetExceptionCallBack_V30(0, IntPtr.Zero, m_fExceptionCB, IntPtr.Zero);

                //设置报警回调函数
                if (m_falarmData_V31 == null)
                {
                    m_falarmData_V31 = new CHCNetSDK.MSGCallBack_V31(MsgCallback_V31);
                    print("设置报警回调函数");
                }
                CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V31(m_falarmData_V31, IntPtr.Zero);// 注册回调函数，接收设备报警消息等。TRUE表示成功，FALSE表示失败
                print(CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V31(m_falarmData_V31, IntPtr.Zero));
                CHCNetSDK.NET_DVR_SETUPALARM_PARAM struAlarmParam = new CHCNetSDK.NET_DVR_SETUPALARM_PARAM();
                struAlarmParam.dwSize = (uint)Marshal.SizeOf(struAlarmParam);
                struAlarmParam.byLevel = 1; //0- 一级布防,1- 二级布防
                struAlarmParam.byAlarmInfoType = 1;//智能交通设备有效，新报警信息类型
                struAlarmParam.byFaceAlarmDetection = 1;//1-人脸侦测
                struAlarmParam.byDeployType = 1;//实时布防


                /*每台设备分别登录，分别调用NET_DVR_SetupAlarmChan_V41进行布防,
                 * 布防即建立设备跟客户端之间报警上传的连接通道，这样设备发生报警之后通过该连接上传报警信息，
                 * SDK在报警回调函数中接收和处理报警信息数据即可。
                 */
                for (int i = 0; i < iDeviceNumber; i++)
                {
                    m_lAlarmHandle[m_lUserID] = CHCNetSDK.NET_DVR_SetupAlarmChan_V41(m_lUserID, ref struAlarmParam);
                    if (m_lAlarmHandle[m_lUserID] < 0)
                    {
                        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        strErr = "布防失败，错误号：" + iLastErr; //布防失败，输出错误号
                        print("哈哈哈" + strErr);
                    }
                    else
                    {
                        print("布防成功" + m_lAlarmHandle[m_lUserID]);

                    }

                    print("布防" + m_lAlarmHandle[m_lUserID]);
                }
                print(m_falarmData_V31);

                Log.Debug("数据" + data);
            }
        }


        public void Login()
        {

            DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
            //登录设备 Login the device
            m_lUserID = CHCNetSDK.NET_DVR_Login_V30(ip, port, userName, password, ref DeviceInfo);
            if (m_lUserID < 0)
            {

                strErr = " 登陆失败= " + CHCNetSDK.NET_DVR_GetLastError(); //登录失败，输出错误号 Failed to login and output the error code
                print(strErr);
            }
            else
            {
                //登录成功
                iDeviceNumber++;
                string str1 = "" + m_lUserID;
                 Log.Debug(str1);
                 Log.Debug("登录成功" + m_lUserID);
                byte[] strIP = new byte[16 * 16];
                uint dwValidNum = 0;
                Boolean bEnableBind = false;

                if (CHCNetSDK.NET_DVR_GetLocalIP(strIP, ref dwValidNum, ref bEnableBind))
                {
                    if (dwValidNum > 0)
                    {
                         Log.Debug(strIP);
                    }
                }

                m_falarmData_V31 = new CHCNetSDK.MSGCallBack_V31(MsgCallback_V31);//设置报警回调函数

                // SetAlarm();
                //listViewDevice.Items.Add(new ListViewItem(new string[] { str1, textBoxIP.Text, "未布防" }));//将已注册设备添加进列表
            }
        }
        public bool MsgCallback_V31(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            // //通过lCommand来判断接收到的报警信息类型，不同的lCommand对应不同的pAlarmInfo内容
            AlarmMessageHandle(lCommand, ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
            //print(lCommand + " : " + pAlarmer.sSerialNumber + " : " + pAlarmInfo + " : " + dwBufLen + " : " + pUser);

            return true;//回调函数需要有返回，表示正常接收到数据
        }
        public void MsgCallback(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {

        }
        public void AlarmMessageHandle(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            switch (lCommand)
            {

                case CHCNetSDK.COMM_ALARM_ACS://门禁主机报警上传
                                              //ProcessCommAlarm_AcsAlarm(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    CHCNetSDK.NET_DVR_ACS_ALARM_INFO struAcsAlarm = new CHCNetSDK.NET_DVR_ACS_ALARM_INFO();


                    uint dwSize = (uint)Marshal.SizeOf(struAcsAlarm);
                    struAcsAlarm = (CHCNetSDK.NET_DVR_ACS_ALARM_INFO)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_DVR_ACS_ALARM_INFO));

                    //报警设备IP地址
                    string strIP = System.Text.Encoding.UTF8.GetString(pAlarmer.sDeviceIP).TrimEnd('\0');
                    print(strIP);



                    //报警时间：年月日时分秒
                    string strTimeYear = (struAcsAlarm.struTime.dwYear).ToString();
                    string strTimeMonth = (struAcsAlarm.struTime.dwMonth).ToString("d2");
                    string strTimeDay = (struAcsAlarm.struTime.dwDay).ToString("d2");
                    string strTimeHour = (struAcsAlarm.struTime.dwHour).ToString("d2");
                    string strTimeMinute = (struAcsAlarm.struTime.dwMinute).ToString("d2");
                    string strTimeSecond = (struAcsAlarm.struTime.dwSecond).ToString("d2");
                    string strTime = strTimeYear + "-" + strTimeMonth + "-" + strTimeDay + " " + strTimeHour + ":" + strTimeMinute + ":" + strTimeSecond;


                    //门禁参数
                    // string stringAlarm = "门禁主机报警信息，dwMajor：0x" + Convert.ToString(struAcsAlarm.dwMajor, 16) + "，dwMinor：0x" +
                    //    Convert.ToString(struAcsAlarm.dwMinor, 16) + "，卡号：" + System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0')
                    //    + "，读卡器编号：" + struAcsAlarm.struAcsEventInfo.dwCardReaderNo + "，报警触发时间：" + strTime +
                    //    "，事件流水号：" + struAcsAlarm.struAcsEventInfo.dwSerialNo;
                    // print(stringAlarm);

                    //print("图片参数"+struAcsAlarm.dwPicDataLen + "  " + struAcsAlarm.pPicData);
                    string stringAlarm = "IP：" + strIP + "，卡号：" + System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0')
                   + "，身份证号:" + struAcsAlarm.struAcsEventInfo.dwEmployeeNo + "，报警触发时间：" + strTime;



                    //保存抓拍图片
                    if ((struAcsAlarm.dwPicDataLen != 0) && (struAcsAlarm.pPicData != IntPtr.Zero))
                    {
                        string str = "C:\\Picture\\[" + strIP + "]_lUerID_[" + pAlarmer.lUserID + "]_" + System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0') + " , " + strTimeSecond + " ,  " + ".jpg";
                        FileStream fs = new FileStream(str, FileMode.Create);
                        int iLen = (int)struAcsAlarm.dwPicDataLen;
                        byte[] by = new byte[iLen];
                        faceby = by;
                        Marshal.Copy(struAcsAlarm.pPicData, by, 0, iLen);
                        fs.Write(by, 0, iLen);
                        fs.Close();
                    }


                    /* 0x4b --人脸认证通过，0x4c--人脸认证失败 */
                    DoorAlarmData alarmData = new DoorAlarmData();
                    alarmData.equipIp = strIP;
                    if (Convert.ToString(struAcsAlarm.dwMinor, 16) == "4b")//人脸认证成功
                    {
                        //         print("图片参数"+struAcsAlarm.dwPicDataLen + "  " + struAcsAlarm.pPicData);
                        //     string stringAlarm = "IP：" + strIP +" 职工 "+ "，卡号：" + System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0')
                        //    + "，身份证号:" + struAcsAlarm.struAcsEventInfo.dwEmployeeNo +  "，报警触发时间：" + strTime ;

                        alarmData.eventType = "人脸认证通过";
                        Log.Debug("人脸认证通过" + alarmData.eventType);


                        LoomAlarm.QueueOnMainThread((param) =>
                        {
                            temp = Instantiate(prefab).transform;
                            temp.SetParent(contentTransform);
                            temp.GetChild(0).GetComponent<TextMeshProUGUI>().text = System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0');
                            temp.GetChild(1).GetComponent<TextMeshProUGUI>().text = "人脸认证通过";
                            temp.GetChild(1).GetComponent<TextMeshProUGUI>().color=Color.green;
                            temp.GetChild(2).GetComponent<TextMeshProUGUI>().text = strTime;
                            temp.GetChild(3).GetComponent<TextMeshProUGUI>().text = strIP;
                            temp.transform.localScale = new Vector3(1f, 1f, 1f);

                           
                            


                            // 将图片数据加载到Texture2D
                            Texture2D tex = new Texture2D(1, 1);
                            tex.LoadImage(faceby);
                            // 将Texture2D转换为Sprite并赋值给Image组件
                            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                            //faceImage.sprite = sprite;
                            temp.GetChild(6).GetComponent<Image>().sprite = sprite;

                           
                            Log.Debug("测试");

                        }, null);

                        //UpdateClientList(stringAlarm);
                    }

                    //认证失败
                    if (Convert.ToString(struAcsAlarm.dwMinor, 16) == "4c")
                    {
                        Log.Debug("人脸认证失败");

                        LoomAlarm.QueueOnMainThread((param) =>
                       {
                           temp = Instantiate(prefab).transform;
                           temp.SetParent(contentTransform);
                           temp.GetChild(0).GetComponent<TextMeshProUGUI>().text = System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0');
                           temp.GetChild(1).GetComponent<TextMeshProUGUI>().text = "人脸认证失败";
                            temp.GetChild(1).GetComponent<TextMeshProUGUI>().color=Color.red;
                           temp.GetChild(2).GetComponent<TextMeshProUGUI>().text = strTime;
                           temp.GetChild(3).GetComponent<TextMeshProUGUI>().text = strIP;
                           temp.transform.localScale = new Vector3(1f, 1f, 1f);
                           // 将图片数据加载到Texture2D
                           Texture2D tex = new Texture2D(1, 1);
                           tex.LoadImage(faceby);
                           // 将Texture2D转换为Sprite并赋值给Image组件
                           Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                           //faceImage.sprite = sprite;
                           temp.GetChild(6).GetComponent<Image>().sprite = sprite;
                            
                       }, null);

                        //UpdateClientList(stringAlarm);

                    }


                    alarmData.faceBase64 = "";
                    alarmData.idNum = struAcsAlarm.struAcsEventInfo.dwEmployeeNo.ToString();
                    alarmData.time = strTime;
                    alarmData.userName = System.Text.Encoding.UTF8.GetString(struAcsAlarm.sNetUser);
                     Log.Debug("验证姓名:" + alarmData.userName);
                     Log.Debug("验证ID:" + alarmData.idNum);
                    break;

                default:
                    {
                        //报警设备IP地址
                        //string strIP = System.Text.Encoding.UTF8.GetString(pAlarmer.sDeviceIP).TrimEnd('\0');
                        //print(strIP);

                        ////报警信息类型
                        //string stringAlarm = "报警上传，信息类型：0x" + Convert.ToString(lCommand, 16);

                        //}
                    }
                    break;

            }
        }
        /// <summary>
        /// 门禁主机报警上传
        /// </summary>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        public void ProcessCommAlarm_AcsAlarm(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_DVR_ACS_ALARM_INFO struAcsAlarm = new CHCNetSDK.NET_DVR_ACS_ALARM_INFO();


            uint dwSize = (uint)Marshal.SizeOf(struAcsAlarm);
            struAcsAlarm = (CHCNetSDK.NET_DVR_ACS_ALARM_INFO)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_DVR_ACS_ALARM_INFO));

            //报警设备IP地址
            string strIP = System.Text.Encoding.UTF8.GetString(pAlarmer.sDeviceIP).TrimEnd('\0');
            print(strIP);
            //保存抓拍图片
            if ((struAcsAlarm.dwPicDataLen != 0) && (struAcsAlarm.pPicData != IntPtr.Zero))
            {
                string str = "C:\\Picture\\[" + strIP + "]_lUerID_[" + pAlarmer.lUserID + "]_" + System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0') + ".jpg";


                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)struAcsAlarm.dwPicDataLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struAcsAlarm.pPicData, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();

            }


            //报警时间：年月日时分秒
            string strTimeYear = (struAcsAlarm.struTime.dwYear).ToString();
            string strTimeMonth = (struAcsAlarm.struTime.dwMonth).ToString("d2");
            string strTimeDay = (struAcsAlarm.struTime.dwDay).ToString("d2");
            string strTimeHour = (struAcsAlarm.struTime.dwHour).ToString("d2");
            string strTimeMinute = (struAcsAlarm.struTime.dwMinute).ToString("d2");
            string strTimeSecond = (struAcsAlarm.struTime.dwSecond).ToString("d2");
            string strTime = strTimeYear + "-" + strTimeMonth + "-" + strTimeDay + " " + strTimeHour + ":" + strTimeMinute + ":" + strTimeSecond;
            //门禁参数
            //string stringAlarm = "门禁主机报警信息，dwMajor：0x" + Convert.ToString(struAcsAlarm.dwMajor, 16) + "，dwMinor：0x" +
            //    Convert.ToString(struAcsAlarm.dwMinor, 16) + "，卡号：" + System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0')
            //    + "，读卡器编号：" + struAcsAlarm.struAcsEventInfo.dwCardReaderNo + "，报警触发时间：" + strTime +
            //    "，事件流水号：" + struAcsAlarm.struAcsEventInfo.dwSerialNo;
            //print(stringAlarm);

            //print("图片参数"+struAcsAlarm.dwPicDataLen + "  " + struAcsAlarm.pPicData);
            string stringAlarm = "IP：" + strIP + "的门禁主机报警信息，dwMajor：0x" + Convert.ToString(struAcsAlarm.dwMajor, 16) + "，dwMinor：0x" +
            Convert.ToString(struAcsAlarm.dwMinor, 16) + "，卡号：" + System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0')
           + "，身份证号:" + struAcsAlarm.struAcsEventInfo.dwEmployeeNo + "，读卡器编号：" + struAcsAlarm.struAcsEventInfo.dwCardReaderNo + "，报警触发时间：" + strTime +
            "，事件流水号：" + struAcsAlarm.struAcsEventInfo.dwSerialNo;
            print(stringAlarm);
            data = stringAlarm;
            /* 0x4b --人脸认证通过，0x4c--人脸认证失败 */
            DoorAlarmData alarmData = new DoorAlarmData();
            alarmData.equipIp = strIP;
            if (Convert.ToString(struAcsAlarm.dwMinor, 16) == "4b")
            {
                alarmData.eventType = "人脸认证通过";
                 Log.Debug("人脸认证通过" + alarmData.eventType);
            }

            if (Convert.ToString(struAcsAlarm.dwMinor, 16) == "4c")
            {
                alarmData.eventType = "人脸认证失败";
                 Log.Debug("人脸认证失败" + alarmData.eventType);
            }


            alarmData.faceBase64 = "";
            alarmData.idNum = struAcsAlarm.struAcsEventInfo.dwEmployeeNo.ToString();
            alarmData.time = strTime;
            alarmData.userName = System.Text.Encoding.UTF8.GetString(struAcsAlarm.sNetUser);
             Log.Debug("验证姓名:" + alarmData.userName);
             Log.Debug("验证ID:" + alarmData.idNum);


            //if (struAcsAlarm.byAcsEventInfoExtend == 1)
            //{
            //    CHCNetSDK.NET_DVR_ACS_EVENT_INFO_EXTEND struInfoExtend = new CHCNetSDK.NET_DVR_ACS_EVENT_INFO_EXTEND();
            //    uint dwSizeEx = (uint)Marshal.SizeOf(struInfoExtend);
            //    struInfoExtend = (CHCNetSDK.NET_DVR_ACS_EVENT_INFO_EXTEND)Marshal.PtrToStructure(struAcsAlarm.pAcsEventInfoExtend, typeof(CHCNetSDK.NET_DVR_ACS_EVENT_INFO_EXTEND));
            //    stringAlarm = stringAlarm + ", 工号:" + System.Text.Encoding.UTF8.GetString(struInfoExtend.byEmployeeNo).TrimEnd('\0') +
            //        ", 人员类型:" + struInfoExtend.byUserType;
            //    print(struInfoExtend); 
            //}

            //if (struAcsAlarm.byAcsEventInfoExtendV20 == 1)
            //{
            //CHCNetSDK.NET_DVR_ACS_EVENT_INFO_EXTEND_V20 struInfoExtendV20 = new CHCNetSDK.NET_DVR_ACS_EVENT_INFO_EXTEND_V20();
            //    uint dwSizeEx = (uint)Marshal.SizeOf(struInfoExtendV20);
            //    struInfoExtendV20 = (CHCNetSDK.NET_DVR_ACS_EVENT_INFO_EXTEND_V20)Marshal.PtrToStructure(struAcsAlarm.pAcsEventInfoExtendV20, typeof(CHCNetSDK.NET_DVR_ACS_EVENT_INFO_EXTEND_V20));
            //    stringAlarm = stringAlarm + ", 温度:" + struInfoExtendV20.fCurrTemperature + ", 是否异常温度:" + struInfoExtendV20.byIsAbnomalTemperature
            //        + ", 是否需要核验:" + struInfoExtendV20.byRemoteCheck;

            //    if (struInfoExtendV20.byRemoteCheck == 2)
            //    {
            //        //建议另外建线程或者使用消息队列进行处理和下发核验结果给设备，避免阻塞回调
            //        //ACS_remoteCheck(struAcsAlarm.struAcsEventInfo.dwSerialNo);
            //    }

            //保存热成像图片
            //if ((struInfoExtendV20.dwThermalDataLen != 0) && (struInfoExtendV20.pThermalData != IntPtr.Zero))
            //{
            //    string str = "c:\\picture\\Device_Acs_ThermalData_[" + strIP + "]_lUerID_[" + pAlarmer.lUserID + "]_" + iFileNumber + ".jpg";
            //    FileStream fs = new FileStream(str, FileMode.Create);
            //    int iLen = (int)struInfoExtendV20.dwThermalDataLen;
            //    byte[] by = new byte[iLen];
            //    Marshal.Copy(struInfoExtendV20.pThermalData, by, 0, iLen);
            //    fs.Write(by, 0, iLen);
            //    fs.Close();
            //    iFileNumber++;
            //}
            // }

            //if (InvokeRequired)
            //{
            //    object[] paras = new object[3];
            //    paras[0] = DateTime.Now.ToString(); //当前PC系统时间
            //    paras[1] = strIP;
            //    paras[2] = stringAlarm;
            //    ///listViewAlarmInfo.BeginInvoke(new UpdateListBoxCallback(UpdateClientList), paras);
            //}
            //else
            //{
            //    //创建该控件的主线程直接更新信息列表 
            //   // UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
            //}
        }
        /// <summary>
        /// 报警布防
        /// </summary>
        public void SetAlarm()
        {

            CHCNetSDK.NET_DVR_SETUPALARM_PARAM struAlarmParam = new CHCNetSDK.NET_DVR_SETUPALARM_PARAM();
            struAlarmParam.dwSize = (uint)Marshal.SizeOf(struAlarmParam);
            struAlarmParam.byLevel = 1; //0- 一级布防,1- 二级布防
            struAlarmParam.byAlarmInfoType = 1;//智能交通设备有效，新报警信息类型
            struAlarmParam.byFaceAlarmDetection = 1;//1-人脸侦测
            struAlarmParam.byDeployType = 1;//实时布防


            /*每台设备分别登录，分别调用NET_DVR_SetupAlarmChan_V41进行布防,
             * 布防即建立设备跟客户端之间报警上传的连接通道，这样设备发生报警之后通过该连接上传报警信息，
             * SDK在报警回调函数中接收和处理报警信息数据即可。
             */
            for (int i = 0; i < iDeviceNumber; i++)
            {
                m_lAlarmHandle[m_lUserID] = CHCNetSDK.NET_DVR_SetupAlarmChan_V41(m_lUserID, ref struAlarmParam);
                if (m_lAlarmHandle[m_lUserID] < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    strErr = "布防失败，错误号：" + iLastErr; //布防失败，输出错误号
                    print(strErr);
                }
                else
                {
                    print("布防成功" + m_lAlarmHandle[m_lUserID]);

                }

                print("布防" + m_lAlarmHandle[m_lUserID]);
            }

        }
        private void OnDestroy() {
            CloseAlarm();
            LogOut();
        }
        /// <summary>
        /// 撤销布防
        /// </summary>
        public void CloseAlarm()
        {
            print(m_lAlarmHandle[m_lUserID]);

            for (int i = 0; i < iDeviceNumber; i++)
            {
                // m_lUserID = Int32.Parse(listViewDevice.Items[i].SubItems[0].Text);
                if (m_lAlarmHandle[m_lUserID] >= 0)
                {
                    if (!CHCNetSDK.NET_DVR_CloseAlarmChan_V30(m_lAlarmHandle[m_lUserID]))
                    {
                        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        strErr = "撤防失败，错误号：" + iLastErr; //撤防失败，输出错误号
                        //listViewDevice.Items[i].SubItems[2].Text = strErr;
                    }
                    else
                    {
                        //listViewDevice.Items[i].SubItems[2].Text = "未布防";
                        m_lAlarmHandle[i] = -1;
                        print("撤防");
                    }
                }
                else
                {
                    print("未布防");
                    //listViewDevice.Items[i].SubItems[2].Text = "未布防";
                }
            }
            //btn_SetAlarm.Enabled = true;

        }
        /// <summary>
        /// 启动监听
        /// </summary>
        public void StartListen()
        {
            //string sLocalIP = textBoxListenIP.Text;//本地PCip
            //ushort wLocalPort = ushort.Parse(textBoxListenPort.Text);//本地pc端口
            string sLocalIP = "192.168.199.110";
            ushort wLocalPort = 8000;
            if (m_falarmData == null)
            {
                m_falarmData = new CHCNetSDK.MSGCallBack(MsgCallback);
            }

            iListenHandle = CHCNetSDK.NET_DVR_StartListen_V30(sLocalIP, wLocalPort, m_falarmData, IntPtr.Zero);
            if (iListenHandle < 0)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                strErr = "启动监听失败，错误号：" + iLastErr; //撤防失败，输出错误号
                print(strErr);
            }
            else
            {
                print("成功启动监听！");

            }
        }


        //public void cbExceptionCB(uint dwType, int lUserID, int lHandle, IntPtr pUser)
        //{
        //    //异常消息信息类型
        //    //string stringAlarm = "异常消息回调，信息类型：0x" + Convert.ToString(dwType, 16) + ", lUserID:" + lUserID + ", lHandle:" + lHandle;

        //    //if (InvokeRequired)
        //    //{
        //    //    object[] paras = new object[3];
        //    //    paras[0] = DateTime.Now.ToString(); //当前PC系统时间
        //    //    paras[1] = lUserID;
        //    //    paras[2] = stringAlarm;
        //    //    listViewAlarmInfo.BeginInvoke(new UpdateListBoxCallbackException(UpdateClientListException), paras);
        //    //}
        //    //else
        //    //{
        //    //    //创建该控件的主线程直接更新信息列表 
        //    //    UpdateClientListException(DateTime.Now.ToString(), lUserID, stringAlarm);
        //    //}
        //}
        public void UpdateClientList(string strAlarmTime)
        {
            data = strAlarmTime;

            Log.Debug("报警数据" + data);
        }
        /// <summary>
        /// 注销登录,释放SDK资源
        /// </summary>
        public void LogOut()
        {
            CHCNetSDK.NET_DVR_Logout(m_lUserID);
            CHCNetSDK.NET_DVR_Cleanup();
            print("注销");
        }
    }
}

