using System;
using System.Text;
using System.Threading;
using HslCommunication.MQTT;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MQTTReceiver : MonoBehaviour
{
    private readonly object messageLock = new object();
    private volatile MqttClient mqttClient;
    private string pendingPayload;
    private volatile string pendingStatus;
    private volatile bool destroyed;

    public Text timeText;
    public string TimeText;

    private void Start()
    {
        if (!AppRuntimeConfig.Settings.enableLegacyMqttReceiver)
        {
            enabled = false;
            return;
        }

        if (timeText == null)
        {
            GameObject textObject = GameObject.Find("Canvas/Text");
            if (textObject != null)
            {
                timeText = textObject.GetComponent<Text>();
            }
        }

        // ConnectServer can wait on network I/O. Never execute it on Unity's render thread.
        string clientId = "SecurityCameraTG-" + Guid.NewGuid().ToString("N");
        ThreadPool.QueueUserWorkItem(_ => Connect(clientId));
    }

    private void Connect(string clientId)
    {
        try
        {
            MqttClient client = new MqttClient(new MqttConnectionOptions
            {
                ClientId = clientId,
                IpAddress = AppRuntimeConfig.Settings.mqttHost
            });

            HslCommunication.OperateResult connect = client.ConnectServer();
            if (!connect.IsSuccess)
            {
                SetStatus("MQTT连接失败");
                return;
            }

            HslCommunication.OperateResult subscribe = client.SubscribeMessage("test");
            if (!subscribe.IsSuccess)
            {
                SetStatus("MQTT订阅失败");
                return;
            }

            if (destroyed) return;
            mqttClient = client;
            client.OnMqttMessageReceived += OnMessageReceived;
            SetStatus("MQTT订阅成功");
        }
        catch (Exception exception)
        {
            SetStatus("MQTT连接异常: " + exception.Message);
        }
    }

    private void OnMessageReceived(MqttClient client, string topic, byte[] payload)
    {
        if (destroyed || payload == null) return;
        string message = Encoding.UTF8.GetString(payload);
        lock (messageLock)
        {
            pendingPayload = message;
        }
    }

    private void Update()
    {
        string status = pendingStatus;
        if (!string.IsNullOrEmpty(status))
        {
            pendingStatus = null;
            Debug.Log(status);
        }

        string json = null;
        lock (messageLock)
        {
            if (!string.IsNullOrEmpty(pendingPayload))
            {
                json = pendingPayload;
                pendingPayload = null;
            }
        }

        if (string.IsNullOrEmpty(json)) return;
        TimeText = json;

        try
        {
            JObject obj = JObject.Parse(json);
            JToken speed = obj["转速1"];
            if (timeText != null && speed != null)
            {
                timeText.text = speed.ToString();
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("MQTT消息格式错误: " + exception.Message);
        }
    }

    private void SetStatus(string status)
    {
        if (!destroyed) pendingStatus = status;
    }

    private void OnDestroy()
    {
        destroyed = true;
        if (mqttClient != null)
        {
            mqttClient.OnMqttMessageReceived -= OnMessageReceived;
        }
    }
}
