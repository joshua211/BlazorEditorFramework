using BlazorEditorFramework.Core.Editor;
using BlazorEditorFramework.Core.Editor.Extensions;
using BlazorEditorFramework.Core.Editor.Extensions.BaseTypes;

namespace BlazorEditorFramework.Example.Decorations;

public class TooltipDecoration : DecoExtension
{
    public override IEnumerable<Decoration> EnterNode(EditorState state, Node node)
    {
        if (node.NodeType == "ignore")
            return [];

        var nodeText = state.Document.Substring(node.From, node.To - node.From);
        var isEven = node.NodeType == "even";
        var count = nodeText.Length;

        var deco = new Decoration(node.From, node.To, typeof(TooltipDecorationDisplay), DecorationOrientation.Overlap,
            new Dictionary<string, object>()
            {
                { "Text", nodeText },
                { "IsEven", isEven },
                { "Count", count }
            });

        return [deco];
    }
}