## 项目模板（客户端）
- win 使用 winforms+webview2+aot 参考 WinFormedge 创建全由 web 层控制的无边框窗口，WinFormedge 这个实现了类似 wpf 的框架，没必要太重了，把里面的主窗口 style 逻辑 copy 过来
  - ~~方案一：js 与 C# 通信使用 [本机端和 Web 端代码的互操作](https://learn.microsoft.com/zh-cn/microsoft-edge/webview2/how-to/communicate-btwn-web-native) 不使用注入 JS 对象实例的方式，因为需要 COM 接口导出，目前与 AOT 不兼容~~
    - js 使用 ```window.chrome.webview.postMessage(`SetTitleText ${titleText.value}`);``` 发送消息，C# 使用 ```WebView2.WebMessageReceived``` 事件监听，格式为 json 字符串
    - C# 发送使用 [CoreWebView2.PostWebMessageAsJson](https://learn.microsoft.com/zh-cn/dotnet/api/microsoft.web.webview2.core.corewebview2.postwebmessageasjson)，JS 注册 ```window.chrome.webview.addEventListener("message"```
  数据量大时使用 [SharedBuffer](https://github.com/MicrosoftEdge/WebView2Feedback/blob/main/specs/SharedBuffer.md)
  - 方案二：直接拦截 url post 请求利用 req 和 rsp 替代函数调用
	- 前后端分离，启动 AigioLTemplate.WebHost 项目提供本地 WebApi，前端项目直接调用
	  - 在 WebView2 中，拦截请求处理 fetch
  - fetch api 也由 js 抽象接口，改为调用 C# HttpClient 发送？这样隐藏 baseUrl 在 aot 层汇编？
- mac 使用 .net9.0-macos 原生本机 api 创建本机窗口配合 wkwebivew 复用
- 移动端待定

### Ipc 命名管道
查看当前管道，在 Windows Powershell 中输入
```
[System.IO.Directory]::GetFiles("\\.\\pipe\\")
```

查看管道与对应的进程信息
```
打开 ref\ProcessExplorer\procexp64.exe，使用 Find -> Find Handle or DLL... 选项并输入模式 \Device\NamedPipe\
```
