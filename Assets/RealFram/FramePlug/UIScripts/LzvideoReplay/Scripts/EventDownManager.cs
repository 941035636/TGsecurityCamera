using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using RenderHeads.Media.AVProVideo;


public class EventDownManager : SingletonManager<EventDownManager>
{

    public Image EventImg;
    private TMP_Text DetailText;
    // Start is called before the first frame update
    void Start()
    {
      
     
        DetailText = transform.Find("Detailbg/DetailText").GetComponent<TextMeshProUGUI>();
        //PlayEventVideo(Path);
    }

    /// <summary>
    /// 从封面图文件夹里加载封面图
    /// </summary>
    public void GetCoverImg(string fileName)
    {
        Texture2D texture2D_1 = new Texture2D(2560, 1440);
        FileStream fs = File.Open(@"c:\faceevent\" + fileName + ".jpg", FileMode.Open, FileAccess.Read);
        byte[] bytes = new byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);
        texture2D_1.LoadImage(bytes);
        Log.Debug(texture2D_1);
        EventImg.sprite = Sprite.Create(texture2D_1, new Rect(0, 0, texture2D_1.width, texture2D_1.height), new Vector2(0.5f, 0.5f));
        fs.Close();
    }
    public void GetDoorCoverImg(string fileName)
    {
        Texture2D texture2D_1 = new Texture2D(2560, 1440);
        FileStream fs = File.Open(@"c:\doorevent\" + fileName + ".jpg", FileMode.Open, FileAccess.Read);
        byte[] bytes = new byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);
        texture2D_1.LoadImage(bytes);
        Log.Debug(fileName+" "+texture2D_1);
        EventImg.sprite = Sprite.Create(texture2D_1, new Rect(0, 0, texture2D_1.width, texture2D_1.height), new Vector2(0.5f, 0.5f));
        fs.Close();
    }
}
