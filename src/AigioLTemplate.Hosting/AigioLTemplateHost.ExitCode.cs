namespace AigioLTemplate.Hosting;

#pragma warning disable IDE1006 // 命名样式
#pragma warning disable SA1302 // Interface names should begin with I
#if !NETFRAMEWORK
partial interface AigioLTemplateHost
#else
partial class AigioLTemplateHost
#endif
{
    /// <summary>
    /// AigioLTemplate 应用程序进程退出状态码
    /// <para>建议将用户定义的退出代码限制为 范围 64 - 113（除了 0，表示成功），以符合 C/C++ 标准</para>
    /// <para>https://tldp.org/LDP/abs/html/exitcodes.html</para>
    /// </summary>
    internal enum ExitCode : byte
    {
        /// <summary>
        /// 🆗
        /// </summary>
        Ok = 0,

        /// <summary>
        /// 出现 <see cref="System.Exception"/>
        /// </summary>
        Exception = 64,

        /// <summary>
        /// 参数不正确
        /// <para>例如</para>
        /// <list type="bullet">
        /// <item>某个业务定义的字符串类型参数不能为空字符串或 <see langword="null"/></item>
        /// </list>
        /// </summary>
        Failure_BadArguments = 89,

        /// <summary>
        /// 未找到
        /// <para>例如</para>
        /// <list type="bullet">
        /// <item>某个业务定义的 Id 类参数查找不到或匹配不上</item>
        /// </list>
        /// </summary>
        Failure_NotFound = 90,

        /// <summary>
        /// 提权启动自身，当前进程退出
        /// </summary>
        Elevated = 91,

        /// <summary>
        /// 失败
        /// </summary>
        Failure = 92,

        /// <summary>
        /// 禁止多开程序
        /// </summary>
        Failure_Mutex = 93,

        /// <summary>
        /// 不兼容的操作系统版本
        /// </summary>
        Failure_IncompatibleOS = 98,

        /// <summary>
        /// <see cref="Environment.UserInteractive"/> 不能为 <see langword="false"/>
        /// </summary>
        Failure_UserInteractiveFalse = 99,

        /// <summary>
        /// 未授权访问，缺少必要权限，例如 AppData 或 Cache 文件夹无法访问或未使用管理员权限启动此进程在某些命令行参数下
        /// </summary>
        Failure_StartUnauthorizedAccess = 111,

        ///// <summary>
        ///// 重复调用启动函数
        ///// </summary>
        //Failure_StartDuplicate = 112,
    }

    internal readonly record struct ExitCodeStruct
    {
        readonly int Value;

        ExitCodeStruct(int value)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();

        public static implicit operator ExitCodeStruct(int exitCode)
            => new(exitCode);

        public static implicit operator int(ExitCodeStruct exitCode)
            => exitCode.Value;

        public static implicit operator ExitCodeStruct(ExitCode exitCode)
            => new(unchecked((int)exitCode));

        public static implicit operator ExitCode(ExitCodeStruct exitCode)
            => unchecked((ExitCode)exitCode);
    }
}
