using System;
using System.IO;

public class FileUtilR
{
    /// <summary>
    /// 获取当前运行路径
    /// </summary>
    /// <returns></returns>
    public static string GetRunDirectory()
    {
        return Environment.CurrentDirectory;
    }
    /// <summary>
    /// 获取当前运行路径的父路径
    /// </summary>
    /// <returns></returns>
    public static string GetRunDirectoryInParentPath(int pathNum = 3)
    {
        //对于我们当前运行的程序来说，他的目录即为他的父对象
        //获取当前运行目录的父文件夹信息
        //对于我们的当前程序来说。pathInfo为X://XX//...//bin;
        //GetRunDirectory为X://XX//...//Debug;
        DirectoryInfo pathInfo = Directory.GetParent(GetRunDirectory());
        //根据pathNum获取当前路径向上几层的父路径
        while (pathNum > 0 && pathInfo.Parent != null)
        {
            DirectoryInfo info = pathInfo.Parent;
            pathInfo = info;
            pathNum--;
        }
        //获取一个完整的文件夹路径
        return pathInfo.FullName;
    }
    /// <summary>
    /// 创建一个文件夹
    /// </summary>
    /// <param name="path">文件夹路径</param>
    /// <param name="foldName">文件夹名称</param>
    /// <returns></returns>
    public static string CreateFolder(string path)
    {
        //如果目录不存在，则创建一个目录文件夹
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        return path;
    }
    /// <summary>
    /// 创建一个文件夹
    /// </summary>
    /// <param name="path">文件夹路径</param>
    /// <param name="foldName">文件夹名称</param>
    /// <returns></returns>
    public static string CreateFolder(string path, string foldName)
    {
        //拼接路径
        string fold = path + "/" + foldName;
        //如果目录不存在，则创建一个目录文件夹
        if (!Directory.Exists(fold))
            Directory.CreateDirectory(fold);
        return fold;
    }
    /// <summary>
    /// 创建一个文本文件
    /// </summary>
    /// <param name="path">文本文件</param>
    /// <param name="fileName">文件名字</param>
    /// <param name="info">文件信息</param>
    public static void CreateFile(string path, string fileName, string info)
    {
        //写入流对象
        StreamWriter streamWriter;
        //拼接一个文本文件对象
        FileInfo fileInfo = new FileInfo(path + "/" + fileName);
        //判断文件夹是否存在，如果不存在，则创建一个目录
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        //如果该文件已存在，则直接删除
        if (fileInfo.Exists)
            File.Delete(path + "/" + fileName);
        //创建一个文本对象，并返回给流对象
        streamWriter = fileInfo.CreateText();
        //写入数据
        streamWriter.WriteLine(info);
        //写入完成后关闭
        streamWriter.Close();
        //销毁
        streamWriter.Dispose();
    }
    /// <summary>
    /// 向一个文件内添加一段数据
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <param name="info"></param>
    public static void AddFile(string path, string fileName, string info)
    {
        //数据流对象
        StreamWriter streamWriter;
        //拼接一个文本文件对象
        FileInfo fileInfo = new FileInfo(path + "/" + fileName);
        //判断文件夹是否存在，如果不存在，则创建一个目录
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        //如果文件不存在，则创建一个文件，否则加载该文件，并将文件对象赋值给流对象
        streamWriter = !fileInfo.Exists ? fileInfo.CreateText() : fileInfo.AppendText();
        //写入数据
        streamWriter.WriteLine(info);
        //写入完成后关闭
        streamWriter.Close();
        //销毁
        streamWriter.Dispose();
    }
    /// <summary>
    /// 读取文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string LoadFile(string path, string fileName)
    {
        //如果文件不存在，则返回空
        if (!IsExistsFile(path, fileName)) return null;
        //使用读取流对象打开一个文本
        StreamReader sr = File.OpenText(path + "/" + fileName);

        string content = sr.ReadToEnd();
        string str = content.Replace("\r", "").Trim();
        sr.Close();
        sr.Dispose();
        return str;
    }
    /// <summary>
    /// 文件是否存在
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static bool IsExistsFile(string path, string fileName)
    {
        //如果文件件不存在的话，直接返回不存在
        if (!Directory.Exists(path))
            return false;
        //如果文件不存在的话 返回不存在
        if (!File.Exists(path + "/" + fileName))
            return false;
        return true;
    }
    /// <summary>
    /// 覆盖文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    public static void CoverFile(string path, string fileName, string info)
    {
        string filePath = path + "/" + fileName;
        File.WriteAllText(filePath, info);
    }
    /// <summary>
    /// 提取文本最后一行数据
    /// </summary>
    /// <param name="fs">文件流</param>
    /// <returns>最后一行数据</returns>
    public static string GetLastLine(string path, string fileName)
    {
        //如果文件不存在，则返回空
        if (!IsExistsFile(path, fileName)) return null;
        //使用读取流对象打开一个文本
        StreamReader sr = File.OpenText(path + "/" + fileName);
        string lastStr = "";
        while (!sr.EndOfStream)
        {
            lastStr = sr.ReadLine();
        }
        sr.Close();
        sr.Dispose();
        return lastStr;
    }

    public static void DeleteFile(string path, string fileName)
    {
        string filePath = path + "/" + fileName;
        //如果文件不存在，则返回空

        if (!File.Exists(filePath))
        {
            return;
        }
        else
        {

            File.Delete(filePath);
        }
    }

    public static void DeleteFolder(string path, string foldName)
    {
        string fold = path + "/" + foldName;
        //如果文件不存在，则返回空
        if (!Directory.Exists(fold))
            return;
        try
        {
            Directory.Delete(fold, true);
        }
        catch (Exception ex)
        {
            Log.Debug(ex.Message);
        }
    }
}


