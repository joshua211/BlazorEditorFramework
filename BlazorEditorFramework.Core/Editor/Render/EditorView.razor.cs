using System.Diagnostics;
using BlazorEditorFramework.Core.Editor.Extensions;
using BlazorEditorFramework.Core.Editor.Input;
using BlazorEditorFramework.Core.Editor.Render.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorEditorFramework.Core.Editor.Render;

public partial class EditorView : IDisposable
{
    private CancellationTokenSource cancellationTokenSource = new();
    private List<Decoration> decorations = new();
    private bool hasPendingSelectionUpdate = false;
    private List<Row> rows = new();
    private DotNetObjectReference<EditorView> viewReference = default!;
    private Virtualize<Row> virtualize = default!;

    [Parameter] public EditorState EditorState { get; set; } = null!;

    [Inject] public IJSRuntime JsRuntime { get; private set; } = default!;
    [Inject] public IRowCreator RowCreator { get; set; } = default!;
    [Inject] public ILogger<EditorView> Logger { get; set; } = default!;

    public void Dispose()
    {
        viewReference.Dispose();
    }

    public async Task Refresh()
    {
        await virtualize.RefreshDataAsync();
        StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        EditorState.DocumentChanged += OnDocumentChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            viewReference = DotNetObjectReference.Create(this);
            await JsRuntime.InvokeVoidAsync("setupEditor", "editor", viewReference);
        }
    }

    private async Task OnDocumentChanged(EditorState _, DocumentChangedArgs args)
    {
        await RefreshDataAsync(args, args.CancellationToken);
    }
    
    private async Task RefreshDataAsync(DocumentChangedArgs args, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        var watch = Stopwatch.StartNew();
        await virtualize.RefreshDataAsync();
        StateHasChanged();

        if (cancellationToken.IsCancellationRequested)
            return;

        // TODO keep track on view side if we should move the cursor or not
        if (args.Changes.Any() || (args.OldSelection != args.NewSelection && args.Origin != InputOrigin.Input))
        {
            Logger.LogDebug("Move selection to {SelectionStart} - {SelectionEnd}", EditorState.Selection.From,
                EditorState.Selection.To);
            hasPendingSelectionUpdate = true;
            var jsWatch = Stopwatch.StartNew();
            await JsRuntime.InvokeVoidAsync("setSelection", cancellationToken, EditorState.Selection.From,
                EditorState.Selection.To);
            jsWatch.Stop();
            Logger.LogDebug("JS setSelection in {JsWatchElapsedMilliseconds}ms", jsWatch.ElapsedMilliseconds);
        }

        watch.Stop();
        Logger.LogDebug("Refreshed data in {WatchElapsedMilliseconds}ms total", watch.ElapsedMilliseconds);
    }

    private async ValueTask<ItemsProviderResult<Row>> GetRowsAsync(ItemsProviderRequest request)
    {
        var watch = Stopwatch.StartNew();
        var (nodes, lines) = await EditorState.GetNodesAsync(request.StartIndex, request.Count);
        var calculatedRows = RowCreator.CalculateRows(lines, nodes, EditorState);

        var result = new ItemsProviderResult<Row>(calculatedRows, EditorState.Lines.Count);
        watch.Stop();
        Logger.LogDebug("GetRowsAsync in {WatchElapsedMilliseconds}ms total", watch.ElapsedMilliseconds);

        return result;
    }

    /*private void CalculateRows(string document, IReadOnlyCollection<Node> nodes,
        IReadOnlyList<DocumentLine> newLines, IReadOnlyList<DocumentLine>? oldLines = null,
        IReadOnlyCollection<DocumentChange>? changes = null)
    {
        Console.WriteLine($"Calculate rows at {DateTime.Now.Millisecond}");
        var watch = new Stopwatch();
        watch.Start();

        if (oldLines is null || changes is null)
        {
            rows.Clear();

            foreach (var line in newLines)
            {
                rows.Add(new Row(line.Content(EditorState), line.From, line.To, EditorState, nodes));
            }
        }
        else
        {
            // 1. find which lines have changed
            var changedLines = changes.Where(c => c.ChangeType == ChangeType.Modified).SelectMany(c =>
            {
                var linesWithChanges = new List<DocumentLine?>();
                var list = oldLines.ToList();
                linesWithChanges.Add(list.GetLineContainingPosition(c.From));

                if (c.From != c.To)
                    linesWithChanges.Add(list.GetLineContainingPosition(c.To));

                return linesWithChanges;
            });

            foreach (var line in changedLines)
            {
                var newLine = newLines.GetLineContainingPosition(line!.From);
                rows[line!.Index] = new Row(newLine!.Content(EditorState), newLine.From, newLine.To, EditorState,
                    nodes);
            }
        }

        Console.WriteLine($"Calculation done at {DateTime.Now.Millisecond} in {watch.ElapsedMilliseconds}ms");

        StateHasChanged();
    }*/

    [JSInvokable]
    public async Task HandleInput(string inputType, string data, byte[]? byteData, bool isCtrlPressed,
        bool isShiftPressed, bool isAltPressed, bool isMetaPressed)
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource = new CancellationTokenSource();
        Console.WriteLine("HandleInput: " + inputType);
        var typedInput = inputType switch
        {
            "insertText" => InputType.InsertText,
            "deleteContentBackward" => InputType.DeleteContentBackward,
            "deleteContentForward" => InputType.DeleteContentForward,
            "deleteWordBackward" => InputType.DeleteWordBackward,
            "deleteWordForward" => InputType.DeleteWordForward,
            "insertFromPaste" => InputType.InsertFromPaste,
            "insertParagraph" => InputType.InsertParagraph,
            "insertLineBreak" => InputType.InsertLineBreak,
            "insertTab" => InputType.InsertTab,
            _ => throw new InvalidOperationException("Invalid input type " + inputType)
        };

        var modifiers = new List<string>();
        if (isAltPressed)
            modifiers.Add(ModifierKeys.Alt);
        if (isCtrlPressed)
            modifiers.Add(ModifierKeys.Control);
        if (isShiftPressed)
            modifiers.Add(ModifierKeys.Shift);
        if (isMetaPressed)
            modifiers.Add(ModifierKeys.Meta);

        InputData inputData;
        if (byteData is not null)
        {
            var stream = new MemoryStream(byteData);
            inputData = new StreamInputData(stream);
            Logger.LogDebug("Received byte data with length {Length}", byteData.Length);
        }
        else
        {
            inputData = new InputData(data, modifiers);
        }

        await EditorState.ApplyInput(typedInput, InputOrigin.Input, inputData, true, cancellationTokenSource.Token);
    }

    [JSInvokable]
    public async Task HandleSelectionChange(int from, int to)
    {
        if (hasPendingSelectionUpdate)
        {
            hasPendingSelectionUpdate = false;
            return;
        }

        await EditorState.ApplySelection(from, to, InputOrigin.Input);
        StateHasChanged();
    }


    [JSInvokable]
    public async Task HandleKeyUp(string key, bool alt, bool ctrl, bool shift)
    {
        var modifiers = new List<string>();
        if (alt)
            modifiers.Add(ModifierKeys.Alt);
        if (ctrl)
            modifiers.Add(ModifierKeys.Control);
        if (shift)
            modifiers.Add(ModifierKeys.Shift);

        if (modifiers.Contains(key))
            return;

        var inputType = key switch
        {
            "Tab" => InputType.InsertTab,
            "Enter" => InputType.InsertParagraph,
            "Backspace" => InputType.DeleteContentBackward,
            "Delete" => InputType.DeleteContentForward,
            _ => InputType.InsertText
        };

        var data = new InputData(key, modifiers);
        await EditorState.ApplyInput(inputType, InputOrigin.Input, data, true);
        StateHasChanged();
    }
}