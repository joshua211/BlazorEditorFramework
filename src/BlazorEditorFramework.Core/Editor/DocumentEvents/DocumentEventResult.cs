using BlazorEditorFramework.Core.Editor.Input;

namespace BlazorEditorFramework.Core.Editor.DocumentEvents;

public record DocumentEventResult(string Document, Selection Selection, IReadOnlyCollection<DocumentChange> Changes);