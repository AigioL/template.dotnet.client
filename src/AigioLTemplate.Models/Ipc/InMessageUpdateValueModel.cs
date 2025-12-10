namespace AigioLTemplate.Models.Ipc;

public sealed partial record class InMessageUpdateValueModel
{
    public required string Name { get; set; }

    public double Value { get; set; }
}
