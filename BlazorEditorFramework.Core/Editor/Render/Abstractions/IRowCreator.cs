namespace BlazorEditorFramework.Core.Editor.Render.Abstractions;

public interface IRowCreator
{
    IReadOnlyCollection<Row> CalculateRows(IReadOnlyList<DocumentLine> lines, IReadOnlyCollection<Node> nodes,
        EditorState state);
}