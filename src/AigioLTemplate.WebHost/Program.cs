using AigioL.Common.Models;
using AigioLTemplate.Commands;
using AigioLTemplate.Commands.CommandLines;
using AigioLTemplate.Hosting;
using AigioLTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioLTemplate.WebHost;

static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        Console.Write("IsDynamicCodeCompiled: ");
        Console.WriteLine(RuntimeFeature.IsDynamicCodeCompiled);

        Console.Write("IsDynamicCodeSupported: ");
        Console.WriteLine(RuntimeFeature.IsDynamicCodeSupported);

        unsafe
        {
            IMainCommand.FunctionPointerRunWebApp = &_5d504e18.RunWebAppInBackground;
            IMainCommand.FunctionPointerShutdownWebApp = &_5d504e18.ShutdownWebApp;
        }

        var exitCode = AigioLTemplateHost.Start(args);
        return exitCode;
    }
}

static class _5d504e18
{
    static WebApplication? app;

    internal static void RunWebAppInBackground() => InBackground(() =>
    {
        try
        {
            RunWebApp();
        }
        finally
        {
            app = null;
            AigioLTemplateHost.Exit();
        }
    });

    static AigioLTemplateWebAppOptions GetOptions()
    {
        AigioLTemplateWebAppOptions? o = null;
        var path = Path.Combine(AppContext.BaseDirectory, $"{Path.GetFileNameWithoutExtension(Environment.ProcessPath)}.json");
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            o = JsonSerializer.Deserialize(fs, AppJsonSerializerContext.Default.AigioLTemplateWebAppOptions);
        }
        catch (DirectoryNotFoundException)
        {
        }
        catch (FileNotFoundException)
        {
        }
        return o ?? new();
    }

    static void RunWebApp()
    {
        var o = GetOptions();
        var builder = WebApplication.CreateSlimBuilder(o);

        if (o.UseHttps)
        {
            builder.WebHost.UseKestrelHttpsConfiguration();
        }

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, DefaultJsonSerializerContext_.Default);
        });

        builder.Services.AddOpenApi();

        app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference("/", options =>
            {
                options.Title = "AigioLTemplate.WebHost Scalar API Reference";
            });
        }

        var sampleTodos = new Todo[] {
            new(1, "Walk the dog"),
            new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
            new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
            new(4, "Clean the bathroom"),
            new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
        };

        var todosApi = app.MapGroup("/todos");
        todosApi.MapGet("/", () => sampleTodos);
        todosApi.MapGet("/{id}", (int id) =>
            sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
                ? Results.Ok(todo)
                : Results.NotFound());

        MapDocCommands();

        app.MapPost("/c/{commandName}", async (
            HttpContext context,
            [FromRoute] string commandName,
            [FromHeader(Name = "Content-Type")] string contentType) =>
        {
            // POST: https://localhost:9443/c/{commandName}
            // https://localhost:9443/c/CreateRemoteThread
            // https://localhost:9443/c/%e6%97%a0%e8%af%b7%e6%b1%82%e6%9c%89%e5%93%8d%e5%ba%94%e7%a4%ba%e4%be%8b

            if (!CommandHelpers.TryGetSerializableImplTypeByContentType(
                contentType,
                out var implType))
            {
                context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                return;
            }
            var inputStream = context.Request.Body;
            var outputStream = context.Response.Body;
            await CommandHelpers.InvokeAsync(commandName, implType, inputStream, outputStream, context.RequestAborted);
        });

        app.MapPost("/e/{c}", async (
            HttpContext context,
            [FromRoute] string c) =>
        {
            // TODO: 加密版本的 API
            // 1. JS 随机生成 AES 密钥
            // 2. 使用 RSA 公钥加密 AES 密钥
            // 3. 用 AES 加密 commandName+contentType 生成字节转为 Base64Url 字符串作为路由参数 c
            // 4. 用 AES 加密请求正文内容发送请求
            // 5. C# 处理终结点请求
            // 6. 用 AES 解密响应正文内容
            await Task.CompletedTask;
        });

        //app.UseWelcomePage("/");

        if (o.UseHttps)
        {
            app.UseHttpsRedirection();
        }

        app.Run();
    }

    static void MapDocCommands()
    {
        var mMapDocPost = typeof(_5d504e18).GetMethod("MapDocPost", BindingFlags.NonPublic | BindingFlags.Static);
        ArgumentNullException.ThrowIfNull(mMapDocPost);
        var commands = CommandHelpers.GetCommands();
        foreach (var it in commands)
        {
            var commandName = it.Key;
            switch (commandName)
            {
                case "MainWindow":
                case "Window":
                    continue;
            }
#pragma warning disable IL2057 // Unrecognized value passed to the parameter of method. It's not possible to guarantee the availability of the target type.
            var commandType =
                Type.GetType($"AigioLTemplate.Commands.Features.{commandName}") ??
                Type.GetType($"AigioLTemplate.Commands.Features.Samples.{commandName}") ??
                Type.GetType($"AigioLTemplate.Commands.Features.Essentials.{commandName}") ??
                Type.GetType($"AigioLTemplate.Commands.Features.WebApi.{commandName}");
#pragma warning restore IL2057 // Unrecognized value passed to the parameter of method. It's not possible to guarantee the availability of the target type.
            ArgumentNullException.ThrowIfNull(commandType);
            var i = commandType.GetInterfaces().FirstOrDefault(static x => x.IsGenericType && x.Name == "IV2FeatureCommand`2");
            ArgumentNullException.ThrowIfNull(i);
            var ga = i.GetGenericArguments();
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning disable IL2060 // Call to 'System.Reflection.MethodInfo.MakeGenericMethod' can not be statically analyzed. It's not possible to guarantee the availability of requirements of the generic method.
            mMapDocPost.MakeGenericMethod(ga[0], typeof(ApiRsp<>).MakeGenericType(ga[1]))
                .Invoke(null, [commandName]);
#pragma warning restore IL2060 // Call to 'System.Reflection.MethodInfo.MakeGenericMethod' can not be statically analyzed. It's not possible to guarantee the availability of requirements of the generic method.
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
        }
    }

    static void MapDocPost<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TArgs, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(string commandName)
    {
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning disable RDG011 // Type parameters not supported
        app!.MapPost($"/doc/{commandName}", async (
            HttpContext context,
            [FromHeader(Name = "Content-Type")] string contentType) =>
        {
            if (!CommandHelpers.TryGetSerializableImplTypeByContentType(
              contentType,
              out var implType))
            {
                context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                return default;
            }
            context.Request.EnableBuffering();
            var inputStream = context.Request.Body;
            using var inputStream2 = new MemoryStream();
            using var outputStream = new MemoryStream();
            await inputStream.CopyToAsync(inputStream2, context.RequestAborted);

            var reqJson = Encoding.UTF8.GetString(inputStream2.ToArray());
            inputStream2.Position = 0;

            await CommandHelpers.InvokeAsync(commandName, implType, inputStream2, outputStream, context.RequestAborted);
            outputStream.Position = 0;
            var result = JsonSerializer.Deserialize<TResult>(outputStream, DefaultJsonSerializerContext_.Default.Options);
            return result;
        });
#pragma warning restore RDG011 // Type parameters not supported
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
    }

    static async void InBackground(Action action, bool longRunning = true)
    {
        TaskCreationOptions options = TaskCreationOptions.DenyChildAttach;

        if (longRunning)
        {
            options |= TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness;
        }

        await Task.Factory.StartNew(action, CancellationToken.None, options, TaskScheduler.Default).ConfigureAwait(false);
    }

    static async void Stop(this WebApplication app) => await app.StopAsync();

    static void Shutdown(this WebApplication app)
    {
        app.Stop();
        app.WaitForShutdown();
    }

    internal static void ShutdownWebApp() => app?.Shutdown();
}

sealed record class Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

sealed class AigioLTemplateWebAppOptions : WebApplicationOptions
{
    public bool UseHttps { get; set; } = true;
}

[JsonSerializable(typeof(AigioLTemplateWebAppOptions))]
[JsonSerializable(typeof(Todo[]))]
[JsonSourceGenerationOptions(
    UseStringEnumConverter = true)]
sealed partial class AppJsonSerializerContext : JsonSerializerContext
{
    static AppJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new AppJsonSerializerContext(o);
    }
}
