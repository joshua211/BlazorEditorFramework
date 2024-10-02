namespace BlazorEditorFramework.Core.Editor.Render;

public record TextCol(string Content, int From, int To, string Tag, string ClassString) : Col(From, To, Tag);