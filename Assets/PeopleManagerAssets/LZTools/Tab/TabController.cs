using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZTools;

public class TabController : MonoBehaviour
{
    public List<Tab> tablist = new List<Tab>();
    /// <summary>
    /// 改变状态
    /// </summary>
    /// <param name="tabName"></param>
    public void ChoiceTab(int tabName)
    {
        tablist.ForEach(item =>
        {
            item.ChangeTab(tabName);
        });
    }
    public void AddTab(Tab obj)
    {
        if (!tablist.Contains(obj))
        {
            tablist.Add(obj);
        }
    }
    public void RemoveTab(Tab obj)
    {
        if (tablist.Contains(obj))
        {
            tablist.Remove(obj);
        }
    }
}
