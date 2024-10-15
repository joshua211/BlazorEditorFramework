namespace BlazorEditorFramework.Core.Editor.Parsing.Abstractions;

public interface ILanguageParser
{
    Task<IReadOnlyCollection<Node>> ParseDocument(string document);

    Task<IReadOnlyCollection<Node>> ParseDocumentParts(string document, List<DocumentPart> partsToParse);
}