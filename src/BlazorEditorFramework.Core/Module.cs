using BlazorEditorFramework.Core.Editor.Caching;
using BlazorEditorFramework.Core.Editor.Caching.Abstractions;
using BlazorEditorFramework.Core.Editor.Input;
using BlazorEditorFramework.Core.Editor.Input.Abstractions;
using BlazorEditorFramework.Core.Editor.Parsing;
using BlazorEditorFramework.Core.Editor.Parsing.Abstractions;
using BlazorEditorFramework.Core.Editor.Render;
using BlazorEditorFramework.Core.Editor.Render.Abstractions;
using BlazorEditorFramework.Core.StateFactory;
using BlazorEditorFramework.Core.StateFactory.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorEditorFramework.Core;

public static class Module
{
    public static IServiceCollection AddBlazorEditorFramework(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IEditorStateFactory, DefaultEditorStateFactory>();
        serviceCollection.AddScoped<INodeCache, NodeCache>();
        serviceCollection.AddScoped<IDocumentSplitter, DefaultDocumentSplitter>();
        serviceCollection.AddScoped<IInputHandler, InputHandler>();
        serviceCollection.AddScoped<IRowCreator, RowCreator>();

        return serviceCollection;
    }
}