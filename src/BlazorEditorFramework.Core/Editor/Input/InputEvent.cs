namespace BlazorEditorFramework.Core.Editor.Input;

public record InputEvent
{
    public InputEvent(InputType type, InputData data, DateTime timeStamp,
        IReadOnlyCollection<InputSideEffect> sideEffects)
    {
        Type = type;
        Data = data;
        TimeStamp = timeStamp;
        SideEffects = sideEffects;
        Id = Guid.NewGuid();
    }

    public InputType Type { get; init; }
    public InputData Data { get; init; }
    public DateTime TimeStamp { get; init; }
    public IReadOnlyCollection<InputSideEffect> SideEffects { get; init; }
    public Guid Id { get; init; }
}