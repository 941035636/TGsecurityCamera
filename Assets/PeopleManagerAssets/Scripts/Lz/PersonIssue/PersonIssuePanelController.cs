using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using zFramework.Media;
using static ManManagentUi;

/// <summary>
/// 人员管理中的自定义门禁下发面板。
/// 只消费服务端既有查询接口，任务选择不会修改人员类型与设备的服务端绑定关系。
/// </summary>
public class PersonIssuePanelController : MonoBehaviour
{
    private const int PageSize = 20;
    private readonly Dictionary<string, NVRInformation> devices = new Dictionary<string, NVRInformation>();
    private readonly Dictionary<string, Toggle> deviceToggles = new Dictionary<string, Toggle>();
    private readonly Dictionary<string, GameObject> deviceRows = new Dictionary<string, GameObject>();
    private readonly List<GroupToggleBinding> groupBindings = new List<GroupToggleBinding>();
    private readonly List<UserType> types = new List<UserType>();

    private GameObject panel;
    private RectTransform deviceContent;
    private Text typeText;
    private Text selectionText;
    private Text statusText;
    private Text resultText;
    private Slider progress;
    private InputField personFilterInput;
    private InputField deviceSearchInput;
    private InputField beginTimeInput;
    private InputField endTimeInput;
    private Toggle sendUserToggle;
    private Toggle sendCardToggle;
    private Toggle sendFaceToggle;
    private Button startButton;
    private Button cancelButton;
    private int typeIndex;
    private bool suppressToggleEvents;
    private bool isRunning;
    private bool cancelRequested;
    private int successCount;
    private int failureCount;
    private Font uiFont;

    private class GroupToggleBinding
    {
        public Toggle Toggle;
        public Text Label;
        public string Name;
        public List<string> DeviceKeys = new List<string>();
    }

    private class PersonPageResult
    {
        public UserInfoArrPeople Page;
        public string Error;
    }

    public static PersonIssuePanelController Ensure(PeopleController owner)
    {
        PersonIssuePanelController controller = owner.GetComponent<PersonIssuePanelController>();
        if (controller == null)
        {
            controller = owner.gameObject.AddComponent<PersonIssuePanelController>();
        }
        return controller;
    }

    public void Show()
    {
        if (isRunning)
        {
            panel.SetActive(true);
            return;
        }

        EnsureView();
        panel.SetActive(true);
        RequestTypes();
        RequestDevices();
    }

    private void EnsureView()
    {
        if (panel != null) return;

        PeopleController owner = GetComponent<PeopleController>();
        uiFont = owner != null && owner.NumPage != null && owner.NumPage.font != null
            ? owner.NumPage.font
            : Resources.GetBuiltinResource<Font>("Arial.ttf");
        Canvas canvas = GetComponentInParent<Canvas>();
        Transform parent = canvas != null ? canvas.transform : transform.root;

        panel = CreatePanel(parent, "PersonIssuePanel", new Color(0f, 0f, 0f, 0.72f));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        Stretch(panelRect, Vector2.zero, Vector2.zero);
        panel.transform.SetAsLastSibling();

        GameObject window = CreatePanel(panel.transform, "Window", new Color(0.075f, 0.14f, 0.20f, 1f));
        RectTransform windowRect = window.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.pivot = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(980f, 680f);

        CreateText(window.transform, "Title", "自定义人员下发", 22, TextAnchor.MiddleLeft,
            new Vector2(20f, -55f), new Vector2(780f, -10f));
        Button close = CreateButton(window.transform, "Close", "×", new Vector2(915f, -52f), new Vector2(960f, -10f));
        close.onClick.AddListener(Hide);

        CreateText(window.transform, "TypeLabel", "人员类型", 15, TextAnchor.MiddleLeft,
            new Vector2(20f, -105f), new Vector2(95f, -65f));
        Button previousType = CreateButton(window.transform, "PreviousType", "<", new Vector2(100f, -103f), new Vector2(138f, -68f));
        typeText = CreateText(window.transform, "SelectedType", "正在读取人员类型...", 15, TextAnchor.MiddleCenter,
            new Vector2(143f, -103f), new Vector2(343f, -68f));
        Button nextType = CreateButton(window.transform, "NextType", ">", new Vector2(348f, -103f), new Vector2(386f, -68f));
        previousType.onClick.AddListener(delegate { ChangeType(-1); });
        nextType.onClick.AddListener(delegate { ChangeType(1); });

        CreateText(window.transform, "PersonFilterLabel", "指定人员", 15, TextAnchor.MiddleLeft,
            new Vector2(405f, -105f), new Vector2(480f, -65f));
        personFilterInput = CreateInput(window.transform, "PersonFilter", "身份证号，多个用逗号分隔；留空表示该类型全部人员",
            new Vector2(485f, -103f), new Vector2(960f, -68f));

        CreateText(window.transform, "DeviceTitle", "选择门禁设备", 16, TextAnchor.MiddleLeft,
            new Vector2(20f, -150f), new Vector2(175f, -112f));
        deviceSearchInput = CreateInput(window.transform, "DeviceSearch", "按设备名称或IP筛选",
            new Vector2(175f, -147f), new Vector2(405f, -115f));
        deviceSearchInput.onValueChanged.AddListener(FilterDevices);
        Button selectAll = CreateButton(window.transform, "SelectAll", "全选", new Vector2(415f, -147f), new Vector2(480f, -115f));
        Button clearAll = CreateButton(window.transform, "ClearAll", "清空", new Vector2(488f, -147f), new Vector2(553f, -115f));
        selectAll.onClick.AddListener(delegate { SetAllDevices(true); });
        clearAll.onClick.AddListener(delegate { SetAllDevices(false); });
        selectionText = CreateText(window.transform, "Selection", "已选 0 台", 14, TextAnchor.MiddleRight,
            new Vector2(700f, -150f), new Vector2(960f, -112f));

        ScrollRect deviceScroll = CreateScroll(window.transform, "DeviceScroll",
            new Vector2(20f, -425f), new Vector2(480f, -155f), out deviceContent);
        deviceScroll.vertical = true;

        GameObject optionPanel = CreatePanel(window.transform, "Options", new Color(0.055f, 0.105f, 0.15f, 1f));
        RectTransform optionRect = optionPanel.GetComponent<RectTransform>();
        SetOffsets(optionRect, new Vector2(500f, -425f), new Vector2(960f, -155f));
        CreateText(optionPanel.transform, "OptionTitle", "下发设置", 16, TextAnchor.MiddleLeft,
            new Vector2(15f, -42f), new Vector2(440f, -8f));
        sendUserToggle = CreateToggle(optionPanel.transform, "SendUser", "人员基础信息", new Vector2(15f, -80f), new Vector2(145f, -48f));
        sendCardToggle = CreateToggle(optionPanel.transform, "SendCard", "卡号", new Vector2(155f, -80f), new Vector2(260f, -48f));
        sendFaceToggle = CreateToggle(optionPanel.transform, "SendFace", "人脸", new Vector2(270f, -80f), new Vector2(375f, -48f));
        sendUserToggle.isOn = true;
        sendCardToggle.isOn = true;
        sendFaceToggle.isOn = true;
        CreateText(optionPanel.transform, "BeginLabel", "有效期开始", 14, TextAnchor.MiddleLeft,
            new Vector2(15f, -125f), new Vector2(105f, -92f));
        beginTimeInput = CreateInput(optionPanel.transform, "BeginTime", "yyyy-MM-ddTHH:mm:ss",
            new Vector2(110f, -125f), new Vector2(430f, -92f));
        beginTimeInput.text = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        CreateText(optionPanel.transform, "EndLabel", "有效期结束", 14, TextAnchor.MiddleLeft,
            new Vector2(15f, -170f), new Vector2(105f, -137f));
        endTimeInput = CreateInput(optionPanel.transform, "EndTime", "yyyy-MM-ddTHH:mm:ss",
            new Vector2(110f, -170f), new Vector2(430f, -137f));
        endTimeInput.text = "2036-06-11T16:05:00";
        CreateText(optionPanel.transform, "Safety", "任务启动后会锁定本次人员类型和设备选择，不会修改服务端权限配置。", 13,
            TextAnchor.UpperLeft, new Vector2(15f, -250f), new Vector2(440f, -185f));

        progress = CreateSlider(window.transform, "Progress", new Vector2(20f, -466f), new Vector2(960f, -440f));
        statusText = CreateText(window.transform, "Status", "请选择人员类型和门禁设备", 14, TextAnchor.MiddleLeft,
            new Vector2(20f, -505f), new Vector2(960f, -472f));

        RectTransform resultContent;
        CreateScroll(window.transform, "ResultScroll", new Vector2(20f, -610f), new Vector2(960f, -510f), out resultContent);
        resultText = CreateText(resultContent, "Results", "", 13, TextAnchor.UpperLeft,
            Vector2.zero, new Vector2(0f, 0f));
        RectTransform resultRect = resultText.rectTransform;
        resultRect.anchorMin = new Vector2(0f, 1f);
        resultRect.anchorMax = new Vector2(1f, 1f);
        resultRect.pivot = new Vector2(0.5f, 1f);
        resultRect.sizeDelta = new Vector2(0f, 100f);
        ContentSizeFitter resultFitter = resultText.gameObject.AddComponent<ContentSizeFitter>();
        resultFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        startButton = CreateButton(window.transform, "Start", "开始下发", new Vector2(710f, -660f), new Vector2(825f, -620f));
        cancelButton = CreateButton(window.transform, "Cancel", "取消任务", new Vector2(835f, -660f), new Vector2(960f, -620f));
        startButton.onClick.AddListener(StartIssue);
        cancelButton.onClick.AddListener(CancelIssue);
        cancelButton.interactable = false;
    }

    private void Hide()
    {
        if (isRunning)
        {
            statusText.text = "任务正在执行。如需关闭，请先点击取消任务。";
            return;
        }
        panel.SetActive(false);
    }

    private void RequestTypes()
    {
        types.Clear();
        typeIndex = 0;
        typeText.text = "正在从服务端读取人员类型...";
        string url = GameStart.ApiUrl("/api/personnel/tg/user/type");
        HttpNetManager.GetInstance().SendDataStr(url, OnTypesReceived, false, false, true);
    }

    private void OnTypesReceived(HttpCallBackArgs args)
    {
        if (args == null || args.HasError || string.IsNullOrEmpty(args.Value))
        {
            typeText.text = "人员类型读取失败";
            AppendResult("[人员类型接口失败] " + (args == null ? "无响应" : args.ErrorValue));
            return;
        }

        try
        {
            List<UserType> serverTypes = JsonConvert.DeserializeObject<List<UserType>>(args.Value);
            if (serverTypes != null)
            {
                // 访客使用独立的访客管理页面，其余类型全部按服务端原始ID和名称展示。
                types.AddRange(serverTypes.Where(t => t != null
                    && (string.IsNullOrEmpty(t.typeName) || !t.typeName.Contains("访客"))));
            }
            TypeMenuController.typeArr = serverTypes ?? new List<UserType>();

            int currentType = TypeMenuController.Ins == null ? -1 : TypeMenuController.Ins.ChooseType;
            int currentIndex = types.FindIndex(t => t.typeId == currentType);
            typeIndex = currentIndex >= 0 ? currentIndex : 0;
            UpdateTypeText();
            if (types.Count == 0)
            {
                AppendResult("[人员类型接口] 服务端未返回可用的非访客人员类型");
            }
        }
        catch (Exception e)
        {
            types.Clear();
            typeText.text = "人员类型数据解析失败";
            AppendResult("[人员类型解析失败] " + e.Message);
            Log.Error("自定义人员下发解析人员类型失败:" + e);
        }
    }

    private void ChangeType(int direction)
    {
        if (isRunning || types.Count == 0) return;
        typeIndex = (typeIndex + direction + types.Count) % types.Count;
        UpdateTypeText();
    }

    private int GetSelectedTypeId()
    {
        if (types.Count > 0 && typeIndex >= 0 && typeIndex < types.Count) return types[typeIndex].typeId;
        return -1;
    }

    private void UpdateTypeText()
    {
        typeText.text = types.Count == 0
            ? "没有可用人员类型"
            : types[typeIndex].typeName + "  (ID:" + types[typeIndex].typeId + ")";
    }

    private void RequestDevices()
    {
        ClearDeviceRows();
        statusText.text = "正在读取门禁设备...";
        string url = GameStart.ApiUrl("/api/personnel/tg/area/info?pageNum=1&pageSize=10000&areaId=&areaName=&areaType=1");
        HttpNetManager.GetInstance().SendDataStr(url, OnDevicesReceived, false, false, false);
    }

    private void OnDevicesReceived(HttpCallBackArgs args)
    {
        if (args == null || args.HasError || string.IsNullOrEmpty(args.Value))
        {
            statusText.text = "门禁设备读取失败：" + (args == null ? "无响应" : args.ErrorValue);
            return;
        }

        try
        {
            AreaDevsData response = JsonUtility.FromJson<AreaDevsData>(args.Value);
            List<GetAreaGroupDevs> records = response == null ? null : response.records;
            if (records == null)
            {
                statusText.text = "门禁设备数据为空";
                return;
            }

            HashSet<int> visitedAreas = new HashSet<int>();
            List<GetAreaGroupDevs> roots = records.Where(a => a != null && (a.parentId == 0 || !records.Exists(p => p != null && p.id == a.parentId))).ToList();
            if (roots.Count == 0) roots = records;
            foreach (GetAreaGroupDevs root in roots)
            {
                AddArea(root, 0, visitedAreas);
            }
            statusText.text = devices.Count == 0 ? "没有找到门禁设备" : "已读取 " + devices.Count + " 台门禁设备";
            UpdateSelectionState();
        }
        catch (Exception e)
        {
            statusText.text = "门禁设备数据解析失败：" + e.Message;
            Log.Error("自定义人员下发解析门禁树失败:" + e);
        }
    }

    private List<string> AddArea(GetAreaGroupDevs area, int depth, HashSet<int> visitedAreas)
    {
        List<string> descendantKeys = new List<string>();
        if (area == null || (area.id != 0 && !visitedAreas.Add(area.id))) return descendantKeys;

        GroupToggleBinding binding = new GroupToggleBinding();
        binding.Name = string.IsNullOrEmpty(area.areaName) ? "未命名区域" : area.areaName;
        binding.Toggle = AddTreeToggle(binding.Name, depth, true, null);
        binding.Label = binding.Toggle.GetComponentInChildren<Text>();
        groupBindings.Add(binding);

        if (area.devList != null)
        {
            foreach (NVRInformation device in area.devList)
            {
                string key = DeviceKey(device);
                if (devices.ContainsKey(key)) continue;
                devices.Add(key, device);
                descendantKeys.Add(key);
                Toggle toggle = AddTreeToggle(DeviceDisplayName(device), depth + 1, false, key);
                deviceToggles.Add(key, toggle);
                deviceRows.Add(key, toggle.gameObject);
            }
        }

        if (area.children != null)
        {
            foreach (GetAreaGroupDevs child in area.children)
            {
                descendantKeys.AddRange(AddArea(child, depth + 1, visitedAreas));
            }
        }

        binding.DeviceKeys = descendantKeys.Distinct().ToList();
        binding.Toggle.onValueChanged.AddListener(delegate(bool selected)
        {
            if (suppressToggleEvents) return;
            suppressToggleEvents = true;
            foreach (string key in binding.DeviceKeys)
            {
                Toggle deviceToggle;
                if (deviceToggles.TryGetValue(key, out deviceToggle)) deviceToggle.isOn = selected;
            }
            suppressToggleEvents = false;
            UpdateSelectionState();
        });
        return descendantKeys;
    }

    private Toggle AddTreeToggle(string label, int depth, bool isGroup, string deviceKey)
    {
        GameObject row = new GameObject((isGroup ? "Area_" : "Device_") + label, typeof(RectTransform), typeof(LayoutElement));
        row.transform.SetParent(deviceContent, false);
        LayoutElement layout = row.GetComponent<LayoutElement>();
        layout.preferredHeight = 31f;
        Toggle toggle = CreateToggle(row.transform, "Toggle", label,
            new Vector2(10f + depth * 20f, -30f), new Vector2(440f, 0f));
        RectTransform toggleRect = toggle.GetComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(0f, 1f);
        toggleRect.anchorMax = new Vector2(0f, 1f);
        toggleRect.pivot = new Vector2(0f, 1f);
        if (!isGroup)
        {
            toggle.onValueChanged.AddListener(delegate { if (!suppressToggleEvents) UpdateSelectionState(); });
        }
        else
        {
            Text text = toggle.GetComponentInChildren<Text>();
            text.fontStyle = FontStyle.Bold;
        }
        return toggle;
    }

    private void SetAllDevices(bool selected)
    {
        if (isRunning) return;
        suppressToggleEvents = true;
        foreach (Toggle toggle in deviceToggles.Values) toggle.isOn = selected;
        suppressToggleEvents = false;
        UpdateSelectionState();
    }

    private void UpdateSelectionState()
    {
        int count = deviceToggles.Count(p => p.Value.isOn);
        selectionText.text = "已选 " + count + " / " + devices.Count + " 台";
        suppressToggleEvents = true;
        foreach (GroupToggleBinding group in groupBindings)
        {
            int selected = group.DeviceKeys.Count(k => deviceToggles.ContainsKey(k) && deviceToggles[k].isOn);
            group.Toggle.isOn = group.DeviceKeys.Count > 0 && selected == group.DeviceKeys.Count;
            group.Label.text = group.Name + "  [" + selected + "/" + group.DeviceKeys.Count + "]";
        }
        suppressToggleEvents = false;
    }

    private void FilterDevices(string filter)
    {
        string normalized = (filter ?? string.Empty).Trim();
        foreach (KeyValuePair<string, GameObject> pair in deviceRows)
        {
            NVRInformation device = devices[pair.Key];
            bool visible = string.IsNullOrEmpty(normalized)
                || DeviceDisplayName(device).IndexOf(normalized, StringComparison.OrdinalIgnoreCase) >= 0
                || device.Ip.IndexOf(normalized, StringComparison.OrdinalIgnoreCase) >= 0;
            pair.Value.SetActive(visible);
        }
    }

    private void ClearDeviceRows()
    {
        devices.Clear();
        deviceToggles.Clear();
        deviceRows.Clear();
        groupBindings.Clear();
        for (int i = deviceContent.childCount - 1; i >= 0; i--) Destroy(deviceContent.GetChild(i).gameObject);
        UpdateSelectionState();
    }

    private async void StartIssue()
    {
        if (isRunning) return;
        int typeId = GetSelectedTypeId();
        List<NVRInformation> selectedDevices = deviceToggles.Where(p => p.Value.isOn).Select(p => devices[p.Key]).ToList();
        if (typeId < 0)
        {
            statusText.text = "请选择人员类型";
            return;
        }
        if (selectedDevices.Count == 0)
        {
            statusText.text = "请至少选择一台门禁设备";
            return;
        }
        if (!sendUserToggle.isOn && !sendCardToggle.isOn && !sendFaceToggle.isOn)
        {
            statusText.text = "请至少选择一项下发内容";
            return;
        }
        DateTime begin;
        DateTime end;
        if (!DateTime.TryParse(beginTimeInput.text, out begin) || !DateTime.TryParse(endTimeInput.text, out end) || end <= begin)
        {
            statusText.text = "有效期格式错误，或结束时间早于开始时间";
            return;
        }

        isRunning = true;
        cancelRequested = false;
        successCount = 0;
        failureCount = 0;
        startButton.interactable = false;
        cancelButton.interactable = true;
        SetControlsInteractable(false);
        resultText.text = string.Empty;
        progress.value = 0f;

        try
        {
            statusText.text = "正在读取待下发人员...";
            List<UserInfo> people = await LoadPeopleAsync(typeId, ParsePersonFilter(personFilterInput.text));
            if (cancelRequested) return;
            if (people.Count == 0)
            {
                statusText.text = "当前条件下没有可下发人员";
                return;
            }

            int totalSteps = people.Count * selectedDevices.Count;
            int completedSteps = 0;
            progress.maxValue = totalSteps;
            HKPerson issuer = HKPerson.Instance;
            string beginText = begin.ToString("yyyy-MM-ddTHH:mm:ss");
            string endText = end.ToString("yyyy-MM-ddTHH:mm:ss");

            foreach (NVRInformation device in selectedDevices)
            {
                if (cancelRequested) break;
                string deviceName = DeviceDisplayName(device);
                statusText.text = "正在登录门禁：" + deviceName;
                await NVRManager.LoginAsync(device, false);

                int loginHandle;
                if (!HikvisonNVR.LoginhandleDic.TryGetValue(device.Ip, out loginHandle) || loginHandle < 0)
                {
                    failureCount += people.Count;
                    completedSteps += people.Count;
                    AppendResult("[设备失败] " + deviceName + " 登录失败，跳过 " + people.Count + " 人");
                    progress.value = completedSteps;
                    continue;
                }

                int userHandle = -1;
                int cardHandle = -1;
                int faceHandle = -1;
                try
                {
                    if (sendUserToggle.isOn) userHandle = issuer.OpenDeviceConnect(loginHandle, 0);
                    if (sendCardToggle.isOn) cardHandle = issuer.OpenDeviceConnect(loginHandle, 1);
                    if (sendFaceToggle.isOn) faceHandle = issuer.OpenDeviceConnect(loginHandle, 2);

                    foreach (UserInfo person in people)
                    {
                        if (cancelRequested) break;
                        string reason = await IssuePersonAsync(issuer, person, userHandle, cardHandle, faceHandle, beginText, endText);
                        completedSteps++;
                        progress.value = completedSteps;
                        if (string.IsNullOrEmpty(reason))
                        {
                            successCount++;
                            AppendResult("[成功] " + deviceName + " / " + person.username + " / " + person.idNum);
                        }
                        else
                        {
                            failureCount++;
                            AppendResult("[失败] " + deviceName + " / " + person.username + " / " + person.idNum + "：" + reason);
                        }
                        statusText.text = "正在下发 " + completedSteps + "/" + totalSteps + "，成功 " + successCount + "，失败 " + failureCount;
                        await Task.Yield();
                    }
                }
                finally
                {
                    StopRemoteConfig(userHandle);
                    StopRemoteConfig(cardHandle);
                    StopRemoteConfig(faceHandle);
                    await NVRManager.LogoutAsync(device.Ip);
                    HikvisonNVR.LoginhandleDic.Remove(device.Ip);
                }
            }

            statusText.text = cancelRequested
                ? "任务已取消。成功 " + successCount + "，失败 " + failureCount
                : "下发完成。成功 " + successCount + "，失败 " + failureCount;
        }
        catch (Exception e)
        {
            failureCount++;
            statusText.text = "任务异常终止：" + e.Message;
            AppendResult("[任务异常] " + e);
            Log.Error("自定义人员下发任务异常:" + e);
        }
        finally
        {
            isRunning = false;
            startButton.interactable = true;
            cancelButton.interactable = false;
            SetControlsInteractable(true);
        }
    }

    private async Task<string> IssuePersonAsync(HKPerson issuer, UserInfo person, int userHandle, int cardHandle,
        int faceHandle, string begin, string end)
    {
        if (person == null || string.IsNullOrEmpty(person.idNum)) return "身份证号为空";
        List<string> errors = new List<string>();

        if (sendFaceToggle.isOn)
        {
            string faceError = SaveFace(person);
            if (!string.IsNullOrEmpty(faceError)) errors.Add(faceError);
        }

        if (sendUserToggle.isOn)
        {
            string error = await ExecuteStepAsync("人员", delegate
            {
                return issuer.AddUserWithResult(person.idNum, person.username, "normal", begin, end, userHandle);
            });
            if (!string.IsNullOrEmpty(error)) errors.Add(error);
        }
        if (sendCardToggle.isOn)
        {
            string error = await ExecuteStepAsync("卡号", delegate
            {
                return issuer.AddCardWithResult(person.idNum, person.idNum, cardHandle);
            });
            if (!string.IsNullOrEmpty(error)) errors.Add(error);
        }
        if (sendFaceToggle.isOn && errors.All(e => !e.StartsWith("人脸图片")))
        {
            string error = await ExecuteStepAsync("人脸", delegate
            {
                return issuer.AddFaceWithResult(person.idNum, faceHandle);
            });
            if (!string.IsNullOrEmpty(error)) errors.Add(error);
        }
        return string.Join("；", errors.ToArray());
    }

    private async Task<string> ExecuteStepAsync(string step, Func<HikvisionIssueStepResult> action)
    {
        try
        {
            HikvisionIssueStepResult result = await Task.Run(action);
            return result != null && result.Success ? null : FormatStepError(step,
                result ?? HikvisionIssueStepResult.Fail("没有返回结果"));
        }
        catch (Exception e)
        {
            return step + "异常：" + e.Message;
        }
    }

    private string SaveFace(UserInfo person)
    {
        if (string.IsNullOrEmpty(person.faceBase64)) return "人脸图片为空";
        try
        {
            string base64 = person.faceBase64;
            int comma = base64.IndexOf(',');
            if (comma >= 0) base64 = base64.Substring(comma + 1);
            byte[] bytes = Convert.FromBase64String(base64);
            if (bytes.Length == 0) return "人脸图片为空";
            if (bytes.Length > 200 * 1024) return "人脸图片超过设备限制的200KB";
            string directory = @"C:\facepicture";
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(Path.Combine(directory, SafeFileName(person.idNum) + ".jpg"), bytes);
            return null;
        }
        catch (Exception e)
        {
            return "人脸图片保存失败：" + e.Message;
        }
    }

    private async Task<List<UserInfo>> LoadPeopleAsync(int typeId, HashSet<string> filter)
    {
        List<UserInfo> result = new List<UserInfo>();
        int pageNumber = 1;
        while (!cancelRequested)
        {
            PersonPageResult response = await RequestPeoplePageAsync(typeId, pageNumber);
            if (!string.IsNullOrEmpty(response.Error)) throw new InvalidOperationException(response.Error);
            UserInfoArrPeople page = response.Page;
            if (page == null || page.records == null || page.records.Count == 0) break;
            foreach (UserInfo person in page.records)
            {
                if (filter.Count == 0 || filter.Contains(person.idNum)) result.Add(person);
            }
            statusText.text = "正在读取人员：第 " + pageNumber + "/" + Math.Max(page.pages, 1) + " 页，已选 " + result.Count + " 人";
            if (pageNumber >= page.pages) break;
            pageNumber++;
        }
        return result;
    }

    private Task<PersonPageResult> RequestPeoplePageAsync(int typeId, int pageNumber)
    {
        TaskCompletionSource<PersonPageResult> source = new TaskCompletionSource<PersonPageResult>();
        string url = GameStart.ApiUrl("/api/personnel/tg/user/client?")
            + "idNum=&pageNum=" + pageNumber + "&pageSize=" + PageSize + "&type=" + typeId + "&issued=-1";
        HttpNetManagerPeople.GetInstance().SendDataStr(url, delegate(HttpCallBackArgsPeople args)
        {
            PersonPageResult result = new PersonPageResult();
            if (args == null || args.HasError)
            {
                result.Error = "人员信息请求失败：" + (args == null ? "无响应" : args.ErrorValue);
            }
            else
            {
                try
                {
                    result.Page = JsonUtility.FromJson<UserInfoArrPeople>(args.Value);
                }
                catch (Exception e)
                {
                    result.Error = "人员信息解析失败：" + e.Message;
                }
            }
            source.TrySetResult(result);
        }, null, false, false, true);
        return source.Task;
    }

    private void CancelIssue()
    {
        if (!isRunning) return;
        cancelRequested = true;
        cancelButton.interactable = false;
        statusText.text = "正在取消，将在当前人员处理完成后安全停止...";
    }

    private void StopRemoteConfig(int handle)
    {
        if (handle >= 0) CHCNetSDK.NET_DVR_StopRemoteConfig(handle);
    }

    private void AppendResult(string line)
    {
        const int maximumCharacters = 20000;
        string next = resultText.text + line + "\n";
        resultText.text = next.Length > maximumCharacters ? next.Substring(next.Length - maximumCharacters) : next;
    }

    private string FormatStepError(string step, HikvisionIssueStepResult result)
    {
        string codes = string.Empty;
        if (result.DeviceStatusCode != 0) codes += " 设备状态=" + result.DeviceStatusCode;
        if (result.SdkErrorCode != 0) codes += " SDK=" + result.SdkErrorCode;
        return step + "失败：" + result.Message + codes;
    }

    private HashSet<string> ParsePersonFilter(string value)
    {
        return new HashSet<string>((value ?? string.Empty)
            .Split(new[] { ',', '，', ';', '；', '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(v => v.Trim()), StringComparer.OrdinalIgnoreCase);
    }

    private void SetControlsInteractable(bool enabled)
    {
        personFilterInput.interactable = enabled;
        deviceSearchInput.interactable = enabled;
        beginTimeInput.interactable = enabled;
        endTimeInput.interactable = enabled;
        sendUserToggle.interactable = enabled;
        sendCardToggle.interactable = enabled;
        sendFaceToggle.interactable = enabled;
        foreach (Toggle toggle in deviceToggles.Values) toggle.interactable = enabled;
        foreach (GroupToggleBinding group in groupBindings) group.Toggle.interactable = enabled;
    }

    private string DeviceKey(NVRInformation device)
    {
        return device.id > 0 ? "id:" + device.id : "host:" + device.ActiveHost + ":" + device.channel;
    }

    private string DeviceDisplayName(NVRInformation device)
    {
        string name = string.IsNullOrEmpty(device.cameraname) ? device.description : device.cameraname;
        if (string.IsNullOrEmpty(name)) name = "未命名门禁";
        return name + "  (" + device.ActiveHost + ")";
    }

    private string SafeFileName(string value)
    {
        foreach (char invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
        return value;
    }

    private GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    private Text CreateText(Transform parent, string name, string value, int size, TextAnchor alignment, Vector2 min, Vector2 max)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        Text text = go.GetComponent<Text>();
        text.font = uiFont;
        text.fontSize = size;
        text.color = new Color(0.88f, 0.94f, 0.98f, 1f);
        text.alignment = alignment;
        text.text = value;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        SetOffsets(text.rectTransform, min, max);
        return text;
    }

    private Button CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max)
    {
        GameObject go = CreatePanel(parent, name, new Color(0.08f, 0.42f, 0.62f, 1f));
        SetOffsets(go.GetComponent<RectTransform>(), min, max);
        Button button = go.AddComponent<Button>();
        button.targetGraphic = go.GetComponent<Image>();
        Text text = CreateText(go.transform, "Text", label, 15, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
        Stretch(text.rectTransform, Vector2.zero, Vector2.zero);
        return button;
    }

    private InputField CreateInput(Transform parent, string name, string placeholder, Vector2 min, Vector2 max)
    {
        GameObject go = CreatePanel(parent, name, new Color(0.035f, 0.075f, 0.105f, 1f));
        SetOffsets(go.GetComponent<RectTransform>(), min, max);
        InputField input = go.AddComponent<InputField>();
        Text value = CreateText(go.transform, "Text", "", 14, TextAnchor.MiddleLeft, new Vector2(8f, 0f), new Vector2(-8f, 0f));
        Stretch(value.rectTransform, new Vector2(8f, 3f), new Vector2(-8f, -3f));
        Text hint = CreateText(go.transform, "Placeholder", placeholder, 13, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
        hint.color = new Color(0.48f, 0.58f, 0.64f, 1f);
        Stretch(hint.rectTransform, new Vector2(8f, 3f), new Vector2(-8f, -3f));
        input.textComponent = value;
        input.placeholder = hint;
        return input;
    }

    private Toggle CreateToggle(Transform parent, string name, string label, Vector2 min, Vector2 max)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Toggle));
        go.transform.SetParent(parent, false);
        SetOffsets(go.GetComponent<RectTransform>(), min, max);
        GameObject background = CreatePanel(go.transform, "Background", new Color(0.13f, 0.24f, 0.31f, 1f));
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0f, 0.5f);
        backgroundRect.sizeDelta = new Vector2(20f, 20f);
        backgroundRect.anchoredPosition = new Vector2(11f, 0f);
        GameObject check = CreatePanel(background.transform, "Checkmark", new Color(0.15f, 0.78f, 0.55f, 1f));
        Stretch(check.GetComponent<RectTransform>(), new Vector2(4f, 4f), new Vector2(-4f, -4f));
        Text text = CreateText(go.transform, "Label", label, 14, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
        Stretch(text.rectTransform, new Vector2(30f, 0f), Vector2.zero);
        Toggle toggle = go.GetComponent<Toggle>();
        toggle.targetGraphic = background.GetComponent<Image>();
        toggle.graphic = check.GetComponent<Image>();
        return toggle;
    }

    private ScrollRect CreateScroll(Transform parent, string name, Vector2 min, Vector2 max, out RectTransform content)
    {
        GameObject viewport = CreatePanel(parent, name, new Color(0.035f, 0.075f, 0.105f, 1f));
        SetOffsets(viewport.GetComponent<RectTransform>(), min, max);
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = true;
        GameObject contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObject.transform.SetParent(viewport.transform, false);
        content = contentObject.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.sizeDelta = Vector2.zero;
        VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        ScrollRect scroll = viewport.AddComponent<ScrollRect>();
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = content;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;
        return scroll;
    }

    private Slider CreateSlider(Transform parent, string name, Vector2 min, Vector2 max)
    {
        GameObject root = CreatePanel(parent, name, new Color(0.04f, 0.08f, 0.11f, 1f));
        SetOffsets(root.GetComponent<RectTransform>(), min, max);
        Slider slider = root.AddComponent<Slider>();
        GameObject fill = CreatePanel(root.transform, "Fill", new Color(0.10f, 0.70f, 0.62f, 1f));
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        Stretch(fillRect, new Vector2(2f, 2f), new Vector2(-2f, -2f));
        slider.fillRect = fillRect;
        slider.targetGraphic = fill.GetComponent<Image>();
        slider.interactable = false;
        return slider;
    }

    private void SetOffsets(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.offsetMin = min;
        rect.offsetMax = max;
    }

    private void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
