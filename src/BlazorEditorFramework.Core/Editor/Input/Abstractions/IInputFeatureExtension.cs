using BlazorEditorFramework.Core.Editor.Extensions.Abstractions;

namespace BlazorEditorFramework.Core.Editor.Input.Abstractions;

public interface IInputFeatureExtension : IEditorExtension
{
    IEnumerable<InputEvent> ExpandInput(InputEvent inputEvent);

    Task<EditorUpdate?> HandleInputAsync(string document, Selection selection,
        InputEvent inputEvent, bool isFirstExecution);
}