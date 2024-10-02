using System.Diagnostics;
using BlazorEditorFramework.Core.Editor.Input;
using BlazorEditorFramework.Core.Editor.Input.Abstractions;

namespace BlazorEditorFramework.Core.Editor.DocumentEvents;

public class DocumentEventStream
{
    private readonly string rootDocument;
    private readonly Selection rootSelection;

    private string? cachedDocument = null;
    private Selection? cachedSelection = null;
    private int currentIndex = 0;
    private List<InputEvent> events;
    private HashSet<Guid> knownIds = new();
    private Stack<InputEvent> redoStack = new();

    public DocumentEventStream(string rootDocument, Selection rootSelection)
    {
        this.rootDocument = rootDocument;
        this.rootSelection = rootSelection;
        this.events = new List<InputEvent>();
    }

    public string Document => cachedDocument ?? rootDocument;

    public Selection Selection => cachedSelection ?? rootSelection;

    public async Task<DocumentEventResult> ApplyEventAsync(InputEvent ev, IInputHandler inputHandler)
    {
        var watch = Stopwatch.StartNew();
        events.Add(ev);

        var expanded = inputHandler.ExpandInput(ev).ToList();
        events.AddRange(expanded);

        var res = await ApplyUncommittedEvents(inputHandler);
        knownIds.Add(ev.Id);
        knownIds.UnionWith(expanded.Select(x => x.Id));

        watch.Stop();
        Log.Logger.Information("Applied event {Event} in {Elapsed} ms", ev.Type, watch.ElapsedMilliseconds);

        return res;
    }

    public async Task UndoLatestEvent(IInputHandler inputHandler)
    {
        cachedDocument = null;
        cachedSelection = null;
        currentIndex = 0;
        if (events.Count <= 0)
            return;

        var ev = events[^1];
        foreach (var sideEffect in ev.SideEffects)
        {
            await sideEffect.UndoSideEffectAsync();
        }

        events.Remove(ev);
        knownIds.Remove(ev.Id);
        redoStack.Push(ev);

        await ApplyUncommittedEvents(inputHandler);
    }

    public async Task RedoLatestEvent(IInputHandler inputHandler)
    {
        if (redoStack.Count <= 0)
            return;

        cachedDocument = null;
        cachedSelection = null;
        currentIndex = 0;

        var ev = redoStack.Pop();
        events.Add(ev);

        await ApplyUncommittedEvents(inputHandler);

        knownIds.Add(ev.Id);
    }

    private async Task<DocumentEventResult> ApplyUncommittedEvents(IInputHandler inputHandler)
    {
        var currentDoc = cachedDocument ?? rootDocument;
        var currentSelection = cachedSelection ?? rootSelection;
        var changes = new List<DocumentChange>();

        foreach (var ev in events.Skip(currentIndex))
        {
            var istFirstExec = knownIds.Contains(ev.Id) is false;
            var res = await inputHandler.HandleInputOnDocumentAsync(currentDoc, currentSelection, ev, istFirstExec);

            if (istFirstExec)
            {
                foreach (var sideEffect in ev.SideEffects)
                {
                    await sideEffect.ApplySideEffectAsync();
                }
            }

            currentDoc = res.NewDocument;
            currentSelection = res.NewSelection;
            changes.AddRange(res.Changes);
            currentIndex++;
        }

        cachedDocument = currentDoc;
        cachedSelection = currentSelection;

        return new DocumentEventResult(currentDoc, currentSelection, changes);
    }
}