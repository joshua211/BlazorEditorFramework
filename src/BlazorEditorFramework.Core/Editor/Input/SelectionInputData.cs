namespace BlazorEditorFramework.Core.Editor.Input;

public class SelectionInputData : InputData
{
    public SelectionInputData(Selection selection) : base(null, new List<string>())
    {
        Selection = selection;
    }

    public Selection Selection { get; private set; }
}