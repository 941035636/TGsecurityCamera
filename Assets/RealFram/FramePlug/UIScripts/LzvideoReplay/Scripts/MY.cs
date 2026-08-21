using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class MY : MonoBehaviour
{
    void Awake()
    {
        //ScreenResolution.Ins.ChangeScreenSizeHide(1280, 720, 960, 540);

    }
    // Start is called before the first frame update
    void Start()
    {
        //ScreenResolution.Ins.ChangeScreenSizeShow(1280, 720, 960, 540);

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {

        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ReplayUIController.Ins.isFull)
            {
                ScreenResolution.Ins.ChangeScreenSizeHide(1280, 720, 960, 540);
                switch (ReplayUIController.Ins.ChooseViewNum)
                {
                    case 1:
                        ReplayUIController.Ins.Segmentation(1, 1);
                        break;
                    case 4:
                        ReplayUIController.Ins.Segmentation(4, 4);
                        break;
                    case 6:
                        ReplayUIController.Ins.Segmentation(10, 6);
                        break;
                    case 8:
                        ReplayUIController.Ins.Segmentation(17, 8);
                        break;
                    case 9:
                        ReplayUIController.Ins.Segmentation(9, 9);
                        break;
                    case 13:
                        ReplayUIController.Ins.Segmentation(17, 13);
                        break;
                    case 16:
                        ReplayUIController.Ins.Segmentation(16, 16);
                        break;
                }
                //UIController.Ins.ReplayPage.SetActive(true);
            }

        }
    }
    // Update is called once per frame

    public void OpenReplayPage()
    {
        transform.SetAsLastSibling();
    }

}
