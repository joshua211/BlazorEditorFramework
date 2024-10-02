namespace BlazorEditorFramework.Core.Editor.Actions.Abstractions;

public interface IEditorAction
{
    string Identifier { get; }
    Task ExecuteAsync(EditorState state);
}