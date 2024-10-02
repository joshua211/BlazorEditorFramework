namespace BlazorEditorFramework.Core.Editor.Input;

public record EditorUpdate(
    InputEvent InputEvent,
    string NewDocument,
    Selection NewSelection,
    string OriginalDocument,
    Selection OriginalSelection,
    IReadOnlyCollection<DocumentChange> Changes)
{
    public bool HasAnythingChanged =>
        Changes.Count > 0 || NewDocument != OriginalDocument || NewSelection != OriginalSelection;
}