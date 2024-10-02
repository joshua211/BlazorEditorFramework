namespace BlazorEditorFramework.Core.Editor.Input;

public class StreamInputData : InputData
{
    public StreamInputData(Stream streamData) : base(null, new List<string>())
    {
        StreamData = streamData;
    }

    public Stream StreamData { get; private set; }
}