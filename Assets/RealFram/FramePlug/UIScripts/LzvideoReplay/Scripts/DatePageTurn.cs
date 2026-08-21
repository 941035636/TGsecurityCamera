using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DatePageTurn : SingletonManager<MonoBehaviour>
{
    //滚动条
    public ScrollRect scrollRect;
    //左右按钮
    public Button leftButton, rightButton;

    //滚动阈值
    public float Threshold = 0.2f;
    //缓动速度
    public float speed = 10f;

    //是否开启拖拽滚动
    private bool IsStart = false;

    //总个数
    private int Number;

    //当前进行到的个数
    private int currentNumber;
    private int CurrentNumber
    {
        get
        {
            return currentNumber;
        }
        set
        {
            currentNumber = value;
            //更新按钮显示状态
            ButtonUIUpdate();
        }
    }

    /// <summary>
    /// 初始化轮播
    /// </summary>
    /// <param name="_number">总页数</param>
    /// <param name="_currentNumber">初始页数</param>
    public void OnInitRollScroll(int _number, int _currentNumber)
    {
        if (_number <= 1) return;
        //总个数
        Number = _number;
        //当前个数
        CurrentNumber = _currentNumber;
        //设置起点
        StartCoroutine(HandlePosition(GetScrollPosition(CurrentNumber)));
        //content = scrollRect.content;
        //左按钮点击事件
        leftButton.onClick.RemoveAllListeners();
        leftButton.onClick.AddListener(OnLeft);
        //右按钮点击事件
        rightButton.onClick.RemoveAllListeners();
        rightButton.onClick.AddListener(OnRight);
        IsStart = true;
    }
    //延迟执行设置初始化滚动起点
    private IEnumerator HandlePosition(float number)
    {
        yield return null;
        yield return null;
        scrollRect.horizontalNormalizedPosition = number;
    }

    #region 拖拽事件
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsStart) return;
        StopAllCoroutines();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsStart) return;
        // Log.Debug("结束拖拽");
        //滚动前的position
        float lastPosition = GetScrollPosition(currentNumber);
        //滚动后的position
        float currentPosition = scrollRect.horizontalNormalizedPosition;
        if (currentPosition > lastPosition)
        {
            float n = 1f / (Number - 1);
            float thre = currentPosition - lastPosition;
            if (thre > n * Threshold)
            {
                OnRight();
            }
            else //还原
            {
                StopAllCoroutines();
                StartCoroutine(LerpRoll(lastPosition));
            }
        }
        else
        {
            float n = 1f / (Number - 1);
            float thre = lastPosition - currentPosition;
            if (thre > n * Threshold)
            {
                OnLeft();
            }
            else //还原
            {
                StopAllCoroutines();
                StartCoroutine(LerpRoll(lastPosition));
            }
        }
    }
    #endregion

    #region 左右按钮事件

    //左按钮事件
    private void OnLeft()
    {
        if (CurrentNumber > 0)
        {
            CurrentNumber--;
            StopAllCoroutines();
            StartCoroutine(LerpRoll(GetScrollPosition(CurrentNumber)));
        }
    }
    //右按钮事件
    private void OnRight()
    {
        if (CurrentNumber < Number - 1)
        {
            CurrentNumber++;
            StopAllCoroutines();
            StartCoroutine(LerpRoll(GetScrollPosition(CurrentNumber)));
        }
    }

    #endregion

    #region 功能方法

    //缓动滚动到...
    private IEnumerator LerpRoll(float position)
    {
        while (true)
        {
            if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - position) <= 0.001f)
            {
                scrollRect.horizontalNormalizedPosition = position;
                break;
            }
            else
            {
                scrollRect.horizontalNormalizedPosition = Mathf.Lerp(scrollRect.horizontalNormalizedPosition, position, Time.deltaTime * speed);
            }
            yield return null;
        }
    }

    /// <summary>
    /// 获取滚动条value 0 - 1的值
    /// </summary>
    private float GetScrollPosition(int number)
    {
        float n = 1f / (Number - 1);
        float scrollPosition = Mathf.Clamp(n * number, 0, 1);
        // Log.Debug(scrollPosition);
        return scrollPosition;
    }

    //按钮更新显示状态
    private void ButtonUIUpdate()
    {
        if (Number == 1)
        {
            leftButton.interactable = false;
            rightButton.interactable = false;
            return;
        }
        if (currentNumber == 0)
        {
            leftButton.interactable = false;
            rightButton.interactable = true;
        }
        else if (currentNumber == Number - 1)
        {
            leftButton.interactable = true;
            rightButton.interactable = false;
        }
        else
        {
            leftButton.interactable = true;
            rightButton.interactable = true;
        }
    }
    #endregion
}