using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    private IPEndPoint endPoint;

    void Start()
    {
        udpClient = new UdpClient(5252); // 创建一个UDP客户端并绑定到本地端口1234
        endPoint = new IPEndPoint(IPAddress.Any, 0); // 设置接收任何IP地址，任何端口号的数据
        StartCoroutine(ReceiveData()); // 开启一个协程来接收数据
    }

    IEnumerator ReceiveData()
    {
        while (true)
        {
            byte[] receiveBytes = udpClient.Receive(ref endPoint); // 接收UDP数据

            string receiveString = Encoding.ASCII.GetString(receiveBytes); // 将字节转换为字符串

             Log.Debug("Received: " + receiveString);

            yield return null; // 等待下一帧继续接收
        }
    }

    void OnDisable()
    {
        //udpClient.Close(); // 关闭UDP客户端
    }
}
