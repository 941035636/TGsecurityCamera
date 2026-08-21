using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 层级菜单项
/// </summary>
public class BaseLevelPartObj : MonoBehaviour
{
    public Transform LeftToolBar;//该项的左边工具栏
    public static BaseLevelPartObj CurSelectedBLPO;
    public Text TextTitle;//标题
    public Image ImageBtnBg;//背景按钮
    public Image[] ImageBtnFold;//折叠按钮0时折叠，1是展开
    public Image Icon;//更换icon图标
    public RectTransform M_MySelfRectT { get; private set; }
    public ContentSizeFitter M_SubObjParentObj { get; private set; }//下级菜单的父物体
    public BaseLevelPartObj M_MyParetnObj { get; private set; }//我的父物体
    public PartsLevel M_Level { get; private set; }//我的层级
    public float M_HeightAllSubItems { get; private set; } = 0;//整个子列表的加起来的高度,包括所有的子孙项
    public bool M_IsBeSelected { get; set; }
    private Vector2 PriFloldSizeDelta;//折叠时候的位置
    private Vector2 CurUnFoldSizeDelta;//当前展开的位置
    public bool M_IsFold { get; private set; } = true;//是否折叠
    private static float TimeLastClick;//上次点中的时间
    private int numSubItems = 0;//子项的数目
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void SetFoldImage(bool isFold)
    {
        if (numSubItems == 0) return;//没有子菜单就无所谓展开与否了
        ImageBtnFold[0].gameObject.SetActive(!isFold);
        ImageBtnFold[1].gameObject.SetActive(isFold);
        Icon.sprite = Resources.Load<Sprite>("UI/录像回放/文件夹-0");
        transform.Find("LeftToolBarList/Add").gameObject.SetActive(false);
    }
    public void AddAllSubItemHeight(float height)
    {
        M_HeightAllSubItems += height;
    }
    /// <summary>
    /// 是否折叠子列表
    /// </summary>
    /// <param name="isFold"></param>
    public void Btn_FoldSubList(bool isFold)
    {
        if (numSubItems == 0) return;//没有子菜单就无所谓展开与否了
        M_IsFold = isFold;
        ContentSizeFitter tempSubItemsParent = M_MyParetnObj == null ? UIManger.M_Instance.ParentPartFirstLevel : M_MyParetnObj.M_SubObjParentObj;
        tempSubItemsParent.enabled = false;
        M_SubObjParentObj.gameObject.SetActive(!isFold);
        SetFoldImage(isFold);
        float tempSign = isFold ? -1 : 1;
        if (isFold)
        {
            M_MySelfRectT.sizeDelta = PriFloldSizeDelta;
        }
        else
        {
            CurUnFoldSizeDelta = PriFloldSizeDelta + new Vector2(0, M_HeightAllSubItems);
            M_MySelfRectT.sizeDelta = CurUnFoldSizeDelta;
        }
        tempSubItemsParent.enabled = true;
        //给父物体也增加高度并且刷新
        if (null != M_MyParetnObj)
        {
            M_MyParetnObj.SubItemFoldAddHeight(tempSign * M_HeightAllSubItems);
        }
        // Log.Debug(TextTitle.text);
    }
    //搜索项选中的时候自动展开其所有父类
    public bool SearcherSelectedUnFlod()
    {
        bool isOver = false;
        if (null != M_MyParetnObj)
        {
            M_MyParetnObj.SearcherSelectedUnFlod();
        }
        if (M_IsFold)
            Btn_FoldSubList(false);
        return isOver;
    }
    /// <summary>
    /// 子菜单展开带来的高度增加
    /// </summary>
    public void SubItemFoldAddHeight(float height)
    {
        AddAllSubItemHeight(height);
        ContentSizeFitter tempSubItemsParent = M_MyParetnObj == null ? UIManger.M_Instance.ParentPartFirstLevel : M_MyParetnObj.M_SubObjParentObj;
        //如果是一级列表则是最上面的那个父物体
        tempSubItemsParent.enabled = false;
        CurUnFoldSizeDelta = PriFloldSizeDelta + new Vector2(0, M_HeightAllSubItems);
        M_MySelfRectT.sizeDelta = CurUnFoldSizeDelta;
        tempSubItemsParent.enabled = true;
    }
    public virtual void Btn_PointEnter()
    {
        if (M_IsBeSelected) return;
        ImageBtnBg.color = UIManger.M_Instance.ColorBtnSelf[1];
        if (numSubItems == 0)
        {
            transform.Find("LeftToolBarList/Add").gameObject.SetActive(true);
        }
    }
    public virtual void Btn_PointExit()
    {
        if (M_IsBeSelected) return;
        ImageBtnBg.color = UIManger.M_Instance.ColorBtnSelf[0];
        if (numSubItems == 0)
        {
            transform.Find("LeftToolBarList/Add").gameObject.SetActive(false);
        }
    }
    public virtual void Btn_PointUp()
    {
        if (null != CurSelectedBLPO && CurSelectedBLPO != this)
        {
            CurSelectedBLPO.M_IsBeSelected = false;
            CurSelectedBLPO.ImageBtnBg.color = UIManger.M_Instance.ColorBtnSelf[0];
        }
        CurSelectedBLPO = this;
        //   UIManger.M_Instance.SelectedBLPobj(this);
        M_IsBeSelected = true;
        ImageBtnBg.color = UIManger.M_Instance.ColorBtnSelf[2];
    }
    public virtual void Btn_PointDown()
    {
        if (M_IsBeSelected)
        {
            if (Time.time - TimeLastClick < 0.2f)
            {
                //    Log.Debug("点击了一次！");
            }
            TimeLastClick = Time.time;
        }
        ImageBtnBg.color = UIManger.M_Instance.ColorBtnSelf[3];
        RectTransform tempContent = UIManger.M_Instance.ParentPartFirstLevel.GetComponent<RectTransform>();
        Vector3 tempVecCotent = tempContent.localPosition;
        float tempOffsetHeight = tempContent.sizeDelta.y - UIManger.M_Instance.ScrollViewRect.sizeDelta.y;
        float tempSelectY = tempContent.parent.InverseTransformPoint(LeftToolBar.position).y;
        // Log.Debug("选中的高度:" + tempSelectY + "滚轮的高度：" + tempVecCotent.y);
    }
    //有子物体了，设置其折叠按钮
    public void AddSubObj(int num)
    {
        numSubItems += num;
        SetFoldImage(true);
    }
    //搜素失活状态
    public void SearcherUnActive()
    {
        SetParetn(null);
        gameObject.SetActive(false);
        UnActivePriVec = M_MySelfRectT.sizeDelta;
        M_MySelfRectT.sizeDelta = PriFloldSizeDelta;//失活的时候让每个项都保持在折叠的状态
    }
    Vector2 UnActivePriVec;
    //从搜素中恢复
    public void RestoreFromSearcher()
    {
        if (numSubItems > 0)
        {
            SetFoldImage(M_IsFold);
        }
        gameObject.SetActive(true);
        SetParetn(M_MyParetnObj);
        M_MySelfRectT.sizeDelta = UnActivePriVec;
    }
    //搜素匹配到
    public void SearcherMatch()
    {
        //先激活物体
        gameObject.SetActive(true);
        //折叠、展开按钮都失活
        ImageBtnFold[0].gameObject.SetActive(false);
        ImageBtnFold[1].gameObject.SetActive(false);
    }
    public void SetParetn(BaseLevelPartObj parent)
    {
        if (null != parent)
        {
            transform.SetParent(parent.M_SubObjParentObj.transform);
        }
        else
        {
            //如果是一级菜单则其父物体为最上面的内容管理器
            transform.SetParent(UIManger.M_Instance.ParentPartFirstLevel.transform);
        }
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
    }

    public virtual void Init(string partName, BaseLevelPartObj parent, PartsLevel level)
    {
        ImageBtnBg.color = UIManger.M_Instance.ColorBtnSelf[0];//初始化背景图
        M_SubObjParentObj = GetComponentInChildren<ContentSizeFitter>(true);
        M_MySelfRectT = GetComponent<RectTransform>();
        PriFloldSizeDelta = M_MySelfRectT.sizeDelta;
        CurUnFoldSizeDelta = PriFloldSizeDelta;
        M_MyParetnObj = parent;
        SetParetn(parent);
        M_Level = level;
        TextTitle.text = partName;
        //SetLevelShow();
        ImageBtnFold[0].gameObject.SetActive(false);
        ImageBtnFold[1].gameObject.SetActive(false);
        if (null != M_MyParetnObj)
        {
            M_MyParetnObj.M_SubObjParentObj.gameObject.SetActive(false); ////一开始除了一级菜单都不显示
            M_MyParetnObj.AddAllSubItemHeight(M_MySelfRectT.rect.height);//增加当前物体的父物体子列表高度
        }
    }
}
