using UnityEngine;
using UnityEngine.EventSystems;
using ZTools;
public class ClickToInvisiblePeople : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    bool isPointerInsideP = false;   //维护一个布尔值 来判断当前的鼠标在不在物体的范围

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInsideP = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInsideP = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (gameObject.transform.parent.localPosition.y != 5000)
            {
                if (isPointerInsideP)
                {
                    //这里写你要进行的操作c
                }
                else
                {
                    gameObject.transform.parent.gameObject.GetComponent<ZCalendar>().Hide();    //我这里是隐藏它
                                                                                                //Destroy(gameObject);    //你也可以删除
                }
            }

        }

    }
}