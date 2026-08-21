using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DoubleClikeButton : Toggle
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
            OnDoubleClick.Invoke();
        else
            ResetTime();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (m_firstTime.Equals(default(DateTime)))
            m_firstTime = DateTime.Now;
        else
        {
            m_SecondTime = DateTime.Now;
        }
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (!m_firstTime.Equals(default(DateTime)) && !m_SecondTime.Equals(default(DateTime)))
        {
            var intervalTime = m_SecondTime - m_firstTime;
            float milliTime = intervalTime.Seconds * 1000 + intervalTime.Milliseconds;
            if (milliTime < 400)
            {
                Press();
            }
            else
                ResetTime();
        }

    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        ResetTime();
    }
}