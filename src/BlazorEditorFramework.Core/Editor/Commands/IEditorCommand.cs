namespace BlazorEditorFramework.Core.Editor.Commands;

public interface IEditorCommand
{
    IReadOnlyCollection<string> ModifierKeys { get; }
    string Key { get; }
    string ActionIdentifier { get; }

    bool Matches(IReadOnlyCollection<string> modifiers, string key);
}