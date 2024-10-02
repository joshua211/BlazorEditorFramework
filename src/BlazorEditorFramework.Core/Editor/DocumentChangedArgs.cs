using BlazorEditorFramework.Core.Editor.Input;
using BlazorEditorFramework.Core.Editor.Render;

namespace BlazorEditorFramework.Core.Editor;

public class DocumentChangedArgs : EventArgs
{
    public DocumentChangedArgs(string newDocument, string oldDocument,
        IReadOnlyList<DocumentLine> newLines, IReadOnlyList<DocumentLine> oldLines,
        Selection newSelection, Selection oldSelection,
        IReadOnlyCollection<DocumentChange> changes, InputOrigin origin, CancellationToken cancellationToken)
    {
        NewDocument = newDocument;
        NewLines = newLines;
        OldDocument = oldDocument;
        OldLines = oldLines;
        Changes = changes;
        NewSelection = newSelection;
        OldSelection = oldSelection;
        CancellationToken = cancellationToken;
        Origin = origin;
    }

    public string NewDocument { get; }
    public string OldDocument { get; }
    public IReadOnlyList<DocumentLine> NewLines { get; }
    public IReadOnlyList<DocumentLine> OldLines { get; }
    public IReadOnlyCollection<DocumentChange> Changes { get; }
    public Selection NewSelection { get; private set; }
    public Selection OldSelection { get; private set; }
    public CancellationToken CancellationToken { get; private set; }
    public InputOrigin Origin { get; private set; }
}