using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropDownHelper : TMP_Dropdown
{
    // 重写单击下拉列表触发的事件
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        // 物体不可交互 或者 物体没被激活 的时候直接返回
        if (!IsInteractable() || !IsActive()) return;

        // 取 ScrollRect 组件身上的竖直滑动条
        Scrollbar scrollbar = gameObject.GetComponentInChildren<ScrollRect>()?.verticalScrollbar;

        // 没有找到活动条 或者 选项列表的长度小一 的时候直接返回
        // 这里的 options 指的是下拉列表中所有的元素(项目)
        if (scrollbar == null || options.Count <= 1) return;

        // 判断滚动条的方向
        if (scrollbar.direction != Scrollbar.Direction.BottomToTop)
        {
           Log.Debug("滚动条方向有误，请检查！");
            return;
        }

        // 这里的 value 值是下拉列表中当前选择项目的索引号
        // 0 是下拉菜单中的第一个选项，1 是第二个选项，依此类推
        scrollbar.value = Mathf.Max(0.001f, 1.0f - (float)value / (options.Count - 1));
    }

}
