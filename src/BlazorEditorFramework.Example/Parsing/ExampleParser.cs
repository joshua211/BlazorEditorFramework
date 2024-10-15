using System.Text.RegularExpressions;
using BlazorEditorFramework.Core.Editor;
using BlazorEditorFramework.Core.Editor.Parsing;
using BlazorEditorFramework.Core.Editor.Parsing.Abstractions;

namespace BlazorEditorFramework.Example.Parsing;

public partial class ExampleParser : ILanguageParser
{
    public Task<IReadOnlyCollection<Node>> ParseDocument(string document)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<Node>> ParseDocumentParts(string document, List<DocumentPart> partsToParse)
    {
        var nodes = new List<Node>();
        var from = 0;
        var to = 0;

        foreach (var part in partsToParse)
        {
            var words = MyRegex().Split(part.Content).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            for (var i = 0; i < words.Length; i++)
            {
                var word = words[i];
                if (word is " " or "\n")
                {
                    if (word is not "\n")
                        nodes.Add(new(from, ++from, "ignore"));
                    else
                        from++;

                    continue;
                }

                var isLetterCountEvent = word.Length % 2 == 0;
                to = from + word.Length;
                nodes.Add(new(from, to, isLetterCountEvent ? "even" : "odd"));
                from = to;
            }
        }

        return Task.FromResult<IReadOnlyCollection<Node>>(nodes);
    }

    [GeneratedRegex(@"(?<=\s)|(?=\s)")]
    private static partial Regex MyRegex();
}