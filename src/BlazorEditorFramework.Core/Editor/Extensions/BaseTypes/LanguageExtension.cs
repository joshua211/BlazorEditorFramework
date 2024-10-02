using BlazorEditorFramework.Core.Editor.Parsing.Abstractions;

namespace BlazorEditorFramework.Core.Editor.Extensions.BaseTypes;

public abstract class LanguageExtension : Extension
{
    public abstract ILanguageParser GetParser();
}