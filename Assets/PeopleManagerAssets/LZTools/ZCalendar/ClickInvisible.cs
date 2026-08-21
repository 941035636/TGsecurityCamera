using UnityEngine;
using UnityEngine.EventSystems;
using ZTools;
public class ClickInvisible : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    bool isPointerInside = false;   //维护一个布尔值 来判断当前的鼠标在不在物体的范围

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            if (isPointerInside)
            {
                //这里写你要进行的操作c
            }
            else
            {
                gameObject.transform.parent.parent.transform.GetComponent<ChoiceTime>().StartChoiceTime();    //我这里是隐藏它
                                                                                                              //Destroy(gameObject);    //你也可以删除
            }


        }

    }
}