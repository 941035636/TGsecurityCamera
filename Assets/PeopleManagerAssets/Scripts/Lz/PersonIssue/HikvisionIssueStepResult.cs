using System;

[Serializable]
public class HikvisionIssueStepResult
{
    public bool Success;
    public int SdkErrorCode;
    public int DeviceStatusCode;
    public string Message;

    public static HikvisionIssueStepResult Ok(string message)
    {
        return new HikvisionIssueStepResult
        {
            Success = true,
            Message = message ?? "OK"
        };
    }

    public static HikvisionIssueStepResult Fail(string message, int sdkErrorCode = 0, int deviceStatusCode = 0)
    {
        return new HikvisionIssueStepResult
        {
            Success = false,
            Message = string.IsNullOrEmpty(message) ? "未知错误" : message,
            SdkErrorCode = sdkErrorCode,
            DeviceStatusCode = deviceStatusCode
        };
    }
}
