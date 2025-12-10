namespace AigioLTemplate.Models.Ipc.Events.Abstractions;

internal interface IEventData // private class n
{
    string? Event { get; set; }

    object? Data { get; }

    Type DataType { get; }
}

internal interface IEventData<T> : IEventData
{
    new T? Data { get; set; }
}