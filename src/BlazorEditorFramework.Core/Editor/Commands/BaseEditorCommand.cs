using BlazorEditorFramework.Core.Editor.Extensions.BaseTypes;

namespace BlazorEditorFramework.Core.Editor.Commands;

public abstract class BaseEditorCommand : EditorCommandExtension
{
    public BaseEditorCommand(IEnumerable<string> modifierKeys, string key, string actionIdentifier)
    {
        ModifierKeys = modifierKeys.Order().ToList();
        Key = key;
        ActionIdentifier = actionIdentifier;
    }

    public override IReadOnlyCollection<string> ModifierKeys { get; }
    public override string Key { get; }
    public override string ActionIdentifier { get; }
}