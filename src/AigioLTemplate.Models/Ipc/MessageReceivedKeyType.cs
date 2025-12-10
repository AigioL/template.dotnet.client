namespace AigioLTemplate.Models.Ipc;

public enum MessageReceivedKeyType
{
    /// <summary>
    /// 游戏进程的 In 管道收到更新数值的消息时
    /// </summary>
    InMessageUpdateValue = 1,

    /// <summary>
    /// 当全局热键被按下时
    /// </summary>
    OnHotkeyPressed = 2,

    /// <summary>
    /// 游戏进程退出时
    /// </summary>
    GameProcessExited = 3,

    /// <summary>
    /// 消息接收管道发生异常时
    /// </summary>
    Exception = 500,
}
