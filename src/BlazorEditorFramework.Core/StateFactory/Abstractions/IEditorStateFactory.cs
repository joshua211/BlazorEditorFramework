using BlazorEditorFramework.Core.Editor;

namespace BlazorEditorFramework.Core.StateFactory.Abstractions;

public interface IEditorStateFactory
{
    EditorState Build(string initialDocument, string? editorId = null);
}