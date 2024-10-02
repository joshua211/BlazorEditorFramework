using BlazorEditorFramework.Core.Editor.Helper;
using BlazorEditorFramework.Core.Editor.Input.Abstractions;

namespace BlazorEditorFramework.Core.Editor.Input;

public class InputHandler : IInputHandler
{
    private List<IInputFeatureExtension> inputFeatures = new();

    public void SetInputFeatureExtensions(IEnumerable<IInputFeatureExtension> inputFeatureExtensions) =>
        inputFeatures.AddRange(inputFeatureExtensions);

    public IEnumerable<InputEvent> ExpandInput(InputEvent ev)
    {
        var newEvents = new List<InputEvent>();
        foreach (var f in inputFeatures)
        {
            newEvents.AddRange(f.ExpandInput(ev));
        }

        return newEvents;
    }

    public async Task<EditorUpdate> HandleInputOnDocumentAsync(string originalDocument, Selection originalSelection,
        InputEvent inputEvent, bool isFirstExecution)
    {
        foreach (var inputFeature in inputFeatures)
        {
            // TODO maybe introduce something like a partial input feature, which merges the changes of multiple input features
            var editorUpdate =
                await inputFeature.HandleInputAsync(originalDocument, originalSelection, inputEvent, isFirstExecution);

            if (editorUpdate is not null)
            {
                return editorUpdate;
            }
        }

        return inputEvent.Type switch
        {
            InputType.InsertText => HandleInsertText(originalDocument, originalSelection, inputEvent),
            InputType.DeleteContentBackward => HandleDeleteContentBackward(originalDocument, originalSelection,
                inputEvent),
            InputType.DeleteContentForward => HandleDeleteContentForward(originalDocument, originalSelection,
                inputEvent),
            InputType.DeleteWordBackward => HandleDeleteContentBackward(originalDocument, originalSelection,
                inputEvent),
            InputType.DeleteWordForward => HandleDeleteContentForward(originalDocument, originalSelection,
                inputEvent),
            InputType.InsertFromPaste => HandleInsertFromPaste(originalDocument, originalSelection, inputEvent),
            InputType.InsertParagraph => HandleInsertParagraph(originalDocument, originalSelection, inputEvent),
            InputType.InsertLineBreak => HandleInsertLineBreak(originalDocument, originalSelection, inputEvent),
            InputType.InsertTab => HandleInsertTab(originalDocument, originalSelection, inputEvent),
            InputType.ChangeSelection => HandleChangeSelection(originalDocument, originalSelection, inputEvent),
            _ => throw new ArgumentException("Invalid input type " + inputEvent.Type)
        };
    }

    private static EditorUpdate HandleChangeSelection(string originalDocument, Selection originalSelection,
        InputEvent currentInputEvent)
    {
        var data = (SelectionInputData)currentInputEvent.Data;
        var selection = data.Selection;
        return new EditorUpdate(currentInputEvent, originalDocument, selection, originalDocument, originalSelection,
            Array.Empty<DocumentChange>());
    }

    private static EditorUpdate HandleInsertTab(string originalDocument, Selection selection, InputEvent inputEvent)
    {
        var changes = new List<DocumentChange>();

        string newDoc;
        Selection newSelection;
        if (selection.IsCollapsed)
        {
            newDoc = originalDocument.Insert(selection.From, new string(' ', 4));
            newSelection = new Selection(From: selection.From + 4, To: selection.From + 4);

            changes.Add(new DocumentChange(selection.From, selection.To, 4, ChangeType.Inserted));
        }
        else
        {
            newDoc = originalDocument
                .Remove(selection.From, selection.Length)
                .Insert(selection.From, new string(' ', 4));

            var selectionSize = selection.Length;

            newSelection = new Selection(From: selection.From, To: selection.From + 4);

            changes.Add(new DocumentChange(selection.From, selection.To, 4 - selectionSize, ChangeType.Inserted));
        }

        return new EditorUpdate(inputEvent, newDoc, newSelection, originalDocument, selection, changes);
    }

    private static EditorUpdate HandleInsertText(string originalDocument, Selection selection, InputEvent inputEvent)
    {
        var data = inputEvent.Data;
        var normalizedSelection = selection.Normalize();
        var changes = new List<DocumentChange>();

        string newDoc;
        Selection newSelection;
        if (selection.IsCollapsed)
        {
            newDoc = originalDocument.Insert(selection.From, data);
            newSelection = new Selection(From: selection.From + data.Length, To: selection.From + data.Length);
        }
        else
        {
            newDoc = originalDocument
                .Remove(normalizedSelection.From, normalizedSelection.To - normalizedSelection.From)
                .Insert(normalizedSelection.From, data);
            newSelection = new Selection(normalizedSelection.From + data.Length,
                normalizedSelection.From + data.Length);
        }

        changes.Add(new DocumentChange(selection.From, selection.To, data.Length - selection.Length,
            ChangeType.Modified));

        return new EditorUpdate(inputEvent, newDoc, newSelection, originalDocument, selection, changes);
    }

    private static EditorUpdate HandleDeleteContentBackward(string originalDocument, Selection selection,
        InputEvent inputEvent)
    {
        var normalizedSelection = selection.Normalize();
        var changes = new List<DocumentChange>();

        if (selection is { IsCollapsed: true, From: 0 })
        {
            return new EditorUpdate(inputEvent, originalDocument, selection, originalDocument, selection, changes);
        }

        string newDoc;
        Selection newSelection;
        if (selection.IsCollapsed)
        {
            var isWord = inputEvent.Data.Modifiers.Contains(ModifierKeys.Control);

            // strg + backspace
            if (isWord)
            {
                var start = selection.From - 1;
                while (start > 0 && char.IsWhiteSpace(originalDocument[start]))
                {
                    start--;
                }

                while (start > 0 && !char.IsWhiteSpace(originalDocument[start]))
                {
                    start--;
                }

                newDoc = originalDocument.Remove(start, selection.From - start);
                newSelection = new Selection(From: start, To: start);

                changes.Add(new DocumentChange(selection.From, selection.To, -(selection.From - start),
                    ChangeType.Modified));
            }
            // backspace
            else
            {
                newDoc = originalDocument.Remove(selection.From - 1, 1);
                newSelection = new Selection(From: selection.From - 1, To: selection.From - 1);

                changes.Add(new DocumentChange(selection.From, selection.From, -1, ChangeType.Modified));
            }
        }
        else
        {
            newDoc = originalDocument.Remove(normalizedSelection.From,
                normalizedSelection.To - normalizedSelection.From);
            newSelection = new Selection(normalizedSelection.From, normalizedSelection.From);

            changes.Add(new DocumentChange(normalizedSelection.From, normalizedSelection.To,
                -normalizedSelection.Length, ChangeType.Modified));
        }

        return new EditorUpdate(inputEvent, newDoc, newSelection, originalDocument, selection, changes);
    }

    private static EditorUpdate HandleDeleteContentForward(string originalDocument, Selection selection,
        InputEvent inputEvent)
    {
        var normalizedSelection = selection.Normalize();
        var changes = new List<DocumentChange>();

        if (selection is { IsCollapsed: true } && selection.From == originalDocument.Length)
        {
            return new EditorUpdate(inputEvent, originalDocument, selection, originalDocument, selection, changes);
        }

        string newDoc;
        Selection newSelection;
        if (selection.IsCollapsed)
        {
            var isWord = inputEvent.Data.Modifiers.Contains(ModifierKeys.Control);

            // strg + delete
            if (isWord)
            {
                var start = selection.From;
                while (start < originalDocument.Length && char.IsWhiteSpace(originalDocument[start]))
                {
                    start++;
                }

                while (start < originalDocument.Length && !char.IsWhiteSpace(originalDocument[start]))
                {
                    start++;
                }

                newDoc = originalDocument.Remove(selection.From, start - selection.From);
                newSelection = new Selection(selection.From, To: selection.From);

                changes.Add(new DocumentChange(selection.From, selection.To, -(start - selection.From),
                    ChangeType.Modified));
            }
            // delete
            else
            {
                newDoc = originalDocument.Remove(selection.From, 1);
                newSelection = new Selection(selection.From, To: selection.From);

                changes.Add(new DocumentChange(selection.From, selection.To, -1, ChangeType.Modified));
            }
        }
        else
        {
            newDoc = originalDocument.Remove(normalizedSelection.From,
                normalizedSelection.To - normalizedSelection.From);
            newSelection = new Selection(normalizedSelection.From, To: normalizedSelection.From);

            changes.Add(new DocumentChange(normalizedSelection.From, normalizedSelection.To,
                -normalizedSelection.Length, ChangeType.Modified));
        }

        return new EditorUpdate(inputEvent, newDoc, newSelection, originalDocument, selection, changes);
    }

    private static EditorUpdate HandleInsertFromPaste(string originalDocument, Selection selection,
        InputEvent inputEvent)
    {
        if (inputEvent.Data is StreamInputData || string.IsNullOrEmpty(inputEvent.Data))
        {
            return new EditorUpdate(inputEvent, originalDocument, selection, originalDocument,
                selection, Array.Empty<DocumentChange>());
        }

        return HandleInsertText(originalDocument, selection, inputEvent);
    }

    private static EditorUpdate HandleInsertParagraph(string originalDocument, Selection selection,
        InputEvent inputEvent)
    {
        var isCtrl = inputEvent.Data.Modifiers.Contains(ModifierKeys.Control);

        if (isCtrl)
        {
            var lines = originalDocument.GetLines();
            var line = lines.First(l => selection.Intersects(l));
            selection = new Selection(line.To, line.To);
        }

        var normalizedSelection = selection.Normalize();

        string newDoc;
        Selection newSelection;
        var changes = new List<DocumentChange>();

        if (selection.IsCollapsed)
        {
            newDoc = originalDocument.Insert(selection.From, "\n");
            newSelection = new Selection(From: selection.From + 1, To: selection.From + 1);

            changes.Add(new DocumentChange(selection.From, selection.To, 1, ChangeType.Inserted));
        }
        else
        {
            newDoc = originalDocument
                .Remove(normalizedSelection.From, normalizedSelection.To - normalizedSelection.From)
                .Insert(normalizedSelection.From, "\n");
            newSelection = new Selection(From: normalizedSelection.From + 1, To: normalizedSelection.From + 1);

            changes.Add(new DocumentChange(normalizedSelection.From, normalizedSelection.To,
                1 - normalizedSelection.Length, ChangeType.Inserted));
        }

        return new EditorUpdate(inputEvent, newDoc, newSelection, originalDocument, selection, changes);
    }

    private static EditorUpdate HandleInsertLineBreak(string originalDocument, Selection selection,
        InputEvent inputEvent)
    {
        return HandleInsertParagraph(originalDocument, selection, inputEvent);
    }
}