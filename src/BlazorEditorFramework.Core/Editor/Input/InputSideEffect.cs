namespace BlazorEditorFramework.Core.Editor.Input;

public class InputSideEffect
{
    public InputSideEffect(Func<Task> applySideEffectAsync, Func<Task> undoSideEffectAsync)
    {
        ApplySideEffectAsync = applySideEffectAsync;
        UndoSideEffectAsync = undoSideEffectAsync;
    }

    public Func<Task> ApplySideEffectAsync { get; }

    public Func<Task> UndoSideEffectAsync { get; }
}