using System;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class UIButtonDoubleClick : Toggle
{

    [Serializable]
    public class DoubleClickEvent : UnityEvent { }
    [SerializeField]
    private DoubleClickEvent _onDoubleClick = new DoubleClickEvent();
    public DoubleClickEvent OnDoubleClick
    {
        get
        {
            return _onDoubleClick;
        }

        set
        {
            _onDoubleClick = value;
        }
    }

    [Serializable]
    public class RightClickEvent : UnityEvent { }
    [SerializeField]
    private RightClickEvent _onrightClick = new RightClickEvent();
    public RightClickEvent OnRightClick
    {
        get
        {
            return _onrightClick;
        }

        set
        {
            _onrightClick = value;
        }
    }


    [Serializable]
    public class LeftClickEvent : UnityEvent { }
    [SerializeField]
    private LeftClickEvent _onleftClick = new LeftClickEvent();
    public LeftClickEvent OnLeftClick
    {
        get
        {
            return _onleftClick;
        }

        set
        {
            _onleftClick = value;
        }
    }


    [Serializable]
    public class ScrollWheelEvent : UnityEvent { }
    [SerializeField]
    private ScrollWheelEvent _scrollWheel = new ScrollWheelEvent();
    public ScrollWheelEvent OnscrollWheel
    {
        get
        {
            return _scrollWheel;
        }

        set
        {
            _scrollWheel = value;
        }
    }


    private DateTime m_firstTime;
    private DateTime m_SecondTime;
    private void ResetTime()
    {
        m_firstTime = default(DateTime);
        m_SecondTime = default(DateTime);
    }
    private void Press()
    {
        if (OnDoubleClick != null)
        {
            OnDoubleClick.Invoke();
            ResetTime();
        }

        else
            ResetTime();
    }
    private void RightPress()
    {
        if (OnRightClick != null)
        {
            OnRightClick.Invoke();
        }

    }
    private void LeftPress()
    {
        if (OnLeftClick != null)
        {
            OnLeftClick.Invoke();
        }

    }
    private void ScrollWheel() 
    {
        if (OnscrollWheel!=null)
        {
            OnscrollWheel.Invoke();
        }
    
    }


    public override void OnPointerDown(PointerEventData eventData)
    {

        if (Input.GetMouseButtonDown(0))
        {
            base.OnPointerDown(eventData);
            if (m_firstTime.Equals(default(DateTime)))
                m_firstTime = DateTime.Now;
            else
            {
                m_SecondTime = DateTime.Now;
            }
            LeftPress();
        }
        else
        {
            Log.Debug("鼠标右键点击 ");
            RightPress();
        }

    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        if (Input.GetMouseButtonUp(0))
        {
            base.OnPointerUp(eventData);
            if (!m_firstTime.Equals(default(DateTime)) && !m_SecondTime.Equals(default(DateTime)))
            {
                var intervalTime = m_SecondTime - m_firstTime;
                float milliTime = intervalTime.Seconds * 1000 + intervalTime.Milliseconds;
                if (milliTime < 4000)
                {
                    Press();
                    Log.Debug("间隔正常:" + milliTime);
                }
                else
                {
                    Log.Debug("间隔太长:"+milliTime);
                    ResetTime();
                    //Press();
                   
                }

            }
            else
            {

            }
        }


    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (Input.GetMouseButtonUp(0))
        {
            base.OnPointerExit(eventData);
            ResetTime();

        }

    }



    //public override void OnPointerStay (PointerEventData eventData)
    //{
    //    base.OnPointerEnter(eventData);
    //float mouseCenter = Input.GetAxis("Mouse ScrollWheel");
    //    if (mouseCenter==0)
    //    {
    //        return;
    //    }
    //    //if (RectTransformUtility.RectangleContainsScreenPoint(transform.GetComponent<RectTransform>(),Input.mousePosition))
    //    //{

    //    //}
    //    ScrollWheel();

    //}

    private void Update()
    {
        float mouseCenter = Input.GetAxis("Mouse ScrollWheel");
        if (mouseCenter == 0)
        {
            return;
        }
        else 
        {
            ScrollWheel();
        }
        //if (RectTransformUtility.RectangleContainsScreenPoint(transform.GetComponent<RectTransform>(),Input.mousePosition))
        //{

        //}
      
    }



}
