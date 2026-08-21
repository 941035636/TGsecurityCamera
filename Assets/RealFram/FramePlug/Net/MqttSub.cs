using System;
using System.Collections;
using System.Collections.Generic;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;
using zFramework.Media;
using System.Threading;


public class mqttSub :Singleton<mqttSub>
{

     private string subscribe_chanel;
    public Image showImg;
    private MqttClient client;
    private int isConnecting;
    private GameStart gameStart;
    private readonly object reconnectLock = new object();
    private Timer reconnectTimer;
    private int reconnectAttempt;
    private volatile bool manualDisconnect;
    private string userName;
    private string password;
    private string host;
    private int brokerPort;

    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="username"></param>
    /// <param name="pwd"></param>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public void MqttConnect(string username,string pwd,string ip,int port)
    {
        userName = username;
        password = pwd;
        host = ip;
        brokerPort = port;
        manualDisconnect = false;
        if (client != null && client.IsConnected)
        {
            return;
        }
        BeginConnect();
    }

    private void BeginConnect()
    {
        if (Interlocked.Exchange(ref isConnecting, 1) == 1)
        {
            return;
        }
        gameStart = GameStart.Instance;
        ThreadPool.QueueUserWorkItem(_ => Connect());
    }

    private void Connect()
    {
        MqttClient newClient = null;
        try
        {
        string clientId = Guid.NewGuid().ToString();
        newClient = new MqttClient(host, brokerPort, false, null);
        newClient.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
        newClient.MqttMsgDisconnected += Client_MqttMsgDisconnected;
        newClient.Connect(clientId, userName, password);
        client = newClient;

      
        //MqttSubscribe("topic/update");//订阅人员信息更新主题
        //MqttSubscribe("topic/blackList");//订阅黑名单下发// 迁移到服务中
        newClient.Subscribe(new string[] { "topic/notice" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        reconnectAttempt = 0;
        CancelReconnectTimer();
        gameStart.RunOnMainThread(() => Log.Debug("Mqtt连接成功"));
        }
        catch (Exception exception)
        {
            if (newClient != null)
            {
                newClient.MqttMsgPublishReceived -= Client_MqttMsgPublishReceived;
                newClient.MqttMsgDisconnected -= Client_MqttMsgDisconnected;
                try
                {
                    if (newClient.IsConnected)
                    {
                        newClient.Disconnect();
                    }
                }
                catch
                {
                    // The original connection error is more useful; reconnect will create a fresh client.
                }
            }
            if (ReferenceEquals(client, newClient))
            {
                client = null;
            }
            gameStart.RunOnMainThread(() =>
            {
                Log.Error("MQTT连接失败: " + exception.Message);
            });
            ScheduleReconnect();
        }
        finally
        {
            Interlocked.Exchange(ref isConnecting, 0);
        }
    }

    //断开链接
    public void MqttDisConnect() 
    {
        manualDisconnect = true;
        CancelReconnectTimer();
        MqttClient current = client;
        client = null;
        if (current != null)
        {
            current.MqttMsgPublishReceived -= Client_MqttMsgPublishReceived;
            current.MqttMsgDisconnected -= Client_MqttMsgDisconnected;
            try
            {
                if (current.IsConnected)
                {
                    current.Disconnect();
                }
            }
            catch (Exception exception)
            {
                Log.Error("MQTT断开失败: " + exception.Message);
            }
        }
    }

    private void Client_MqttMsgDisconnected(object sender, EventArgs e)
    {
        if (!manualDisconnect && ReferenceEquals(sender, client))
        {
            ScheduleReconnect();
        }
    }

    private void ScheduleReconnect()
    {
        if (manualDisconnect)
        {
            return;
        }
        int minSeconds = Math.Max(1, AppRuntimeConfig.Settings.mqttReconnectMinSeconds);
        int maxSeconds = Math.Max(minSeconds, AppRuntimeConfig.Settings.mqttReconnectMaxSeconds);
        int exponent = Math.Min(reconnectAttempt++, 10);
        int delaySeconds = Math.Min(maxSeconds, minSeconds * (1 << exponent));
        lock (reconnectLock)
        {
            if (reconnectTimer != null)
            {
                return;
            }
            reconnectTimer = new Timer(_ =>
            {
                lock (reconnectLock)
                {
                    reconnectTimer.Dispose();
                    reconnectTimer = null;
                }
                BeginConnect();
            }, null, delaySeconds * 1000, Timeout.Infinite);
        }
    }

    private void CancelReconnectTimer()
    {
        lock (reconnectLock)
        {
            if (reconnectTimer != null)
            {
                reconnectTimer.Dispose();
                reconnectTimer = null;
            }
        }
    }
    /// <summary>
    /// 订阅接口
    /// </summary>
    public void MqttSubscribe(string subscribe_chanel) 
    {
        //订阅按钮监听事件
        MqttClient current = client;
        if (current != null && current.IsConnected && subscribe_chanel != "")
        {
            current.Subscribe(new string[] { subscribe_chanel }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

    }

    /// <summary>
    /// 回调
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Client_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
    {
        string message = System.Text.Encoding.UTF8.GetString(e.Message);
        string topic = e.Topic;
         //Log.Debug("Mqqt接收到消息是:" +topic+":"+message);
       
        string receiveMessage = topic + ":" + message;
        // { "type":1, "idRang":"10-110", "ids":{ [1,3,7]} }
        //type:0 新增,//
        //EventCenter.BroadCast<string>(Eventdefine.httpsend,message);
       gameStart.MessageQueue.Enqueue(receiveMessage);




    }



}
