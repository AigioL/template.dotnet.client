namespace AigioLTemplate.Models.Ipc.Queues.Abstractions;

internal interface IQueueItem // private class m
{
    int Id { get; set; }

    QueueItemStatus Status { get; set; }

    object? Data { get; }

    Type DataType { get; }
}

internal interface IQueueItem<T> : IQueueItem
{
    new T? Data { get; set; }
}