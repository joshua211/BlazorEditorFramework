using BlazorEditorFramework.Core.Editor.Extensions.BaseTypes;

namespace BlazorEditorFramework.Core.Editor.Actions;

public abstract class BaseEditorAction : ActionExtension
{
    protected BaseEditorAction(string identifier)
    {
        Identifier = identifier;
    }

    public override string Identifier { get; }
    public abstract override Task ExecuteAsync(EditorState state);
}