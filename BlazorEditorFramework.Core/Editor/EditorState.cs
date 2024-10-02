using System.Diagnostics;
using BlazorEditorFramework.Core.Editor.Abstractions;
using BlazorEditorFramework.Core.Editor.Actions.Abstractions;
using BlazorEditorFramework.Core.Editor.Caching.Abstractions;
using BlazorEditorFramework.Core.Editor.Commands;
using BlazorEditorFramework.Core.Editor.DocumentEvents;
using BlazorEditorFramework.Core.Editor.Extensions;
using BlazorEditorFramework.Core.Editor.Extensions.Abstractions;
using BlazorEditorFramework.Core.Editor.Extensions.BaseTypes;
using BlazorEditorFramework.Core.Editor.Helper;
using BlazorEditorFramework.Core.Editor.Input;
using BlazorEditorFramework.Core.Editor.Input.Abstractions;
using BlazorEditorFramework.Core.Editor.Parsing;
using BlazorEditorFramework.Core.Editor.Parsing.Abstractions;
using BlazorEditorFramework.Core.Editor.Render;

namespace BlazorEditorFramework.Core.Editor;

public class EditorState
{
    private readonly DocumentEventStream documentEventStream;
    private readonly IDocumentSplitter documentSplitter;
    private readonly IInputHandler inputHandler;
    private readonly INodeCache nodeCache;
    private Dictionary<string, IEditorAction> actions;

    private List<ClassMappingExtension> classMappings;
    private List<IEditorCommand> commands;
    private List<DecoExtension> decoExtensions;
    private List<IInputFeatureExtension> inputFeatures;
    private LanguageExtension? languageExtension;

    private List<DocumentLine> lines;
    private List<DocumentPart> parts;

    public EditorState(string document, string noteId, IInputHandler inputHandler, INodeCache nodeCache,
        IDocumentSplitter splitter,
        Selection? startSelection = null)
    {
        this.inputHandler = inputHandler;
        this.nodeCache = nodeCache;
        this.documentSplitter = splitter;

        var doc = document.Replace(Environment.NewLine, "\n");
        documentEventStream = new DocumentEventStream(doc, startSelection ?? new Selection(0, 0));
        NoteId = noteId;
        classMappings = new List<ClassMappingExtension>();
        commands = new List<IEditorCommand>();
        inputFeatures = new List<IInputFeatureExtension>();
        actions = new Dictionary<string, IEditorAction>();
        decoExtensions = new List<DecoExtension>();
        parts = CalculateParts();
        lines = CalculateLines();
    }

    public string Document => documentEventStream.Document;
    public Selection Selection => documentEventStream.Selection;
    public string NoteId { get; private set; }
    public IReadOnlyList<DocumentLine> Lines => lines;
    public IReadOnlyCollection<ClassMappingExtension> ClassMappings => classMappings;

    public event Func<EditorState, DocumentChangedArgs, Task>? DocumentChanged;

    public async Task UndoAsync()
    {
        var oldDocument = Document;
        var oldLines = lines;
        var originalSelection = Selection;

        await documentEventStream.UndoLatestEvent(inputHandler);
        parts = CalculateParts();
        lines = CalculateLines();

        DocumentChanged?.Invoke(this, new DocumentChangedArgs(Document, oldDocument, lines, oldLines, Selection,
            originalSelection, new List<DocumentChange>(), InputOrigin.System, CancellationToken.None));
    }

    public async Task RedoAsync()
    {
        var oldDocument = Document;
        var oldLines = lines;
        var originalSelection = Selection;

        await documentEventStream.RedoLatestEvent(inputHandler);
        parts = CalculateParts();
        lines = CalculateLines();

        DocumentChanged?.Invoke(this, new DocumentChangedArgs(Document, oldDocument, lines, oldLines, Selection,
            originalSelection, new List<DocumentChange>(), InputOrigin.System, CancellationToken.None));
    }

    public async Task<(IReadOnlyCollection<Node>, IReadOnlyList<DocumentLine>)> GetNodesAsync(int fromLine, int count)
    {
        var linesToParse = lines.Skip(fromLine).Take(count).ToList();

        if (languageExtension is null)
        {
            return (ArraySegment<Node>.Empty, linesToParse);
        }

        var parser = languageExtension!.GetParser();
        var cachedNodes = new List<Node>();

        var watch = Stopwatch.StartNew();
        var linesToRemove = linesToParse.Select(l =>
        {
            var nodes = nodeCache.GetNodes(l.From, l.To);
            if (nodes.Count == 0) return null;

            cachedNodes.AddRange(nodes);
            return l;
        }).Where(l => l is not null).ToList();
        watch.Stop();
        Log.Debug("Got {CachedNodesCount} cached nodes in {WatchElapsedMilliseconds}ms", cachedNodes.Count,
            watch.ElapsedMilliseconds);

        watch.Restart();
        var actualLinesToParse = linesToParse.Except(linesToRemove).Where(l => l!.To > l.From).ToList();
        var chunksToParse = parts.Where(p => actualLinesToParse.Any(l => l!.Intersects(p))).ToList();

        var nodes = await parser.ParseDocumentParts(Document, chunksToParse);
        watch.Stop();
        Log.Debug("Parsed {NodesCount} nodes in {WatchElapsedMilliseconds}ms", nodes.Count, watch.ElapsedMilliseconds);

        watch.Restart();
        nodeCache.AddOrReplaceNodes(nodes);
        watch.Stop();
        Log.Debug("Added {NodesCount} new nodes to cache in {WatchElapsedMilliseconds}ms", nodes.Count,
            watch.ElapsedMilliseconds);

        var combinedNodes = nodes.Concat(cachedNodes).ToList();
        var orderedNodes = combinedNodes.OrderBy(n => n.To).ThenBy(node => node.To - node.From).ToList();

        return (orderedNodes, linesToParse);
    }

    private async Task ApplyUpdate(DocumentEventResult eventResult, string originalDocument,
        Selection originalSelection, InputOrigin origin,
        CancellationToken cancellationToken = default,
        bool notify = true)
    {
        var oldParts = parts;
        var oldDocument = Document;
        var oldLines = lines;
        parts = CalculateParts();
        lines = CalculateLines();

        await ApplyChangesToCache(eventResult.Changes, oldParts);

        if (notify)
            DocumentChanged?.Invoke(this, new DocumentChangedArgs(Document, oldDocument, lines, oldLines,
                eventResult.Selection,
                originalSelection, eventResult.Changes, origin, cancellationToken));
    }

    private Task ApplyChangesToCache(IEnumerable<DocumentChange> changes,
        IReadOnlyCollection<DocumentPart> oldParts)
    {
        foreach (var change in changes)
        {
            var watch = Stopwatch.StartNew();
            if (change.ChangeType is ChangeType.Inserted or ChangeType.Modified)
            {
                var chunksToParse = oldParts.Where(p => change.Intersects(p)).ToList();

                Log.Debug("Found {Count} parts of change in {WatchElapsedMilliseconds}ms", chunksToParse.Count,
                    watch.ElapsedMilliseconds);
                watch.Restart();
                foreach (var l in chunksToParse)
                {
                    nodeCache.RemoveRange(l.From, l.To);
                }

                Log.Debug("Removed {Count} lines of change from the cache in {WatchElapsedMilliseconds}ms",
                    chunksToParse.Count, watch.ElapsedMilliseconds);
                watch.Restart();
                var res = nodeCache.ShiftNodes(change.To, change.CharacterShift);
                Log.Debug("Shifted {Res} nodes in the cache in {WatchElapsedMilliseconds}ms", res,
                    watch.ElapsedMilliseconds);
                watch.Stop();
            }
            else
            {
                nodeCache.Clear();
            }
        }

        return Task.CompletedTask;
    }

    public void AddExtension(IEditorExtension extension)
    {
        switch (extension)
        {
            case LanguageExtension language:
                languageExtension = language;
                break;
            case ClassMappingExtension classMapping:
                classMappings.Add(classMapping);
                break;
            case EditorCommandExtension command:
                commands.Add(command);
                break;
            case DecoExtension decoExtension:
                decoExtensions.Add(decoExtension);
                break;
            case ActionExtension act:
                actions.Add(act.Identifier, act);
                break;
            case IInputFeatureExtension inputFeature:
                inputFeatures.Add(inputFeature);
                inputHandler.SetInputFeatureExtensions(inputFeatures);
                break;
            default:
                Log.Warning("Unknown extension type {ExtensionType}", extension.GetType());
                break;
        }
    }

    // TODO cache this
    public string GetClassString(Node node)
    {
        var mappings = string.Join(' ',
            classMappings.Where(mapping => mapping.Node == node.NodeType).Select(m => m.ClassString));
        return mappings;
    }

    // TODO cache this
    public IReadOnlyCollection<Decoration> GetDecorations(Node node)
    {
        var decorations = new List<Decoration>();
        foreach (var extension in decoExtensions)
        {
            decorations.AddRange(extension.EnterNode(this, node));
        }

        return decorations;
    }

    // TODO dont split the entire document. Only the parts that are affected by a change. Keep the rest
    private List<DocumentPart> CalculateParts()
    {
        return documentSplitter.SplitDocument(Document);
    }

    private List<DocumentLine> CalculateLines()
    {
        return parts.SelectMany(p => p.Lines).ToList();
    }

    internal async Task ApplyInput(InputType inputType, InputOrigin origin, InputData data, bool notify = true,
        CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        if (inputType is InputType.InsertText or InputType.InsertTab)
        {
            foreach (var cmd in commands)
            {
                var pressedKey = inputType == InputType.InsertText ? data.Data! : "Tab";
                if (!cmd.Matches(data.Modifiers, pressedKey)) continue;

                if (actions.TryGetValue(cmd.ActionIdentifier, out var action))
                {
                    Log.Logger.Information("Executing action {ActionIdentifier}", cmd.ActionIdentifier);
                    await action.ExecuteAsync(this);

                    return;
                }
            }
        }

        var originalDocument = Document;
        var originalSelection = Selection;

        var inputEvent = new InputEvent(inputType, data, DateTime.Now, ArraySegment<InputSideEffect>.Empty);
        var update = await documentEventStream.ApplyEventAsync(inputEvent, inputHandler);

        await ApplyUpdate(update, originalDocument, originalSelection, origin, cancellationToken, notify);
        watch.Stop();
        Log.Debug("Applied input in {WatchElapsedMilliseconds}ms", watch.ElapsedMilliseconds);
    }

    internal async Task ApplySelection(int start, int end, InputOrigin origin, bool notifyListeners = true)
    {
        var oldSelection = Selection;

        var selection = new Selection(start, end);

        var inputEvent = new InputEvent(InputType.ChangeSelection, new SelectionInputData(selection), DateTime.Now,
            ArraySegment<InputSideEffect>.Empty);
        var update = await documentEventStream.ApplyEventAsync(inputEvent, inputHandler);

        if (notifyListeners)
            DocumentChanged?.Invoke(this, new DocumentChangedArgs(Document, Document, lines, lines, selection,
                oldSelection,
                Array.Empty<DocumentChange>(), origin, CancellationToken.None));
    }

    public IReadOnlyCollection<Node> GetNodesInRange(IRange range)
    {
        return nodeCache.GetNodes(range.From, range.To);
    }
}