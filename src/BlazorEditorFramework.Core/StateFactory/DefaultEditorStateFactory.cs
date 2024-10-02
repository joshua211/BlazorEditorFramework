using BlazorEditorFramework.Core.Editor;
using BlazorEditorFramework.Core.Editor.Caching.Abstractions;
using BlazorEditorFramework.Core.Editor.Input.Abstractions;
using BlazorEditorFramework.Core.Editor.Parsing.Abstractions;
using BlazorEditorFramework.Core.StateFactory.Abstractions;

namespace BlazorEditorFramework.Core.StateFactory;

public class DefaultEditorStateFactory : IEditorStateFactory
{
    private readonly IDocumentSplitter documentSplitter;
    private readonly IInputHandler inputHandler;
    private readonly INodeCache nodeCache;

    public DefaultEditorStateFactory(IInputHandler inputHandler, INodeCache nodeCache,
        IDocumentSplitter documentSplitter)
    {
        this.inputHandler = inputHandler;
        this.nodeCache = nodeCache;
        this.documentSplitter = documentSplitter;
    }

    public EditorState Build(string initialDocument, string? editorId = null)
    {
        editorId ??= Guid.NewGuid().ToString();

        return new EditorState(
            initialDocument,
            editorId,
            inputHandler,
            nodeCache,
            documentSplitter
        );
    }
}