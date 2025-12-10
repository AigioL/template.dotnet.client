//using AigioLTemplate.Models;
//using MemoryPack;
//using System.Diagnostics;
//using System.Diagnostics.CodeAnalysis;
//using System.Text.Json;

//namespace AigioLTemplate.Commands.Features.Abstractions;

///// <summary>
///// 功能业务命令抽象基类
///// </summary>
//abstract class FeatureCommand
//{

//}

///// <inheritdoc cref="FeatureCommand"/>
//abstract class FeatureCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFeatureCommand> : FeatureCommand, IFeatureCommand
//    where TFeatureCommand : IFeatureCommandVoid, IFeatureCommand
//{
//    static string IFeatureCommand.GetCommandName() => TFeatureCommand.GetCommandName();

//    static async ValueTask IFeatureCommand.InvokeAsync(SerializableImplType implType, Stream request, Stream response, CancellationToken cancellationToken)
//    {
//        ApiRsp? result = null;
//        try
//        {
//            var isSuccess = TFeatureCommand.Invoke();
//            result ??= new();
//            result.SetIsSuccess(isSuccess);
//        }
//        catch (Exception ex)
//        {
//#if DEBUG
//            // 在调试模式下，让 IDE 定位到异常位置
//            Debugger.BreakForUserUnhandledException(ex);
//#endif
//            result ??= new();
//            result.SetException(ex);
//        }
//        finally
//        {
//            result ??= new();
//            if (string.IsNullOrWhiteSpace(result.Url))
//            {
//                var url = TFeatureCommand.GetCommandName();
//                result.Url = url;
//            }
//        }
//        switch (implType)
//        {
//            case SerializableImplType.SystemTextJson:
//                {
//                    await JsonSerializer.SerializeAsync(response, result, DefaultJsonSerializerContext_.Default.ApiRsp, cancellationToken);
//                }
//                break;
//            case SerializableImplType.MemoryPack:
//                {
//                    await MemoryPackSerializer.SerializeAsync(response, result, cancellationToken: cancellationToken);
//                }
//                break;
//            default:
//                throw new ArgumentOutOfRangeException(nameof(implType), implType, null);
//        }
//    }
//}

///// <inheritdoc cref="FeatureCommand"/>
//abstract class FeatureCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFeatureCommand, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommandArgs> : FeatureCommand, IFeatureCommand
//    where TFeatureCommand : IFeatureCommand<TCommandArgs>, IFeatureCommand
//    where TCommandArgs : IJsonSerializerContext
//{
//    static string IFeatureCommand.GetCommandName() => TFeatureCommand.GetCommandName();

//    static async ValueTask IFeatureCommand.InvokeAsync(SerializableImplType implType, Stream request, Stream response, CancellationToken cancellationToken)
//    {
//        ApiRsp? result = null;
//        try
//        {
//            TCommandArgs? args;
//            switch (implType)
//            {
//                case SerializableImplType.SystemTextJson:
//                    {
//                        var options = IJsonSerializerContext.GetJsonSerializerOptions<TCommandArgs>();
//#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
//#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
//                        args = await JsonSerializer.DeserializeAsync<TCommandArgs>(request, options, cancellationToken);
//#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
//#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
//                    }
//                    break;
//                case SerializableImplType.MemoryPack:
//                    {
//                        args = await MemoryPackSerializer.DeserializeAsync<TCommandArgs>(request, cancellationToken: cancellationToken);
//                    }
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(implType), implType, null);
//            }

//            var isSuccess = TFeatureCommand.Invoke(args);
//            result ??= new();
//            result.SetIsSuccess(isSuccess);
//        }
//        catch (Exception ex)
//        {
//#if DEBUG
//            // 在调试模式下，让 IDE 定位到异常位置
//            Debugger.BreakForUserUnhandledException(ex);
//#endif
//            result ??= new();
//            result.SetException(ex);
//        }
//        finally
//        {
//            result ??= new();
//            if (string.IsNullOrWhiteSpace(result.Url))
//            {
//                var url = TFeatureCommand.GetCommandName();
//                result.Url = url;
//            }
//        }
//        switch (implType)
//        {
//            case SerializableImplType.SystemTextJson:
//                {
//                    await JsonSerializer.SerializeAsync(response, result, DefaultJsonSerializerContext_.Default.ApiRsp, cancellationToken);
//                }
//                break;
//            case SerializableImplType.MemoryPack:
//                {
//                    await MemoryPackSerializer.SerializeAsync(response, result, cancellationToken: cancellationToken);
//                }
//                break;
//            default:
//                throw new ArgumentOutOfRangeException(nameof(implType), implType, null);
//        }
//    }
//}

///// <inheritdoc cref="FeatureCommand"/>
//abstract class FeatureCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFeatureCommand, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommandArgs, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TContent> : FeatureCommand, IFeatureCommand
//    where TFeatureCommand : IFeatureCommand<TCommandArgs, TContent>, IFeatureCommand
//    where TCommandArgs : IJsonSerializerContext
//{
//    static string IFeatureCommand.GetCommandName() => TFeatureCommand.GetCommandName();

//    static async ValueTask IFeatureCommand.InvokeAsync(SerializableImplType implType, Stream request, Stream response, CancellationToken cancellationToken)
//    {
//        ApiRsp<TContent>? result = null;
//        try
//        {
//            TCommandArgs? args;
//            switch (implType)
//            {
//                case SerializableImplType.SystemTextJson:
//                    {
//                        var options = IJsonSerializerContext.GetJsonSerializerOptions<TCommandArgs>();
//#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
//#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
//                        args = await JsonSerializer.DeserializeAsync<TCommandArgs>(request, options, cancellationToken);
//#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
//#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
//                    }
//                    break;
//                case SerializableImplType.MemoryPack:
//                    {
//                        args = await MemoryPackSerializer.DeserializeAsync<TCommandArgs>(request, cancellationToken: cancellationToken);
//                    }
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(implType), implType, null);
//            }

//            result ??= new();
//            result.Content = TFeatureCommand.Invoke(args);
//            result.SetIsSuccess(true);
//        }
//        catch (Exception ex)
//        {
//#if DEBUG
//            // 在调试模式下，让 IDE 定位到异常位置
//            Debugger.BreakForUserUnhandledException(ex);
//#endif
//            result ??= new();
//            result.SetException(ex);
//        }
//        finally
//        {
//            result ??= new();
//            if (string.IsNullOrWhiteSpace(result.Url))
//            {
//                var url = TFeatureCommand.GetCommandName();
//                result.Url = url;
//            }
//        }
//        switch (implType)
//        {
//            case SerializableImplType.SystemTextJson:
//                {
//                    var jsonTypeInfo = DefaultJsonSerializerContext_.Default.GetTypeInfo(typeof(ApiRsp<TContent>));
//                    ArgumentNullException.ThrowIfNull(jsonTypeInfo); // 返回类型必须加入 DefaultJsonSerializerContext_ 的 attr 中
//                    await JsonSerializer.SerializeAsync(response, result, jsonTypeInfo, cancellationToken);
//                }
//                break;
//            case SerializableImplType.MemoryPack:
//                {
//                    await MemoryPackSerializer.SerializeAsync(response, result, cancellationToken: cancellationToken);
//                }
//                break;
//            default:
//                throw new ArgumentOutOfRangeException(nameof(implType), implType, null);
//        }
//    }
//}

///// <inheritdoc cref="FeatureCommand"/>
//abstract class FeatureCommandVoid<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFeatureCommand, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TContent> : FeatureCommand, IFeatureCommand
//    where TFeatureCommand : IFeatureCommandVoid<TContent>, IFeatureCommand
//{
//    static string IFeatureCommand.GetCommandName() => TFeatureCommand.GetCommandName();

//    static async ValueTask IFeatureCommand.InvokeAsync(SerializableImplType implType, Stream request, Stream response, CancellationToken cancellationToken)
//    {
//        ApiRsp<TContent>? result = null;
//        try
//        {
//            result ??= new();
//            result.Content = TFeatureCommand.Invoke();
//            result.SetIsSuccess(true);
//        }
//        catch (Exception ex)
//        {
//#if DEBUG
//            // 在调试模式下，让 IDE 定位到异常位置
//            Debugger.BreakForUserUnhandledException(ex);
//#endif
//            result ??= new();
//            result.SetException(ex);
//        }
//        finally
//        {
//            result ??= new();
//            if (string.IsNullOrWhiteSpace(result.Url))
//            {
//                var url = TFeatureCommand.GetCommandName();
//                result.Url = url;
//            }
//        }
//        switch (implType)
//        {
//            case SerializableImplType.SystemTextJson:
//                {
//                    var jsonTypeInfo = DefaultJsonSerializerContext_.Default.GetTypeInfo(typeof(ApiRsp<TContent>));
//                    ArgumentNullException.ThrowIfNull(jsonTypeInfo); // 返回类型必须加入 DefaultJsonSerializerContext_ 的 attr 中
//                    await JsonSerializer.SerializeAsync(response, result, jsonTypeInfo, cancellationToken);
//                }
//                break;
//            case SerializableImplType.MemoryPack:
//                {
//                    await MemoryPackSerializer.SerializeAsync(response, result, cancellationToken: cancellationToken);
//                }
//                break;
//            default:
//                throw new ArgumentOutOfRangeException(nameof(implType), implType, null);
//        }
//    }
//}

///// <inheritdoc cref="FeatureCommand"/>
//abstract class FeatureCommandVoid2<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFeatureCommand> : FeatureCommand, IFeatureCommand
//    where TFeatureCommand : IFeatureCommandVoid2, IFeatureCommand
//{
//    static string IFeatureCommand.GetCommandName() => TFeatureCommand.GetCommandName();

//    static async ValueTask IFeatureCommand.InvokeAsync(SerializableImplType implType, Stream request, Stream response, CancellationToken cancellationToken)
//    {
//        ApiRsp? result = null;
//        try
//        {
//            result = TFeatureCommand.Invoke();
//        }
//        catch (Exception ex)
//        {
//#if DEBUG
//            // 在调试模式下，让 IDE 定位到异常位置
//            Debugger.BreakForUserUnhandledException(ex);
//#endif
//            result ??= new();
//            result.SetException(ex);
//        }
//        finally
//        {
//            result ??= new();
//            if (string.IsNullOrWhiteSpace(result.Url))
//            {
//                var url = TFeatureCommand.GetCommandName();
//                result.Url = url;
//            }
//        }
//        switch (implType)
//        {
//            case SerializableImplType.SystemTextJson:
//                {
//                    await JsonSerializer.SerializeAsync(response, result, DefaultJsonSerializerContext_.Default.ApiRsp, cancellationToken);
//                }
//                break;
//            case SerializableImplType.MemoryPack:
//                {
//                    await MemoryPackSerializer.SerializeAsync(response, result, cancellationToken: cancellationToken);
//                }
//                break;
//            default:
//                throw new ArgumentOutOfRangeException(nameof(implType), implType, null);
//        }
//    }
//}

///// <inheritdoc cref="FeatureCommand"/>
//abstract class FeatureCommandVoid2<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFeatureCommand, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TContent> : FeatureCommand, IFeatureCommand
//    where TFeatureCommand : IFeatureCommandVoid2<TContent>, IFeatureCommand
//{
//    static string IFeatureCommand.GetCommandName() => TFeatureCommand.GetCommandName();

//    static async ValueTask IFeatureCommand.InvokeAsync(SerializableImplType implType, Stream request, Stream response, CancellationToken cancellationToken)
//    {
//        ApiRsp<TContent>? result = null;
//        try
//        {
//            result = TFeatureCommand.Invoke();
//        }
//        catch (Exception ex)
//        {
//#if DEBUG
//            // 在调试模式下，让 IDE 定位到异常位置
//            Debugger.BreakForUserUnhandledException(ex);
//#endif
//            result ??= new();
//            result.SetException(ex);
//        }
//        finally
//        {
//            result ??= new();
//            if (string.IsNullOrWhiteSpace(result.Url))
//            {
//                var url = TFeatureCommand.GetCommandName();
//                result.Url = url;
//            }
//        }
//        switch (implType)
//        {
//            case SerializableImplType.SystemTextJson:
//                {
//                    var jsonTypeInfo = DefaultJsonSerializerContext_.Default.GetTypeInfo(typeof(ApiRsp<TContent>));
//                    ArgumentNullException.ThrowIfNull(jsonTypeInfo); // 返回类型必须加入 DefaultJsonSerializerContext_ 的 attr 中
//                    await JsonSerializer.SerializeAsync(response, result, jsonTypeInfo, cancellationToken);
//                }
//                break;
//            case SerializableImplType.MemoryPack:
//                {
//                    await MemoryPackSerializer.SerializeAsync(response, result, cancellationToken: cancellationToken);
//                }
//                break;
//            default:
//                throw new ArgumentOutOfRangeException(nameof(implType), implType, null);
//        }
//    }
//}

///// <inheritdoc cref="FeatureCommand"/>
//abstract class FeatureCommand3<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFeatureCommand, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommandArgs> : FeatureCommand, IFeatureCommand
//    where TFeatureCommand : IFeatureCommand3<TCommandArgs>, IFeatureCommand
//    where TCommandArgs : IJsonSerializerContext
//{
//    static string IFeatureCommand.GetCommandName() => TFeatureCommand.GetCommandName();

//    static async ValueTask IFeatureCommand.InvokeAsync(SerializableImplType implType, Stream request, Stream response, CancellationToken cancellationToken)
//    {
//        ApiRsp? result = null;
//        try
//        {
//            TCommandArgs? args;
//            switch (implType)
//            {
//                case SerializableImplType.SystemTextJson:
//                    {
//                        var options = IJsonSerializerContext.GetJsonSerializerOptions<TCommandArgs>();
//#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
//#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
//                        args = await JsonSerializer.DeserializeAsync<TCommandArgs>(request, options, cancellationToken);
//#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
//#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
//                    }
//                    break;
//                case SerializableImplType.MemoryPack:
//                    {
//                        args = await MemoryPackSerializer.DeserializeAsync<TCommandArgs>(request, cancellationToken: cancellationToken);
//                    }
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(implType), implType, null);
//            }

//            result = TFeatureCommand.Invoke(args);
//        }
//        catch (Exception ex)
//        {
//#if DEBUG
//            // 在调试模式下，让 IDE 定位到异常位置
//            Debugger.BreakForUserUnhandledException(ex);
//#endif
//            result ??= new();
//            result.SetException(ex);
//        }
//        finally
//        {
//            result ??= new();
//            if (string.IsNullOrWhiteSpace(result.Url))
//            {
//                var url = TFeatureCommand.GetCommandName();
//                result.Url = url;
//            }
//        }
//        switch (implType)
//        {
//            case SerializableImplType.SystemTextJson:
//                {
//                    await JsonSerializer.SerializeAsync(response, result, DefaultJsonSerializerContext_.Default.ApiRsp, cancellationToken);
//                }
//                break;
//            case SerializableImplType.MemoryPack:
//                {
//                    await MemoryPackSerializer.SerializeAsync(response, result, cancellationToken: cancellationToken);
//                }
//                break;
//            default:
//                throw new ArgumentOutOfRangeException(nameof(implType), implType, null);
//        }
//    }
//}

///// <inheritdoc cref="FeatureCommand"/>
//abstract class FeatureCommand3<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFeatureCommand, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommandArgs, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TContent> : FeatureCommand, IFeatureCommand
//    where TFeatureCommand : IFeatureCommand3<TCommandArgs, TContent>, IFeatureCommand
//    where TCommandArgs : IJsonSerializerContext
//{
//    static string IFeatureCommand.GetCommandName() => TFeatureCommand.GetCommandName();

//    static async ValueTask IFeatureCommand.InvokeAsync(SerializableImplType implType, Stream request, Stream response, CancellationToken cancellationToken)
//    {
//        ApiRsp<TContent>? result = null;
//        try
//        {
//            TCommandArgs? args;
//            switch (implType)
//            {
//                case SerializableImplType.SystemTextJson:
//                    {
//                        var options = IJsonSerializerContext.GetJsonSerializerOptions<TCommandArgs>();
//#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
//#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
//                        args = await JsonSerializer.DeserializeAsync<TCommandArgs>(request, options, cancellationToken);
//#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
//#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
//                    }
//                    break;
//                case SerializableImplType.MemoryPack:
//                    {
//                        args = await MemoryPackSerializer.DeserializeAsync<TCommandArgs>(request, cancellationToken: cancellationToken);
//                    }
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(implType), implType, null);
//            }

//            result = TFeatureCommand.Invoke(args);
//        }
//        catch (Exception ex)
//        {
//#if DEBUG
//            // 在调试模式下，让 IDE 定位到异常位置
//            Debugger.BreakForUserUnhandledException(ex);
//#endif
//            result ??= new();
//            result.SetException(ex);
//        }
//        finally
//        {
//            result ??= new();
//            if (string.IsNullOrWhiteSpace(result.Url))
//            {
//                var url = TFeatureCommand.GetCommandName();
//                result.Url = url;
//            }
//        }
//        switch (implType)
//        {
//            case SerializableImplType.SystemTextJson:
//                {
//                    var jsonTypeInfo = DefaultJsonSerializerContext_.Default.GetTypeInfo(typeof(ApiRsp<TContent>));
//                    ArgumentNullException.ThrowIfNull(jsonTypeInfo); // 返回类型必须加入 DefaultJsonSerializerContext_ 的 attr 中
//                    await JsonSerializer.SerializeAsync(response, result, jsonTypeInfo, cancellationToken);
//                }
//                break;
//            case SerializableImplType.MemoryPack:
//                {
//                    await MemoryPackSerializer.SerializeAsync(response, result, cancellationToken: cancellationToken);
//                }
//                break;
//            default:
//                throw new ArgumentOutOfRangeException(nameof(implType), implType, null);
//        }
//    }
//}