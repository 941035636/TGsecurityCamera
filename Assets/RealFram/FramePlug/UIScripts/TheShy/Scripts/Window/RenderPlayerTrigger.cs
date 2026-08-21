using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using zFramework.Media;
using TMPro;
public class RenderPlayerTrigger : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (transform.Find("VideoRender").GetComponent<VideoRenderer>().IsRendering)
        {
            transform.Find("MsgBg").gameObject.SetActive(true);
            transform.Find("MsgBg/MsgTxt").GetComponent<TextMeshProUGUI>().text = transform.Find("VideoRender").GetComponent<VideoRenderer>().lumaWidth + "  *  " + transform.Find("VideoRender").GetComponent<VideoRenderer>().lumaHeight + "，25fps";
        }
       
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (transform.Find("VideoRender").GetComponent<VideoRenderer>().IsRendering)
        {
            transform.Find("MsgBg").gameObject.SetActive(false);

        }
    }
}
