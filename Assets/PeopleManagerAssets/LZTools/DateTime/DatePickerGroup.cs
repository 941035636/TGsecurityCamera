using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
/// <summary>
/// 日期选择组
/// </summary>
public class DatePickerGroup : MonoBehaviour
{
    private static DatePickerGroup instance;
    public static DatePickerGroup GetInstance()
    {
        if (instance == null)
        {
            instance = new DatePickerGroup();
        }
        return instance;
    }
    /// <summary>
    /// 最小日期和最大日期
    /// </summary>
    public DateTime _minDate, _maxDate;
    /// <summary>
    /// 选择的日期（年月日时分秒）
    /// </summary>
    public DateTime _selectDate;
    /// <summary>
    /// 时间选择器列表
    /// </summary>
    public List<DatePicker> _datePickerList;
    /// <summary>
    /// 当选择日期的委托事件
    /// </summary>
    public event OnDateUpdate _OnDateUpdate;

    public static DateTime _selectTime;

    public TextMeshProUGUI TimeTxt;
    void Awake()
    {
        //设置最大最小日期
        _minDate = new DateTime(1999, 1, 1, 0, 0, 0);
        _maxDate = new DateTime(2050, 1, 1, 0, 0, 0);
        Init();
        //EventCenter.addlistener<DateTime>(Eventdefine.ChoiceDatetime,Init);
    }

    private void Update()
    {

    }
    public void Init(DateTime dt)
    {
        _selectDate = dt;
        Log.Debug("Init:" + _selectDate);
        for (int i = 0; i < _datePickerList.Count; i++)
        {
            _datePickerList[i].myGroup = this;
            _datePickerList[i].Init();
            _datePickerList[i]._onDateUpdate += onDateUpdate;
        }
    }
    public void Init()
    {
        _selectDate = DateTime.Now;
        for (int i = 0; i < _datePickerList.Count; i++)
        {
            _datePickerList[i].myGroup = this;
            _datePickerList[i].Init();
            _datePickerList[i]._onDateUpdate += onDateUpdate;
        }
    }

    /// <summary>
    /// 当选择的日期更新
    /// </summary>

    public static string Startdate = string.Empty;
    public static string Enddate = string.Empty;
    public static string TotalDay = string.Empty;
  
    public void onDateUpdate()
    {
        // Log.Debug("当前选择日期：" + _selectDate.ToString("yyyy年MM月dd日 HH : mm : ss"));

        //Time = _selectDate.Hour.ToString()+ _selectDate.Minute.ToString()+ _selectDate.Second.ToString();
        //Time = _selectDate.ToString("HHMMss");

  

  
      
        if (ZCalendarDemo.IsStart)
        {
            //Log.Error("选中的起始时间:" + ZCalendarDemo.Date + " " + Time);
            Startdate = ZCalendarDemo.Date;
            Log.Debug("选中的起始时间:" + Startdate);
        }
        else
        {
            //Log.Error("选中的截止时间:" + ZCalendarDemo.Date + " " + Time);
            Enddate = ZCalendarDemo.Date ;
            //ZCalendarDemo.IsEnd = false;
            Log.Debug("选中的截止时间:" + Enddate);
        }


        //将选中的时间给_selectTime ，供其他界面调用
        _selectTime = _selectDate;
        for (int i = 0; i < _datePickerList.Count; i++)
        {
            _datePickerList[i].RefreshDateList();
        }
    
        TotalDay = Startdate + " " + Enddate;

    }
    private void OnDestroy()
    {
        //EventCenter.RemoveListener<DateTime>(Eventdefine.ChoiceDatetime, Init);
    }
}

