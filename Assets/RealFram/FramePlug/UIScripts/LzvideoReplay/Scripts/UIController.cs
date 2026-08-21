using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using UnityEngine;
using UnityEngine.UI;

public class UIController : SingletonManager<UIController>
{
  

    // private Button _minBtn;
    public Button _maxBtn;
    // private Button _exitBtn;
    public bool isMax = false;

    public void MinWindow()
    {

        ScreenResolution.Ins.SetMinWindows();
    }
    public void MaxWindow()
    {

        if (isMax)
        {

            ScreenResolution.Ins.ChangeScreenSizeHide(1280, 720, 960, 540);
            
            _maxBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/Fullscreen");
        }
        else
        {
            ScreenResolution.Ins.ChangeScreenSizeHide(1920, 1040, 960, 540);
            //ScreenResolution.Ins.ChangeScreenSizeHide(1920, 1080, 960, 540);
            _maxBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/Window");
        }
        isMax = !isMax;

    }
    public void ExitWindow()
    {
        Application.Quit();
    }
 
}
