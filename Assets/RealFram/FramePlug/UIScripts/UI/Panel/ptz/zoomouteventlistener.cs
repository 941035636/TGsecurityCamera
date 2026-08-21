
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class zoomouteventlistener : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    bool isdown = false;
    public void OnPointerDown(PointerEventData eventData)
    {
        isdown = true;
         Log.Debug("按钮按下响应");
        EventCenter.BroadCast(Eventdefine.ptzzoomoutdown, MainPreviewUi.ChoiceIndex);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
         Log.Debug("按钮抬起响应");
        isdown = false;
        EventCenter.BroadCast(Eventdefine.ptzzoomoutup, MainPreviewUi.ChoiceIndex);
    }


    // Update is called once per frame
    void Update()
    {
        //if (isdown)
        //{
        //     Log.Debug("选择了："+MainPreviewUi.ChoiceIndex+"按钮按下了");
        //} 
    }
}
