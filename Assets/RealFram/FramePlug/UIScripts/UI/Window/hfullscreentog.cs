using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class hfullscreentog : MonoBehaviour
{
    public void changsprite01() 
    {
        transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/视频预览/收-1.png");
    }
}
