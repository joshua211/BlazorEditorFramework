namespace BlazorEditorFramework.Core.Editor.Parsing.Abstractions;

public interface IDocumentSplitter
{
    List<DocumentPart> SplitDocument(string document);
}