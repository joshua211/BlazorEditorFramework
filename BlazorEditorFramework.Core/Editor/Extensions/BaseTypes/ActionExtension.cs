using BlazorEditorFramework.Core.Editor.Actions.Abstractions;

namespace BlazorEditorFramework.Core.Editor.Extensions.BaseTypes;

public abstract class ActionExtension : Extension, IEditorAction
{
    public abstract string Identifier { get; }

    public abstract Task ExecuteAsync(EditorState state);
}