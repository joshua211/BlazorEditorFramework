using BlazorEditorFramework.Core.Editor.Extensions.BaseTypes;
using BlazorEditorFramework.Core.Editor.Parsing.Abstractions;

namespace BlazorEditorFramework.Example.Parsing;

public class ExampleLanguageExtension : LanguageExtension
{
    private readonly ExampleParser parser;

    public ExampleLanguageExtension(ExampleParser parser)
    {
        this.parser = parser;
    }

    public override ILanguageParser GetParser()
    {
        return parser;
    }
}