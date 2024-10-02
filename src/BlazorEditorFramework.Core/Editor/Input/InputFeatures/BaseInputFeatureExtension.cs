using BlazorEditorFramework.Core.Editor.Input.Abstractions;

namespace BlazorEditorFramework.Core.Editor.Input.InputFeatures;

public abstract class BaseInputFeatureExtension : IInputFeatureExtension
{
    public virtual IEnumerable<InputEvent> ExpandInput(InputEvent inputEvent)
    {
        return Array.Empty<InputEvent>();
    }

    public async Task<EditorUpdate?> HandleInputAsync(string document,
        Selection selection,
        InputEvent inputEvent, bool isFirstExecution)
    {
        var result = inputEvent.Type switch
        {
            InputType.InsertText => await InsertTextFeatureAsync(document, selection, inputEvent, isFirstExecution),
            InputType.DeleteContentBackward => await DeleteContentBackwardFeatureAsync(document, selection, inputEvent,
                isFirstExecution),
            InputType.DeleteContentForward => await DeleteContentForwardFeatureAsync(document, selection, inputEvent,
                isFirstExecution),
            InputType.DeleteWordBackward =>
                await DeleteContentBackwardFeatureAsync(document, selection, inputEvent, isFirstExecution),
            InputType.DeleteWordForward => await DeleteContentForwardFeatureAsync(document, selection, inputEvent,
                isFirstExecution),
            InputType.InsertFromPaste => await InsertPasteFeatureAsync(document, selection, inputEvent,
                isFirstExecution),
            InputType.InsertParagraph => await InsertParagraphFeatureAsync(document, selection, inputEvent,
                isFirstExecution),
            InputType.InsertLineBreak => await InsertLineBreakFeatureAsync(document, selection, inputEvent,
                isFirstExecution),
            InputType.InsertTab => await InsertTabFeatureAsync(document, selection, inputEvent, isFirstExecution),
            InputType.ChangeSelection => await ChangeSelectionFeatureAsync(document, selection, inputEvent,
                isFirstExecution),
            _ => throw new ArgumentException("Invalid input type " + inputEvent.Type)
        };

        return result;
    }

    protected virtual Task<EditorUpdate?> ChangeSelectionFeatureAsync(
        string document,
        Selection selection, InputEvent inputEvent, bool isFirstExecution)
    {
        return Task.FromResult<EditorUpdate?>(null);
    }

    protected virtual Task<EditorUpdate?> InsertTextFeatureAsync(
        string document,
        Selection selection, InputEvent inputEvent, bool isFirstExecution)
    {
        return Task.FromResult<EditorUpdate?>(null);
    }

    protected virtual Task<EditorUpdate?>
        DeleteContentBackwardFeatureAsync(
            string document, Selection selection, InputEvent inputEvent, bool isFirstExecution)
    {
        return Task.FromResult<EditorUpdate?>(null);
    }

    protected virtual Task<EditorUpdate?>
        DeleteContentForwardFeatureAsync(
            string document, Selection selection,
            InputEvent inputEvent, bool isFirstExecution)
    {
        return Task.FromResult<EditorUpdate?>(null);
    }

    protected virtual Task<EditorUpdate?>
        InsertPasteFeatureAsync(
            string document, Selection selection,
            InputEvent inputEvent, bool isFirstExecution)
    {
        return Task.FromResult<EditorUpdate?>(null);
    }

    protected virtual Task<EditorUpdate?>
        InsertParagraphFeatureAsync(
            string document, Selection selection,
            InputEvent inputEvent, bool isFirstExecution)
    {
        return Task.FromResult<EditorUpdate?>(null);
    }

    protected virtual Task<EditorUpdate?>
        InsertLineBreakFeatureAsync(
            string document, Selection selection,
            InputEvent inputEvent, bool isFirstExecution)
    {
        return Task.FromResult<EditorUpdate?>(null);
    }

    protected virtual Task<EditorUpdate?> InsertTabFeatureAsync(
        string document, Selection selection,
        InputEvent inputEvent, bool isFirstExecution)
    {
        return Task.FromResult<EditorUpdate?>(null);
    }
}