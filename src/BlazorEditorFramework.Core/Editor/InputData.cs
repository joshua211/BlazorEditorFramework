namespace BlazorEditorFramework.Core.Editor;

public class InputData
{
    public InputData(string? data, IReadOnlyCollection<string> modifiers)
    {
        Data = data;
        Modifiers = modifiers;
    }

    public string? Data { get; private set; }
    public IReadOnlyCollection<string> Modifiers { get; private set; }

    public int Length => Data?.Length ?? 0;

    public static implicit operator InputData(string data) => new InputData(data, new List<string>());
    public static implicit operator string(InputData data) => data.Data!;
}