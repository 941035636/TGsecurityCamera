using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


    /// <summary>
    /// 日历工具
    /// </summary>
   
    public class Calendar : MonoBehaviour
    {
        //日期预制体
        [SerializeField] private GameObject dayPrefab;
        //小时预制体
        [SerializeField] private GameObject hourPrefab;
        //分钟预制体
        [SerializeField] private GameObject minutePrefab;

        //年份文本
        [SerializeField] private Text yearText;
        //月份文本
        [SerializeField] private Text monthText;
        //小时:分钟 文本
        [SerializeField] private Text timeText;

        [Tooltip("仅显示本月份的日期项")]
        [SerializeField] private bool onlyCurrentMonthDays = true;
        [Tooltip("历史日期是否可交互")]
        [SerializeField] private bool historyDaysInteractable = false;
    /// <summary>
    /// 编辑结束事件
    /// </summary>
    [SerializeField] public UnityEvent<DateTime> onEndEdit;
    private int currentYear;
        private int currentMonth;
        private int currentDay;
        private int currentHour;
        private int currentMinute;

        /// <summary>
        /// 当前选中的时间
        /// </summary>
        public DateTime CurrentSelectedTime
        {
            get
            {
                return new DateTime(currentYear, currentMonth, currentDay, currentHour, currentMinute, 0);
            }
        }
    

        private void Start()
        {
            DateTime now = DateTime.Now;
            Init(now.Year, now.Month, now.Day, now.Hour, now.Minute);
            SelectTime(now);
        }
   
        //初始化日历
        public void Init(int year, int month, int day, int hour, int minute)
        {
            //首先清空
            Clear();
            //记录当前时间
            currentYear = year;
            currentMonth = month;
            currentDay = day;
            currentHour = hour;
            currentMinute = minute;

            //填充年份
            yearText.text = string.Format("{0}年", currentYear);
            //填充月份
            monthText.text = string.Format("{0}月", currentMonth);
            //获取该月份的天数
            int daysCount = DateTime.DaysInMonth(currentYear, currentMonth);
            //获取该月份一号是星期几
            int weekOfFirstDay = (int)new DateTime(currentYear, currentMonth, 1).DayOfWeek;
            //获取该月份最后一天是星期几
            int weekOfEndDay = (int)new DateTime(currentYear, currentMonth, daysCount).DayOfWeek;
            //上个月份
            DateTime prevMonth = new DateTime(currentYear, currentMonth, 1).AddMonths(-1);
            //上个月份的天数
            int prevMonthDaysCount = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            //一号之前的占位
            for (int i = weekOfFirstDay - 1; i >= 0; i--)
            {
                CreateDayItem(onlyCurrentMonthDays ? 0 : (prevMonthDaysCount - i), false);
            }
            //实例化该月份的所有日期项
            for (int i = 0; i < daysCount; i++)
            {
                CreateDayItem(i + 1, historyDaysInteractable ? true :(new DateTime(currentYear,currentMonth, i + 1) - 
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).TotalDays >= 0);
            }
            //最后一天之后的占位
            for (int i = 0; i < 6 - weekOfEndDay; i++)
            {
                CreateDayItem(onlyCurrentMonthDays ? 0 : (i + 1), false);
            }
            //实例化24小时项
            for (int i = 0; i < 24; i++)
            {
                CreateHourItem(i);
            }
            //实例化60分钟项
            for (int i = 0; i < 60; i++)
            {
                CreateMinuteItem(i);
            }
        }
        //创建日期项
        private void CreateDayItem(int day, bool interactable)
        {
            //实例化
            var instance = Instantiate(dayPrefab);
            //设置父级
            instance.transform.SetParent(dayPrefab.transform.parent, false);
            //显示物体
            instance.SetActive(true);
            //名称设为日期
            instance.name = day.ToString();
            //大于0正常显示日期
            if (day > 0)
            {
                //将日期设置到文本中
                instance.GetComponentInChildren<Text>().text = day.ToString();
                //获取Toggle组件
                Toggle toggle = instance.GetComponent<Toggle>();
                //是否可交互
                toggle.interactable = interactable;
                //不可交互时文本颜色置灰
                toggle.GetComponentInChildren<Text>().color = interactable ? Color.white : Color.grey;
                //绑定事件
                toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                    {
                        currentDay = day;
                    }
                });
            }
            //占位
            else
            {
                //销毁Toggle、Image组件
                Destroy(instance.GetComponent<Toggle>());
                Destroy(instance.GetComponent<Image>());
                //销毁两个子物体
                for (int i = 0; i < 2; i++)
                {
                    Destroy(instance.transform.GetChild(i).gameObject);
                }
            }
        }
        //创建小时项
        private void CreateHourItem(int hour)
        {
            //实例化
            var instance = Instantiate(hourPrefab);
            //设置父级
            instance.transform.SetParent(hourPrefab.transform.parent, false);
            //显示物体
            instance.SetActive(true);
            //小时格式化为00格式
            string hourString = string.Format("{0:D2}", hour);
            //名称设为小时
            instance.name = hourString;
            //将小时设置到文本中
            instance.GetComponentInChildren<Text>().text = hourString;
            //获取Toggle组件
            Toggle toggle = instance.GetComponent<Toggle>();
            //绑定事件
            toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    currentHour = hour;
                    timeText.text = string.Format("{0:D2}:{1:D2}", currentHour, currentMinute);
                }
            });
        }
        //创建分钟项
        private void CreateMinuteItem(int minute)
        {
            //实例化
            var instance = Instantiate(minutePrefab);
            //设置父级
            instance.transform.SetParent(minutePrefab.transform.parent, false);
            //显示物体
            instance.SetActive(true);
            //分钟格式化为00格式
            string minuteString = string.Format("{0:D2}", minute);
            //名称设为分钟
            instance.name = minuteString;
            //将分钟设置到文本中
            instance.GetComponentInChildren<Text>().text = minuteString;
            //获取Toggle组件
            Toggle toggle = instance.GetComponent<Toggle>();
            //绑定事件
            toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    currentMinute = minute;
                    timeText.text = string.Format("{0:D2}:{1:D2}", currentHour, currentMinute);
                }
            });
        }
        //清除日历内容
        private void Clear()
        {
            //清空日期项
            Transform day = dayPrefab.transform.parent;
            if (day.childCount > 1)
            {
                for (int i = 1; i < day.childCount; i++)
                {
                    Destroy(day.GetChild(i).gameObject);
                }
            }
            //清空小时项
            Transform hour = hourPrefab.transform.parent;
            if (hour.childCount > 1)
            {
                for (int i = 1; i < hour.childCount; i++)
                {
                    Destroy(hour.GetChild(i).gameObject);
                }
            }
            //清空分钟项
            Transform minute = minutePrefab.transform.parent;
            if (minute.childCount > 1)
            {
                for (int i = 1; i < minute.childCount; i++)
                {
                    Destroy(minute.GetChild(i).gameObject);
                }
            }
        }

        /// <summary>
        /// 上个月份按钮点击事件
        /// </summary>
        public void OnPrevMonthButtonClick()
        {
            currentMonth--;
            if (currentMonth < 1)
            {
                currentMonth = 12;
                currentYear--;
            }
            Init(currentYear, currentMonth, currentDay, currentHour, currentMinute);
        }
        /// <summary>
        /// 下个月份按钮点击事件
        /// </summary>
        public void OnNextMonthButtonClick()
        {
            currentMonth++;
            if (currentMonth > 12)
            {
                currentMonth = 1;
                currentYear++;
            }
            Init(currentYear, currentMonth, currentDay, currentHour, currentMinute);
        }
        /// <summary>
        /// 上个年份按钮点击事件
        /// </summary>
        public void OnPrevYearButtonClick()
        {
            Init(currentYear - 1, currentMonth, currentDay, currentHour, currentMinute);
        }
        /// <summary>
        /// 下个年份按钮点击事件
        /// </summary>
        public void OnNextYearButtonClick()
        {
            Init(currentYear + 1, currentMonth, currentDay, currentHour, currentMinute);
        }
        /// <summary>
        /// 选中当前时间按钮点击事件
        /// </summary>
        public void OnSelectNowButtonClick()
        {
            SelectTime(DateTime.Now);
        }
        /// <summary>
        /// 确认按钮点击事件
        /// </summary>
        public void OnConfirmButtonClick()
        {
            //onEndEdit?.Invoke(CurrentSelectedTime);
            Debug.Log(CurrentSelectedTime.ToString("yyyy-MM-dd HH: mm:ss"));
        this.gameObject.SetActive(false);
            //Debug.Log("起始时间：" + CurrentSelectedTime.ToString("yyyy-MM-dd HH: mm:ss") + "结束时间：" + CurrentSelectedTime.ToString("yyyy-MM-dd HH: mm:ss"));
        }
        /// <summary>
        /// 选中时间
        /// </summary>
        /// <param name="dateTime">时间</param>
        public void SelectTime(DateTime dateTime)
        {
            //如果日历当前内容不是当前年份和月份 重新初始化日历
            if (dateTime.Year != currentYear || dateTime.Month != currentMonth)
            {
                Init(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute);
            }
            //当前月份的1号是周几 代表1号前面有几个占位物体
            int weekOfFirstDay = (int)new DateTime(dateTime.Year, dateTime.Month, 1).DayOfWeek;
            //选中当前日期
            dayPrefab.transform.parent.GetChild(weekOfFirstDay + dateTime.Day).GetComponent<Toggle>().isOn = true;
            //选中当前小时
            hourPrefab.transform.parent.GetChild(dateTime.Hour + 1).GetComponent<Toggle>().isOn = true;
            //选中当前分钟
            minutePrefab.transform.parent.GetChild(dateTime.Minute + 1).GetComponent<Toggle>().isOn = true;
        }
    }
