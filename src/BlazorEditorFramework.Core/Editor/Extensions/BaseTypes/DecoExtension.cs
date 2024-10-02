namespace BlazorEditorFramework.Core.Editor.Extensions.BaseTypes;

public abstract class DecoExtension : Extension
{
    public abstract IEnumerable<Decoration> EnterNode(EditorState state, Node node);
}