using System;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

/// <summary>
/// 窗口属性和式样更改
/// </summary>
public class WindowsSetting : MonoBehaviour
{
    /// <summary>
    /// 改变窗口的属性
    /// </summary>
    /// <param name="hwnd">窗口句柄</param>
    /// <param name="_nIndex">窗口式样</param>
    /// <param name="dwNewLong">窗口属性</param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, uint dwNewLong);


    /// <summary>
    /// 改变窗口大小位置
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <param name="hWndInsertAfter">窗口Z层位置</param>
    /// <param name="X">窗口左上角在屏幕上X轴位置（从屏幕左上角开始计算）</param>
    /// <param name="Y">窗口左上角在屏幕上Y轴位置（从屏幕左上角开始计算）</param>
    /// <param name="cx">窗口的宽（weight）</param>
    /// <param name="cy">窗口的高（height）</param>
    /// <param name="uFlags">窗口设置</param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);


    /// <summary>
    /// 获取一个前台窗口的句柄。在某些情况下，如一个窗口失去激活时，前台窗口可以是NULL。
    /// </summary>
    /// <returns></returns>
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();


    /// <summary>
    /// 获取一个窗口的窗口属性
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <param name="dwNewLong">需要获取的窗口属性</param>
    /// <returns></returns>
    [DllImport("User32.dll")]
    private static extern uint GetWindowLong(IntPtr hWnd, int dwNewLong);


    /// <summary>
    /// 获取制定标题名字的窗口
    /// </summary>
    /// <param name="lpClassName"></param>
    /// <param name="lpWindowName"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    //窗口的位置大小
    public Rect size;

    //窗口名字，要跟PlayerSetting里面的ProductName一样，否则将获取不了窗口
    public string windowsName;

    private const int GWL_STYLE = -16;  //设定一个新的窗口风格。

    #region uFlags参数
    private const uint SWP_NOSIZE = 0x0001;                        //忽略 cx、cy, 保持大小
    private const uint SWP_NOMOVE = 0x0002;                        //忽略 X、Y, 不改变位置
    private const uint SWP_NOZORDER = 0x0004;                      //忽略 hWndInsertAfter, 保持 Z 顺序
    private const uint SWP_NOREDRAW = 0x0008;                      //不重绘
    private const uint SWP_NOACTIVATE = 0x0010;                    //不激活
    private const uint SWP_FRAMECHANGED = 0x0020;                  //强制发送 WM_NCCALCSIZE 消息, 一般只是在改变大小时才发送此消息
    private const uint SWP_SHOWWINDOW = 0x0040;                    //显示窗口 (默认选择此参数)
    private const uint SWP_HIDEWINDOW = 0x0080;                    //隐藏窗口
    private const uint SWP_NOCOPYBITS = 0x0100;                    //丢弃客户区
    private const uint SWP_NOOWNERZORDER = 0x0200;                 //忽略 hWndInsertAfter, 不改变 Z 序列的所有者
    private const uint SWP_NOSENDCHANGING = 0x0400;                //不发出 WM_WINDOWPOSCHANGING 消息
    private const uint SWP_DRAWFRAME = SWP_FRAMECHANGED;           //画边框
    private const uint SWP_NOREPOSITION = SWP_NOOWNERZORDER;
    private const uint SWP_DEFERERASE = 0x2000;                    //防止产生 WM_SYNCPAINT 消息
    private const uint SWP_ASYNCWINDOWPOS = 0x4000;                //若调用进程不拥有窗口, 系统会向拥有窗口的线程发出需求
    #endregion

    #region hWndInsertAfter参数
    private const int HWND_BOTTOM = 1;           //初始窗口位置在底层
    private const int HWND_TOP = 0;              //初始窗口位置在顶层
    private const int HWND_TOPMOST = -1;         //窗口置顶, 位于任何顶部窗口的前面
    private const int HWND_NOTOPMOST = -2;       //窗口置顶, 位于其他顶部窗口的后面
    #endregion

    #region 常用窗口式样（dwNewLong）的值
    private const uint WS_BORDER = 0x00800000;          //细线边框
    private const uint WS_CAPTION = 0x00C00000;         //标题栏
    private const uint WS_CHILD = 0x40000000;           //子窗口
    private const uint WS_DISABLE = 0x08000000;         //禁用的窗口
    private const uint WS_HSCROLL = 0x00100000;         //横向滚动条
    private const uint WS_VSCROLL = 0x00200000;         //纵向滚动条
    private const uint WS_MAXIMIZE = 0x01000000;        //最大化窗口
    private const uint WS_MAXIMIZEBOX = 0x00010000;     //最大化按钮
    private const uint WS_MINIMIZE = 0x20000000;        //最小化窗口
    private const uint WS_MINIMIZEBOX = 0x00020000;     //最小化按钮
    private const uint WS_POPUP = 0x80000000;           //弹出式窗口（无边框）    
                                                        //※注意：网上多数将WS_POPUP的值定义为0x00800000，但是0x00800000是WS_BORDER的值
    private const uint WS_SYSMENU = 0x000800000;        //窗口菜单按钮
    private const uint WS_THICKFRAME = 0x00040000;      //可拖动调大小窗口（上面粗边框，其他细边框，可调节窗口大小）
    private const uint WS_VISIBLE = 0x10000000;         //初始可见的窗口
    #endregion




    private void Start()
    {
        SetWindowStyle();
        //如果不行，请尝试一下使用协程
        //StartCoroutine("SetWindowStyle");

    }
    private void SetWindowStyle()
    {
        //yield return new WaitForSeconds(0.1f);
        //获取最前端窗口句柄，（慎用，当有置顶窗口在unity的窗口前时，会获取到置顶窗口的句柄）
        //IntPtr ptr = GetForegroundWindow();
        //获取指定标题窗口的句柄（更严谨，除非有别的一样名字的窗口，否则一般不会获取错误）
        IntPtr ptr = FindWindow(null, windowsName);
        //弹出式窗口
        SetWindowLong(ptr, GWL_STYLE, WS_POPUP);
        //【GetWindowLong(ptr, GWL_STYLE) & ~WS_CAPTION & ~WS_BORDER】 从一个标准窗口中去掉标题和边框
        //SetWindowLong(ptr, GWL_STYLE, GetWindowLong(ptr, GWL_STYLE) & ~WS_CAPTION & ~WS_BORDER);
        //设置屏幕大小和位置
        bool result = SetWindowPos(ptr, HWND_TOP, (int)size.x, (int)size.y, (int)size.width, (int)size.height, SWP_SHOWWINDOW);
    }
}