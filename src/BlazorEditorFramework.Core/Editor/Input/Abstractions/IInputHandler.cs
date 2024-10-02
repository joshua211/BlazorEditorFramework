namespace BlazorEditorFramework.Core.Editor.Input.Abstractions;

public interface IInputHandler
{
    Task<EditorUpdate> HandleInputOnDocumentAsync(string originalDocument, Selection originalSelection,
        InputEvent inputEvent, bool isFirstExecution);

    void SetInputFeatureExtensions(IEnumerable<IInputFeatureExtension> inputFeatureExtensions);

    IEnumerable<InputEvent> ExpandInput(InputEvent ev);
}