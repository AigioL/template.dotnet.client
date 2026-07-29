## 项目模板（客户端）
使用 .NET Native AOT 发布的后端无窗口应用程序，配合 WebView2/WKWebView 等平台 Web UI 层构建的客户端应用程序，Ipc 调用使用命名管道 + 前端应用程序调用本机库 SDK 实现

### 项目模板重命名
在项目根目录(slnx 文件所在文件夹)下执行以下命令
1. 编译工具 ```dotnet build src\AigioLTemplate.BuildTools -c Debug```
2. 重命名文件 ```src\artifacts\bin\AigioLTemplate.BuildTools\debug\b.exe rename --projName 新项目英文名称```

### 模块

#### Commands
1. ConsoleCommand 定义解析进程传参命令的业务实现
2. FeatureCommand 定义由 Ipc 调用的业务实现

#### Hosting
参考 .NET 通用主机风格实现的 Host 类，类似 WebHost，使用 WinForms 创建空白透明窗口，实现与 Win32 API 交互需要的消息循环

#### DllExport
本机库 SDK，提供给前端应用程序调用，错误日志在 Windows 事件日志中查看

#### Essentials
提供类似移动端系统 SDK 的功能，参考 .NET MAUI Essentials 实现

#### Log
日志由 ```Microsoft.Extensions.Logging``` 抽象层提供 API，具体取决于项目使用控制台日志输出，或 Windows 事件日志输出，或 NLog 实现的文件日志输出

#### Constants/Models
定义仅客户端需要用到的常量与模型类型，如果服务端也需要使用，则应定义在子模块 ServerSdk 中

#### System.IOPath
定义此应用程序用到的，应用数据文件夹目录，与缓存文件夹目录

#### Windows.Win32
定义 CsWin32 的 WinAPI 调用源生成

#### WebHost
带控制台窗口与控制台日志输出，以及 HTTP 服务端监听 + OpenApi 文档，用于调试和测试 Ipc 调用的业务实现（通过反射查找所有 FeatureCommand 暴露成 API 终结点）

#### Ipc 命名管道
查看当前管道，在 Windows Powershell 中输入
```
[System.IO.Directory]::GetFiles("\\.\\pipe\\")
```

查看管道与对应的进程信息
```
打开 ref\ProcessExplorer\procexp64.exe，使用 Find -> Find Handle or DLL... 选项并输入模式 \Device\NamedPipe\
```
