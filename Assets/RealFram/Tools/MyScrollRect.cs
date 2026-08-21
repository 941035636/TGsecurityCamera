using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;
using DG.Tweening;
public class MyScrollRect :MonoBehaviour
{
    Vector3 previewPosition, newPosition;
    int MaxScale = 3, MinScale = 1;


    private void Update()
    {
        if(getOverUI()== "CanvasWebViewPrefabView")
        ZoomImgByMousePos(this.transform.gameObject);
        if (this.transform.localScale == new Vector3(1, 1, 1)) 
        {
            this.GetComponent<RectTransform>().anchoredPosition = new Vector3(0,39,0);
            this.GetComponent<RectTransform>().pivot = new Vector2(0.5f,0.5f);


        }
    }

    //ZoomObj是需要缩放的UI
    private void ZoomImgByMousePos(GameObject ZoomObj)
    {
        //判断鼠标滚轮是否滚动
        if (Input.GetAxis("Mouse ScrollWheel") == 0)
            return;

        //一些变量的声明
        RectTransform RectTran = ZoomObj.GetComponent<RectTransform>();
        float PivotX = RectTran.pivot.x;
        float PivotY = RectTran.pivot.y;
        float OffsetX = 0f;
        float OffsetY = 0f;
        Vector2 addOffset;

        //获取轴心点在屏幕的坐标
        previewPosition = Camera.main.WorldToScreenPoint(RectTran.position);

        //获取鼠标坐标
        newPosition = Input.mousePosition;

        //计算偏差值
        addOffset = newPosition - previewPosition;

        //计算轴心点偏移量
        if (RectTran.rect.width != 0 && ZoomObj.transform.localScale.x != 0)
            OffsetX = addOffset.x / RectTran.rect.width / RectTran.localScale.x;
        if (RectTran.rect.height != 0 && ZoomObj.transform.localScale.y != 0)
            OffsetY = addOffset.y / RectTran.rect.height / RectTran.localScale.y;

        //计算轴心点新值
        RectTran.pivot += new Vector2(OffsetX, OffsetY);

        //计算UI新位置
        RectTran.anchoredPosition += addOffset;

        //放大UI
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
            ZoomObj.transform.localScale += (ZoomObj.transform.localScale.x >= MaxScale - 0.1f ? Vector3.zero : Vector3.one * 0.1f);
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            ZoomObj.transform.localScale += (ZoomObj.transform.localScale.x < MinScale + 0.1f ? Vector3.zero : Vector3.one * -0.1f);
    }
    public string getOverUI()
    {
        GraphicRaycaster _raycaster = FindObjectOfType<GraphicRaycaster>();
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.pressPosition = Input.mousePosition;
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        _raycaster.Raycast(eventData, results);
        if(results.Count>0)
        return results[0].gameObject.name;
        return "null";
    }


}
