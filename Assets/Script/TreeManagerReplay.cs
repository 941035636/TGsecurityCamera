using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using System;
using static ManManagentUi;
using zFramework.Media;
using static ReplayUIController;
//using WSX.HS;

namespace SuperTreeView
{
    public class TreeManagerReplay : MonoBehaviour
    {
        public TreeView mTreeView;
        public int mCurSelectedItemId = 0;
        public string currentItemId = "";//当前选中id
        string currentLevel = "";//当前选中父层级id拼接（逗号分割）

        int mNewItemCount = 0;

        List<OutlineInfo> outlineInfoList = new List<OutlineInfo>();
        List<TreeViewItem> allTreeViewItemList = new List<TreeViewItem>();
        public static string ChoiceAreaName = string.Empty;
        public static string ChoiceAreaId = string.Empty;
        private TreeViewItem currentItem = new TreeViewItem();

        private bool inputFieldType = false;

        private static TreeManager instance;
        public static TreeManager GetInstance()
        {
            if (instance == null)
            {
                instance = new TreeManager();
            }
            return instance;
        }
        bool init = false;




        void Start()
        {
            mTreeView.OnTreeListAddOneItem = OnTreeListAddOneItem;
            mTreeView.OnTreeListDeleteOneItem = OnTreeListDeleteOneItem;
            mTreeView.OnItemExpandBegin = OnItemExpandBegin;
            mTreeView.OnItemCollapseBegin = OnItemCollapseBegin;
            mTreeView.OnItemCustomEvent = OnItemCustomEvent;
            mTreeView.InitView();




            ////******************假数据动态拼接树开始****************************
            //TreeViewItem item1 = mTreeView.AppendItem("ItemPrefab1");
            //item1.GetComponent<ItemScript>().SetItemInfo("全部", "全部", "1");


            //TreeViewItem childItem1_1 = item1.ChildTree.AppendItem("ItemPrefab1");
            //childItem1_1.GetComponent<ItemScript>().SetItemInfo("技术部门", "技术部门", "1_1");
            //TreeViewItem childItem1_2 = item1.ChildTree.AppendItem("ItemPrefab1");
            //childItem1_2.GetComponent<ItemScript>().SetItemInfo("商务部门", "商务部门", "1_2");
            //TreeViewItem childItem1_3 = item1.ChildTree.AppendItem("ItemPrefab1");
            //childItem1_3.GetComponent<ItemScript>().SetItemInfo("行政部门", "行政部门", "1_3");
            //TreeViewItem childItem1_4 = item1.ChildTree.AppendItem("ItemPrefab1");
            //childItem1_4.GetComponent<ItemScript>().SetItemInfo("总裁办", "总裁办", "1_4");



            //TreeViewItem childItem1_1_1 = childItem1_1.ChildTree.AppendItem("ItemPrefab1");
            //childItem1_1_1.GetComponent<ItemScript>().SetItemInfo("技术一部", "技术一部", "1_1_1");
            //TreeViewItem childItem1_1_2 = childItem1_1.ChildTree.AppendItem("ItemPrefab1");
            //childItem1_1_2.GetComponent<ItemScript>().SetItemInfo("技术二部", "技术二部", "1_1_2");

            //OutlineInfo outline0 = new OutlineInfo();
            //outline0.ParentId = "0";
            //outline0.OutlineId = "1";
            //outline0.OutlineName = "全部";
            //OutlineInfo outline = new OutlineInfo();
            //outline.ParentId = "1";
            //outline.OutlineId = "2";
            //outline.OutlineName = "生活区";
            //OutlineInfo outline1 = new OutlineInfo();
            //outline1.ParentId = "2";
            //outline1.OutlineId = "3";
            //outline1.OutlineName = "生活区一号门";
            //OutlineInfo outline2 = new OutlineInfo();
            //outline2.ParentId = "2";
            //outline2.OutlineId = "4";
            //outline2.OutlineName = "生活区二号门";
            //outlineInfoList.Add(outline0);
            //outlineInfoList.Add(outline);
            //outlineInfoList.Add(outline1);
            //outlineInfoList.Add(outline2);
            //Init(outlineInfoList);

            ////******************假数据动态拼接树结束****************************

            //******************后台数据动态拼接树开始****************************
            //outlineInfoList = ServiceManager.Instance().GetAllTree();
            //allTreeViewItemList = new List<TreeViewItem>();
            //TreeViewItem item1 = new TreeViewItem();
            //for (int i = 0; i < outlineInfoList.Count; i++)
            //{
            //    if (string.IsNullOrEmpty(outlineInfoList[i].ParentId) || int.Parse(outlineInfoList[i].ParentId) == 0)
            //    {
            //        item1 = mTreeView.AppendItem("ItemPrefab1");
            //        item1.GetComponent<ItemScript>().id = outlineInfoList[i].OutlineId;
            //        item1.GetComponent<ItemScript>().parentId = outlineInfoList[i].ParentId;
            //        item1.GetComponent<ItemScript>().labelText.text = outlineInfoList[i].OutlineName;
            //        item1.GetComponent<ItemScript>().SetItem(item1.GetComponent<ItemScript>());
            //        allTreeViewItemList.Add(item1);
            //    }
            //    else
            //    {
            //        for (int j = 0; j < allTreeViewItemList.Count; j++)
            //        {
            //            if (allTreeViewItemList[j].GetComponent<ItemScript>().id.Equals(outlineInfoList[i].ParentId))
            //            {
            //                TreeViewItem childItem = allTreeViewItemList[j].ChildTree.AppendItem("ItemPrefab1");
            //                childItem.GetComponent<ItemScript>().id = outlineInfoList[i].OutlineId;
            //                childItem.GetComponent<ItemScript>().parentId = outlineInfoList[i].ParentId;
            //                childItem.GetComponent<ItemScript>().labelText.text = outlineInfoList[i].OutlineName;
            //                childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
            //                allTreeViewItemList.Add(childItem);
            //            }
            //        }
            //    }
            //}
            //OnItemCustomEvent(item1, CustomEvent.ItemClicked, "");
            //******************后台数据动态拼接树结束****************************

            //EventCenter.addlistener<List<OutlineInfo>>(Eventdefine.TreeView,Init);

        }


        //private void OnDestroy()
        //{
        //    EventCenter.RemoveListener<List<OutlineInfo>>(Eventdefine.TreeView, Init);
        //}


        /// <summary>
        /// 封装外部调用初始化分组接口
        /// </summary>
        //public void Init(List<OutlineInfo> outlineInfoList) 
        //{
        //    allTreeViewItemList = new List<TreeViewItem>();
        //    TreeViewItem item1 = new TreeViewItem();
        //    for (int i = 0; i < outlineInfoList.Count; i++)
        //    {
        //        if (string.IsNullOrEmpty(outlineInfoList[i].ParentId) || int.Parse(outlineInfoList[i].ParentId) == 0)
        //        {
        //            item1 = mTreeView.AppendItem("ItemPrefab1");
        //            item1.GetComponent<ItemScript>().id = outlineInfoList[i].OutlineId;
        //            item1.GetComponent<ItemScript>().parentId = outlineInfoList[i].ParentId;
        //            item1.GetComponent<ItemScript>().labelText.text = outlineInfoList[i].OutlineName;
        //            item1.GetComponent<ItemScript>().SetItem(item1.GetComponent<ItemScript>());
        //            allTreeViewItemList.Add(item1);
        //        }
        //        else
        //        {
        //            for (int j = 0; j < allTreeViewItemList.Count; j++)
        //            {
        //                if (allTreeViewItemList[j].GetComponent<ItemScript>().id.Equals(outlineInfoList[i].ParentId))
        //                {
        //                    TreeViewItem childItem = allTreeViewItemList[j].ChildTree.AppendItem("ItemPrefab1");
        //                    childItem.GetComponent<ItemScript>().id = outlineInfoList[i].OutlineId;
        //                    childItem.GetComponent<ItemScript>().parentId = outlineInfoList[i].ParentId;
        //                    childItem.GetComponent<ItemScript>().labelText.text = outlineInfoList[i].OutlineName;
        //                    childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
        //                    childItem.GetComponent<ItemScript>().clickBtn.onClick.AddListener(()=> {

        //                        Log.Debug("点击了：" + childItem.GetComponent<ItemScript>().labelText.text);
        //                    });
        //                    allTreeViewItemList.Add(childItem);
        //                }
        //            }
        //        }
        //    }
        //    OnItemCustomEvent(item1, CustomEvent.ItemClicked, "");


        //}




        private void Update()
        {

            ///****************处理修改、新增层级，取消聚焦触发**********************************
            if (currentItem != null && currentItem.GetComponent<ItemScript>().IsSelected && inputFieldType)
            {
                int status = 1;
                int temp = -1;
                //增加log
                //LogInfo logInfo = new LogInfo();
                //logInfo.UserName = DataBase.Instance().loginUserInfo.UserName;
                inputFieldType = false;
                OutlineInfo outlineInfo = new OutlineInfo();
                outlineInfo.OutlineName = currentItem.GetComponent<ItemScript>().labelText.text;
                outlineInfo.ParentId = currentItem.GetComponent<ItemScript>().parentId;
                outlineInfo.Level = currentLevel;
                //新增
                if (string.IsNullOrEmpty(currentItem.GetComponent<ItemScript>().id))
                {
                    //temp = ServiceManager.Instance().AddOutlineInfo(outlineInfo);
                    //logInfo.OperationName = Common.outLineAddLog;
                }
                else
                {//修改
                    //outlineInfo.OutlineId = currentItem.GetComponent<ItemScript>().id;
                    //temp = ServiceManager.Instance().ChangeOutlineInfo(outlineInfo);
                    //logInfo.OperationName = Common.outLineEditLog;
                }
                if (temp == 0)
                {
                    status = 0;
                }
                //logInfo.UserName = DataBase.Instance().loginUserInfo.UserName;
                //logInfo.OperationStatus = status.ToString();
                //ServiceManager.Instance().AddLog(logInfo);
            }
        }

        public void OnChangeInputField()
        {
            inputFieldType = true;
        }


        void OnItemExpandBegin(TreeViewItem item)
        {
            ItemScript st = item.GetComponent<ItemScript>();
            st.SetExpandStatus(true);
        }

        void OnItemCollapseBegin(TreeViewItem item)
        {
            ItemScript st = item.GetComponent<ItemScript>();
            st.SetExpandStatus(false);
        }



        public void OnItemCustomEvent(TreeViewItem item, CustomEvent customEvent, System.Object param)
        {
            if (customEvent == CustomEvent.ItemClicked)
            {
                ItemScript st = item.GetComponent<ItemScript>();
                //OutLineSetPage.o_l.st = st;
                if (mCurSelectedItemId > 0)
                {
                    if (item.ItemId == mCurSelectedItemId)
                    {
                        return;
                    }
                    TreeViewItem curSelectedItem = mTreeView.GetTreeItemById(mCurSelectedItemId);
                    if (curSelectedItem != null)
                    {
                        curSelectedItem.GetComponent<ItemScript>().IsSelected = false;
                    }
                    mCurSelectedItemId = 0;
                }
                st.IsSelected = true;
                mCurSelectedItemId = item.ItemId;
                //选中后查询对应id大纲列表
                //OutLineSetPage.o_l.ItemId = st.id;
                //OutLineSetPage.o_l.Level = st.id;
                currentItemId = st.id;
                TreeViewItem pItem = item;
                //拼level
                for (int i = 0; i < allTreeViewItemList.Count; i++)
                {
                    TreeViewItem cItem = pItem.transform.parent.parent.GetComponent<TreeViewItem>();
                    if (cItem != null && cItem.name.Contains("ItemPrefab1"))
                    {
                        currentLevel += "," + cItem.GetComponent<ItemScript>().id;
                        //OutLineSetPage.o_l.Level += "," + cItem.GetComponent<ItemScript>().id;
                        pItem = cItem;
                    }
                    else
                    {
                        break;
                    }
                }
                //if (!"1".Equals(st.id))
                //{
                //    OutLineSetPage.o_l.ItemParentId = st.transform.parent.parent.GetComponent<ItemScript>().id;
                //}
                //else
                //{
                //    OutLineSetPage.o_l.ItemParentId = null;
                //}
                //OutLineSetPage.o_l.refresh = true;
                ChoiceAreaName = st.labelText.text;
                ChoiceAreaId = st.id;
              
                UIManger.ChoiceCameraId = st.nvr.id;
                Log.Debug("选择的回放监控id:" + UIManger.ChoiceCameraId);

                ReplayUIController.IsnewPlay = true;
                SendPlayback playback = new SendPlayback();
                playback.camId = UIManger.ChoiceCameraId;
                playback.userId = PlayerPrefs.GetInt("userId");
                //playback.camId = 114;
                //playback.userId = 123;
                playback.startTime = StartTime;
                playback.endTime = EndTime;
                playback.oldUrl = ReplayUIController.Ins.OldUrl;
                string Sendstr = JsonUtility.ToJson(playback);
                Log.Debug("向服务器发送的Json:" + Sendstr);
                if (string.IsNullOrEmpty(playback.startTime) || string.IsNullOrEmpty(playback.endTime))
                {
                    GameStart.Instance.ShowTip("请选择时间段!");
                }
                else if(UIManger.ChoiceCameraId!=0)
                {
                    EventCenter.BroadCast(Eventdefine.SendPlayback, Sendstr);
                    EventCenter.BroadCast(Eventdefine.resetTime);
                }
               

            }
        }


        void OnTreeListAddOneItem(TreeList treeList)
        {

            int count = treeList.ItemCount;
            TreeViewItem parentTreeItem = treeList.ParentTreeItem;
            if (count > 0 && parentTreeItem != null)
            {
                ItemScript st = parentTreeItem.GetComponent<ItemScript>();
                st.SetExpandBtnVisible(true);
                st.SetExpandStatus(parentTreeItem.IsExpand);


            }


        }


        [Serializable]
        public class Codedata
        {
            public string msg = string.Empty;
            public int code;
        }
        public void AddCameraAreaCallback(HttpCallBackArgs args)
        {

            Log.Debug("收到新建监控分组回传信息：" + args.Value);

            if (!string.IsNullOrEmpty(args.Value))
            {
                Codedata data = JsonUtility.FromJson<Codedata>(args.Value);
                if (data.code == 400)
                {
                    GameStart.Instance.ShowTip(data.msg);
                    OnDeleteCameraBtnClicked();
                }
                else if (data.code == 200)
                {
                    GameStart.Instance.ShowTip("添加分组成功");
                }


            }




        }

        public void AddDoorAreaCallback(HttpCallBackArgs args)
        {

            Log.Debug("收到新建监控分组回传信息：" + args.Value);

            if (!string.IsNullOrEmpty(args.Value))
            {
                Codedata data = JsonUtility.FromJson<Codedata>(args.Value);
                if (data.code == 400)
                {
                    GameStart.Instance.ShowTip(data.msg);
                    OnDeleteCameraBtnClicked();
                }
                else if (data.code == 200)
                {
                    GameStart.Instance.ShowTip("添加分组成功");
                }


            }




        }


        public void EditorCameraAreaCallback(HttpCallBackArgs args)
        {

            Log.Debug("收到修改监控分组回传信息：" + args.Value);

            if (!string.IsNullOrEmpty(args.Value))
            {
                Codedata data = JsonUtility.FromJson<Codedata>(args.Value);
                if (data.code == 400)
                {
                    GameStart.Instance.ShowTip(data.msg);
                    currentItem.GetComponent<ItemScript>().labelText.text = OldName;
                }
                else if (data.code == 200)
                {
                    GameStart.Instance.ShowTip("添加分组成功");
                }


            }




        }



        void OnTreeListDeleteOneItem(TreeList treeList)
        {
            int count = treeList.ItemCount;
            TreeViewItem parentTreeItem = treeList.ParentTreeItem;
            if (count == 0 && parentTreeItem != null)
            {
                ItemScript st = parentTreeItem.GetComponent<ItemScript>();
                st.SetExpandBtnVisible(false);
            }
        }

        TreeViewItem CurSelectedItem
        {
            get
            {
                if (mCurSelectedItemId <= 0)
                {
                    return null;
                }
                TreeViewItem item = mTreeView.GetTreeItemById(mCurSelectedItemId);
                if (item == null)
                {
                    mCurSelectedItemId = 0;
                    return null;
                }
                return item;
            }
        }

        public void OnExpandAllBtnClicked()
        {
            mTreeView.ExpandAllItem();
        }
        public void OnCollapseAllBtnClicked()
        {
            mTreeView.CollapseAllItem();
        }

        public void OnExpandBtnClicked()
        {
            TreeViewItem item = CurSelectedItem;
            if (item == null)
            {
                 Log.Debug("Please Select a Item First");
                return;
            }
            item.Expand();
        }

        public void OnCollapseBtnClicked()
        {
            TreeViewItem item = CurSelectedItem;
            if (item == null)
            {
                 Log.Debug("Please Select a Item First");
                return;
            }
            item.Collapse();
        }


        #region
        public void OnInsertBeforeBtnClicked()
        {
            mNewItemCount++;
            if (mTreeView.IsEmpty)
            {
                TreeViewItem childItem = mTreeView.InsertItem(0, "ItemPrefab1");
                childItem.GetComponent<ItemScript>().SetItemInfo("Movie", "Movie" + mNewItemCount);
            }
            else
            {
                TreeViewItem item = CurSelectedItem;
                if (item == null)
                {
                     Log.Debug("Please Select a Item First");
                    return;
                }
                TreeViewItem childItem = item.ParentTreeList.InsertItem(item.ItemIndex, "ItemPrefab1");
                childItem.GetComponent<ItemScript>().SetItemInfo("Movie", "Movie" + mNewItemCount);
            }

        }

        public void OnInsertAfterBtnClicked()
        {
            mNewItemCount++;
            if (mTreeView.IsEmpty)
            {
                TreeViewItem childItem = mTreeView.InsertItem(0, "ItemPrefab1");
                childItem.GetComponent<ItemScript>().SetItemInfo("Movie", "Movie" + mNewItemCount);
            }
            else
            {
                TreeViewItem item = CurSelectedItem;
                if (item == null)
                {
                     Log.Debug("Please Select a Item First");
                    return;
                }
                TreeViewItem childItem = item.ParentTreeList.InsertItem(item.ItemIndex + 1, "ItemPrefab1");
                childItem.GetComponent<ItemScript>().SetItemInfo("Movie", "Movie" + mNewItemCount);
            }

        }

        #endregion



        public void OnAddCameraChildBtnClicked()
        {
            mNewItemCount++;
            TreeViewItem childItem = new TreeViewItem();
            if (mTreeView.IsEmpty)
            {
                childItem = mTreeView.AppendItem("ItemPrefab1");
                childItem.GetComponent<ItemScript>().SetItemInfo("新建大纲", "新建大纲" + mNewItemCount);
            }
            else
            {
                TreeViewItem item = CurSelectedItem;
                if (item == null)
                {
                     Log.Debug("Please Select a Item First");
                    return;
                }
                childItem = item.ChildTree.AppendItem("ItemPrefab1");
                childItem.GetComponent<ItemScript>().parentId = item.GetComponent<ItemScript>().id;
                childItem.GetComponent<ItemScript>().labelText.text = "新建大纲";
                childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());

            }
            OnItemCustomEvent(childItem, CustomEvent.ItemClicked, "");
            childItem.GetComponent<ItemScript>().labelText.interactable = true;
            childItem.GetComponent<ItemScript>().labelText.Select();
            currentItem = childItem;

            childItem.GetComponent<ItemScript>().labelText.onEndEdit.AddListener((txt) =>
            {

                Log.Debug("走的添加区域");
                BuildGroup element = new BuildGroup();
                element.areaName = txt;
                if (childItem.GetComponent<ItemScript>().parentId != null)
                    element.parentId = int.Parse(childItem.GetComponent<ItemScript>().parentId);
                else
                    element.parentId = 0;
                element.areaType = 2;
                HttpNetManager.GetInstance().SendDataObj("http://" +GameStart.IP  + "/api/personnel/tg/area/info", AddCameraAreaCallback, true, true, element);
            });



        }
        public void OnAddDoorChildBtnClicked()
        {
            mNewItemCount++;
            TreeViewItem childItem = new TreeViewItem();
            if (mTreeView.IsEmpty)
            {
                childItem = mTreeView.AppendItem("ItemPrefab1");
                childItem.GetComponent<ItemScript>().SetItemInfo("新建大纲", "新建大纲" + mNewItemCount);
            }
            else
            {
                TreeViewItem item = CurSelectedItem;
                if (item == null)
                {
                     Log.Debug("Please Select a Item First");
                    return;
                }
                childItem = item.ChildTree.AppendItem("ItemPrefab1");
                childItem.GetComponent<ItemScript>().parentId = item.GetComponent<ItemScript>().id;
                childItem.GetComponent<ItemScript>().labelText.text = "新建大纲";
                childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());

            }
            OnItemCustomEvent(childItem, CustomEvent.ItemClicked, "");
            childItem.GetComponent<ItemScript>().labelText.interactable = true;
            childItem.GetComponent<ItemScript>().labelText.Select();
            currentItem = childItem;

            childItem.GetComponent<ItemScript>().labelText.onEndEdit.AddListener((txt) =>
            {

                Log.Debug("走的添加区域");
                BuildGroup element = new BuildGroup();
                element.areaName = txt;
                if (childItem.GetComponent<ItemScript>().parentId != null)
                    element.parentId = int.Parse(childItem.GetComponent<ItemScript>().parentId);
                else
                    element.parentId = 0;
                element.areaType = 1;
                HttpNetManager.GetInstance().SendDataObj("http://" +GameStart.IP  + "/api/personnel/tg/area/info", AddDoorAreaCallback, true, true, element);
            });



        }
        //新建分组数据结构
        [Serializable]
        public class BuildGroup
        {

            public int id;//区域id
            public string areaName = string.Empty;//区域名字
            public string remark = string.Empty;
            public int areaType;
            public int parentId;//父级id
            public string level = string.Empty;
            public List<Element> children = new List<Element>();//子元素


        }

        public void OnDeleteCameraBtnClicked()
        {
            TreeViewItem item = CurSelectedItem;
            if (item == null)
            {
                 Log.Debug("Please Select a Item First");
                return;
            }
            List<string> outlineIds = new List<string>();
            outlineIds.Add(currentItemId);
            item.ParentTreeList.DeleteItem(item);

            ////****************删除数据库数据*******************
            BuildGroup element = new BuildGroup();
            element.areaName = item.GetComponent<ItemScript>().name;
            element.id = int.Parse(item.GetComponent<ItemScript>().id);
            element.parentId = int.Parse(item.GetComponent<ItemScript>().parentId);
            element.areaType = 2;
            HttpNetManager.GetInstance().SendDataObj("http://" +GameStart.IP  + "/api/personnel/tg/del/area/info", DelCameraAreaCallback, true, true, element);
        }

        public void OnDeleteDoorBtnClicked()
        {
            TreeViewItem item = CurSelectedItem;
            if (item == null)
            {
                 Log.Debug("Please Select a Item First");
                return;
            }
            List<string> outlineIds = new List<string>();
            outlineIds.Add(currentItemId);
            item.ParentTreeList.DeleteItem(item);

            ////****************删除数据库数据*******************
            BuildGroup element = new BuildGroup();
            element.areaName = item.GetComponent<ItemScript>().name;
            element.id = int.Parse(item.GetComponent<ItemScript>().id);
            element.parentId = int.Parse(item.GetComponent<ItemScript>().parentId);
            element.areaType = 2;
            HttpNetManager.GetInstance().SendDataObj("http://" +GameStart.IP  + "/api/personnel/tg/del/area/info", DelCameraAreaCallback, true, true, element);
        }

        public void DelCameraAreaCallback(HttpCallBackArgs args)
        {

            Log.Debug("收到删除监控分组回传信息：" + args.Value);

            if (!string.IsNullOrEmpty(args.Value))
            {
                Codedata data = JsonUtility.FromJson<Codedata>(args.Value);
                if (data.code == 400)
                {
                    GameStart.Instance.ShowTip(data.msg);

                }
                else if (data.code == 200)
                {
                    GameStart.Instance.ShowTip("删除分组成功");
                }


            }




        }
        //外部调用删除
        public void OnDeleteBtnClicked(TreeViewItem item)
        {

            //Log.Error("删除分组");
            if (item == null)
            {
                Log.Debug("Please Select a Item First");
                return;
            }
            List<string> outlineIds = new List<string>();
            outlineIds.Add(currentItemId);
            item.ParentTreeList.DeleteItem(item);


        }


        public void OnBackBtnClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        //新建编辑或者改名
        public string OldName = string.Empty;
        public void OnCameraEditBtnClicked()
        {
            TreeViewItem item = CurSelectedItem;
            if (item == null)
            {
                 Log.Debug("Please Select a Item First");
                return;
            }
            item.GetComponent<ItemScript>().labelText.interactable = true;
            item.GetComponent<ItemScript>().labelText.Select();
            currentItem = item;
            OldName = currentItem.GetComponent<ItemScript>().labelText.text;
            item.GetComponent<ItemScript>().labelText.onEndEdit.AddListener((txt) =>
            {

                Log.Debug("走的区域改名");
                BuildGroup element = new BuildGroup();
                element.areaName = txt;
                element.parentId = int.Parse(item.GetComponent<ItemScript>().parentId);
                element.areaType = 2;
                element.id = int.Parse(item.GetComponent<ItemScript>().id);
                HttpNetManager.GetInstance().SendDataObj("http://" +GameStart.IP  + "/api/personnel/tg/area/name", EditorCameraAreaCallback, true, true, element);
            });

        }
        public void OnDoorEditBtnClicked()
        {
            TreeViewItem item = CurSelectedItem;
            if (item == null)
            {
                 Log.Debug("Please Select a Item First");
                return;
            }
            item.GetComponent<ItemScript>().labelText.interactable = true;
            item.GetComponent<ItemScript>().labelText.Select();
            currentItem = item;
            OldName = currentItem.GetComponent<ItemScript>().labelText.text;
            item.GetComponent<ItemScript>().labelText.onEndEdit.AddListener((txt) =>
            {

                Log.Debug("走的区域改名");
                BuildGroup element = new BuildGroup();
                element.areaName = txt;
                element.parentId = int.Parse(item.GetComponent<ItemScript>().parentId);
                element.areaType = 1;
                element.id = int.Parse(item.GetComponent<ItemScript>().id);
                HttpNetManager.GetInstance().SendDataObj("http://" +GameStart.IP  + "/api/personnel/tg/area/name", EditorCameraAreaCallback, true, true, element);
            });

        }
    }
}