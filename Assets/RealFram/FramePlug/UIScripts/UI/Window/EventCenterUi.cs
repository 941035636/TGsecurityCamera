using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using ZTools;

public class EventCenterUi : Window
{

    public EventCenterPanel _EventCenterPanel;
    public override void Awake(params object[] paralist)
    {
        _EventCenterPanel = GameObject.GetComponent<EventCenterPanel>();

        AddToggleClickListener(_EventCenterPanel.CameraEventTog, cameraEventTogListener);
        AddToggleClickListener(_EventCenterPanel.DoorEventTog, DoorEventTogListener);
        //_EventCenterPanel.CameraEventTog.onValueChanged.Invoke(true);
        _EventCenterPanel.CameraEventTog.isOn = true;
    }

    void cameraEventTogListener(bool ison)
    {
        if (LoginManager.Ins.IsCameraEvent)
        {
            if (ison) 
            {
                CameraEventJsonParse.CameracardNum = "";
                CameraEventJsonParse.Cameraeventype = "";
                CameraEventJsonParse.CameraName = "";
                CameraEventJsonParse.Camerausername = "";
                //_EventCenterPanel._CameraArenameDrop.value = 0;
                _EventCenterPanel._CameratypeDrop.value = 0;
                _EventCenterPanel.CameraPageLoading.GetComponent<PageLoadingCameraEvent>().Init();//监控事件分页
            }
               
            _EventCenterPanel.CameraRightBg.gameObject.SetActive(ison);
        }
        else 
        {
           GameStart.Instance.ShowTip("当前用户没有监控事件权限！");
        }
     


    }
    void DoorEventTogListener(bool ison)
    {
        if (LoginManager.Ins.IsDoorEvent)
        {
            if (ison) 
            {
                DoorEventJsonParse.DoorcardNum = "";
                _EventCenterPanel._DoornameInput.text = "";
                DoorEventJsonParse.Dooreventype = "";
                DoorEventJsonParse.DoorName = "";
                _EventCenterPanel._DoorCardnumInput.text = "";
                DoorEventJsonParse.Doorusername = "";
                DatePickerGroupUser.EventCenterStartDate = "";
                DatePickerGroupUser.EventCenterEndDate = "";
                ZCalendarEvent.EventSearchStartTime = "";
                ZCalendarEvent.EventSearchEndTime = "";

                _EventCenterPanel._DoorArenameDrop.value = 0;
                _EventCenterPanel._DoortypeDrop.value = 0;

                _EventCenterPanel.DoorPageLoading.GetComponent<PageLoadingDoorEvent>().Init();//门禁事件分页
                UnityTimer.Timer.Register(1, () => {
                    EventCenter.BroadCast(Eventdefine.ChoiceTimeShow, "2023-01-01", DateTime.Today.ToString("yyyy-MM-dd"));

                });
               
            }
              
            _EventCenterPanel.DoorRightBg.gameObject.SetActive(ison);
        }
        else 
        {
            GameStart.Instance.ShowTip("当前用户没有门禁事件权限！");
        }
         
    }
}
