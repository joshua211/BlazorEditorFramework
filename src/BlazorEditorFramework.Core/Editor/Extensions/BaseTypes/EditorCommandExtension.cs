using BlazorEditorFramework.Core.Editor.Commands;

namespace BlazorEditorFramework.Core.Editor.Extensions.BaseTypes;

public abstract class EditorCommandExtension : Extension, IEditorCommand
{
    public abstract IReadOnlyCollection<string> ModifierKeys { get; }
    public abstract string Key { get; }
    public abstract string ActionIdentifier { get; }

    public bool Matches(IReadOnlyCollection<string> modifiers, string key)
    {
        return ModifierKeys.SequenceEqual(modifiers.Order()) && Key == key;
    }
}