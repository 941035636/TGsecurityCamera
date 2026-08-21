using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuTopUIController : MonoBehaviour
{
    private Button _menuTopBtn;
    private Button _previewTopLabel;
    private Button _replayTopLabel;
    private GameObject _content;
    // Start is called before the first frame update
    void Start()
    {
        _menuTopBtn = transform.Find("Scroll View/Viewport/MenuTopBtn").GetComponent<Button>();
        _previewTopLabel = transform.Find("Scroll View/Viewport/Content/PreviewTopLabel").GetComponent<Button>();
        _replayTopLabel = transform.Find("Scroll View/Viewport/Content/ReplayTopLabel").GetComponent<Button>();
        _content = transform.Find("Scroll View/Viewport/Content").gameObject;

        _menuTopBtn.onClick.AddListener(ClickMenuTopBtn);
        _previewTopLabel.onClick.AddListener(ClickPreviewTopLabel);
        _replayTopLabel.onClick.AddListener(ClickReplayTopLabel);
    }

    public void ClickPreviewBtn()
    {
        ClickPreviewTopLabel();

        _previewTopLabel.gameObject.SetActive(true);

        _previewTopLabel.gameObject.transform.SetAsFirstSibling();
    }
    public void ClickReplayBtn()
    {
        ClickReplayTopLabel();

        _replayTopLabel.gameObject.SetActive(true);
        _replayTopLabel.gameObject.transform.SetAsFirstSibling();
    }
    public void ClickMenuTopBtn()
    {
        ResetBtnImg();
        _menuTopBtn.transform.GetComponent<Image>().sprite = _menuTopBtn.transform.GetComponent<BtnImage>().Click;

        //UIController.Ins.ClosePageUI();
        //UIController.Ins.MainMenuPage.SetActive(true);
    }
    public void ClickPreviewTopLabel()
    {
        ResetBtnImg();
        _previewTopLabel.transform.GetComponent<Image>().sprite = _previewTopLabel.transform.GetComponent<BtnImage>().Click;

        //UIController.Ins.ClosePageUI();
        //UIController.Ins.PreviewPage.SetActive(true);
        _previewTopLabel.gameObject.transform.GetChild(0).gameObject.SetActive(true);
    }
    public void ClickReplayTopLabel()
    {
        ResetBtnImg();
        _replayTopLabel.transform.GetComponent<Image>().sprite = _replayTopLabel.transform.GetComponent<BtnImage>().Click;
        //UIController.Ins.ClosePageUI();
        //UIController.Ins.ReplayPage.SetActive(true);
        _replayTopLabel.gameObject.transform.GetChild(0).gameObject.SetActive(true);
    }
    public void ResetBtnImg()
    {
         Log.Debug(_content.transform.childCount);
        _menuTopBtn.transform.GetComponent<Image>().sprite = _menuTopBtn.transform.GetComponent<BtnImage>().Default;

        for (int i = 0; i < _content.transform.childCount; i++)
        {
            _content.transform.GetChild(i).GetComponent<Image>().sprite = _content.transform.GetChild(i).GetComponent<BtnImage>().Default;
            _content.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
        }
    }
    public void CloseLabel()
    {
        ClickMenuTopBtn();
    }
}
