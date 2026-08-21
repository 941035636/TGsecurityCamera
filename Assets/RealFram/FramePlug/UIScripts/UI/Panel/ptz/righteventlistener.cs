
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class righteventlistener : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    bool isdown = false;
    public void OnPointerDown(PointerEventData eventData)
    {
        isdown = true;
         Log.Debug("按钮按下响应:    choiceIndex"+MainPreviewUi.ChoiceIndex);

        EventCenter.BroadCast(Eventdefine.ptzrightdown, MainPreviewUi.ChoiceIndex);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
         Log.Debug("按钮抬起响应:    choiceIndex" + MainPreviewUi.ChoiceIndex);
        isdown = false;
        EventCenter.BroadCast(Eventdefine.ptzrightup, MainPreviewUi.ChoiceIndex);
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
