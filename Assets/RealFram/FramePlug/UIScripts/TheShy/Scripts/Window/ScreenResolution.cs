using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ScreenResolution : SingletonManager<ScreenResolution>
{

    //窗口样式
    [DllImport("user32.dll")]
    static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);
    //窗口分辨率和位置
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    //获取当前激活窗口
    [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
    static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string strClassName, int nptWindowName);
    //窗口拖动
    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();
    [DllImport("user32.dll")]
    public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
    [DllImport("user32.dll", SetLastError = true)]
    //得到窗口的样式
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(System.IntPtr hwnd, int nCmdShow);
    /// <summary>
    /// 调用Window的弹框
    /// </summary>
    [DllImport("User32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern int MessageBox(IntPtr handle, string message, string title, int type);
    //获取窗口位置以及大小
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

    /// <summary>
    /// 获取制定标题名字的窗口
    /// </summary>
    /// <param name="lpClassName"></param>
    /// <param name="lpWindowName"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left; //最左坐标
        public int Top; //最上坐标
        public int Right; //最右坐标
        public int Bottom; //最下坐标
    }

    public enum TitleBarStyles
    {
        Hide, Show
    }
    private const int SWP_SHOWWINDOW = 0x0040;
    //边框样式
    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;
    //窗口有细线边框
    private const int WS_BORDER = 1;
    //窗口有一个最大化按钮
    private const int WS_MAXIMIZEBOX = 0x00010000;
    //窗口有一个最小化按钮
    private const int WS_MINIMIZEBOX = 0x00020000;
    //窗口是一个重叠窗口。重叠窗口有标题栏和边框。
    private const int WS_OVERLAPPED = 0x00000000;
    //有标题栏
    private const int WS_TILEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
    //隐藏标题栏图标
    private const int WS_POPUP = 0x800000;
    //窗口的标题栏上有一个窗口菜单。也必须指定ws_CAPTION样式。
    private const int WS_SYSMENU = 0x80000;
    //最大最小化
    private const int SW_SHOWMINIMIZED = 2;//(最小化窗口)
    private const int SW_SHOWMAXIMIZED = 3;//最大化窗口
    //去除标题栏保留边框
    private const int WS_CAPTION = 0x00C00000;
    private const int WS_THICKFRAME = 0x00040000;
    private TitleBarStyles titleBarStyles;

    /// <summary>
    /// 隐藏标题栏修改分辨率
    /// </summary>
    /// <param name="winWidth"></param>
    /// <param name="winHeight"></param>
    /// <param name="winPosX"></param>
    /// <param name="winPosY"></param>
    public void ChangeScreenSizeHide(int winWidth, int winHeight, int winPosX, int winPosY)
    {
        IntPtr ptr = FindWindow(null, "ttt");

        //去除标题栏（不可拖拽缩放）；一定要加SetWindowPos（）设置才会立即生效
        SetWindowLong(ptr, GWL_STYLE, WS_POPUP);
        //去除标题栏（可拖拽缩放）
        //SetWindowLong(GetForegroundWindow(), GWL_STYLE, GetWindowLong(GetForegroundWindow(), GWL_STYLE) & ~WS_CAPTION | WS_THICKFRAME);
        SetWindowPos(GetForegroundWindow(), 0, winPosX - (int)(winWidth * 0.5f), winPosY - (int)(winHeight * 0.5f), winWidth, winHeight, SWP_SHOWWINDOW);
        //ChangeScreenSize(winWidth, winHeight, winPosX, winPosY);

        titleBarStyles = TitleBarStyles.Hide;
    }
    //不隐藏标题栏修改分辨率
    public void ChangeScreenSizeShow(int winWidth, int winHeight, int winPosX, int winPosY)
    {
        if (titleBarStyles == TitleBarStyles.Hide)
        {
            SetWindowLong(GetForegroundWindow(), GWL_STYLE, GetWindowLong(GetForegroundWindow(), GWL_STYLE) | WS_TILEDWINDOW);
        }
        ChangeScreenSize(winWidth, winHeight, winPosX, winPosY);
        titleBarStyles = TitleBarStyles.Show;
    }
    /// <summary>
    /// 修改分辨率
    /// </summary>
    /// <param name="winWidth"></param>
    /// <param name="winHeight"></param>
    /// <param name="winPosX"></param>
    /// <param name="winPosY"></param>
    /// <param name="winPoscy"></param>
    void ChangeScreenSize(int winWidth, int winHeight, int winPosX, int winPosY)
    {
        SetWindowPos(GetForegroundWindow(), 0, winPosX - (int)(winWidth * 0.5f), winPosY - (int)(winHeight * 0.5f), winWidth, winHeight, SWP_SHOWWINDOW);
    }

    void Delay(float seconds, Action onFinished)
    {
        StartCoroutine(DelayCoroutione(seconds, onFinished));
    }
    private IEnumerator DelayCoroutione(float seconds, Action onFinished)
    {
        yield return new WaitForSeconds(seconds);
        onFinished();
    }
    /// <summary>
    /// 窗口最大化
    /// </summary>
    public void SetMaxWindows()
    {
        int currMaxScreenHeight = Screen.currentResolution.height - GetTaskBarHeight();
        ChangeScreenSizeHide(Screen.currentResolution.width, currMaxScreenHeight, (int)(Screen.currentResolution.width * 0.5f), (int)(currMaxScreenHeight * 0.5f));
        // SetWindowPos(GetForegroundWindow(), 0, 0, 0, Screen.currentResolution.width, currMaxScreenHeight, SWP_SHOWWINDOW);
    }
    /// <summary>
    /// 获取任务栏高度
    /// </summary>
    /// <returns>任务栏高度</returns>
    private int GetTaskBarHeight()
    {
        int taskbarHeight = 0;
        IntPtr hWnd = FindWindow("Shell_TrayWnd", 0);
        RECT rect = new RECT();
        GetWindowRect(hWnd, ref rect);
        taskbarHeight = (int)rect.Bottom - (int)rect.Top;
         Log.Debug(taskbarHeight);
        return taskbarHeight;
    }
    /// <summary>
    /// 拖动窗口
    /// </summary>
    /// <param name="window">当前句柄</param>
    public void DragWindow()
    {
        ReleaseCapture();
        SendMessage(GetForegroundWindow(), 0xA1, 0x02, 0);
        SendMessage(GetForegroundWindow(), 0x0202, 0, 0);
    }
    /// <summary>
    /// 最小化窗口
    /// </summary>
    public void SetMinWindows()
    {
        ShowWindow(GetForegroundWindow(), SW_SHOWMINIMIZED);
    }

    /// <summary>
    /// 全屏
    /// </summary>
    public void SetFullScreen()
    {
        ShowWindow(GetForegroundWindow(), SW_SHOWMAXIMIZED);
    }
}

