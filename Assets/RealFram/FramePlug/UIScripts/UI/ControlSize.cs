 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlSize : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SetContentSizeActive();
    }

    public void SetButtonActive(GameObject button)//控制按钮消失
    {
        if (button.active == false)
        {
            button.SetActive(true);
        }
        else
        {
            button.SetActive(false);
        }
        SetContentSizeActive();
        //LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
    }
    void SetContentSizeActive()
    {
        StartCoroutine(HelpSet());//及时触发
    }
    IEnumerator HelpSet()
    {
        this.GetComponent<ContentSizeFitter>().enabled = false;
        yield return null;
        this.GetComponent<ContentSizeFitter>().enabled = true;
    }
}