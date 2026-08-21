using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverUIShowName : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject infoImage;
    public string Name;//位置名称
    public string Trajectory;//顺序数值
    [SerializeField] int sortingOrder = 10; // 设置显示顺序的数值
    public static HoverUIShowName instance;
    //偏移值
    [SerializeField] float offsetX = 0;
    [SerializeField] float offsety = 0;
    private void Start()
    {
        Canvas canvas = infoImage.GetComponent<Canvas>();
        // 检查是否已存在Canvas组件
        if (canvas == null)
        {
            // 如果不存在，则动态添加一个Canvas组件
            canvas = infoImage.AddComponent<Canvas>();
        }
        // 设置Canvas的渲染顺序属性
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
    }
    /// <summary>
    /// 控制UI的位置
    /// </summary>
    void Update()
    {
        if (infoImage.activeSelf)
        {
            // 将infoImage的位置设置为鼠标位置加上偏移值
            infoImage.transform.localPosition = Input.mousePosition + new Vector3(offsetX, offsety, 0f);
           
        }
    }
    // 当鼠标指针进入对象时调用的方法
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (infoImage != null)
        {
            // 激活infoImage并设置显示的文本为Name
            infoImage.SetActive(true);
            infoImage.GetComponentInChildren<Text>().text = Name;
            
            infoImage.transform.GetChild(1).GetComponent<Text>().text = Trajectory;
        }
    }
    // 当鼠标指针离开对象时调用的方法
    public void OnPointerExit(PointerEventData eventData)
    {
        if (infoImage != null)
        {
            infoImage.SetActive(false);
        }
    }

    public void IDEmpty()
    {
        infoImage.transform.GetChild(1).GetComponent<Text>().text = null;
    }
}
