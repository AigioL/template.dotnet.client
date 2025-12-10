namespace AigioLTemplate.Commands.Features.Abstractions;

partial interface IFeatureCommand
{
    ///// <summary>
    ///// 获取业务命令名
    ///// </summary>
    ///// <returns></returns>
    //internal static abstract string GetCommandName(); 直接 typeof 泛型取 name 作为命令名，定义接口函数手动实现影响复制粘贴可能复制忘记改了
}