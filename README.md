# About

**BlazorEditorFramework** is a framework for building advanced language editors in Blazor WebAssembly and Blazor Hybrid.

# Features

- **Css Class Mapping**: Map parsed nodes from your language to css classes. Useful for syntax highlighting
- **Decorations**: Decorate nodes with custom Blazor Components
- **Highly performant**: Only updates the parts of the document that have changed, to enable a smooth

# Examples

TODO

# How to use

Register the services in your `Program.cs` file:
```csharp
builder.Services.AddBlazorEditorFramework();
```

Inject the `IEditorStateFactory` in your component:
```csharp
 [Inject] IEditorStateFactory StateFactory { get; set; } = default!;
```

Create a new `EditorState` in your component:
```csharp
var doc = "Hello World!";

state = StateFactory.Build(doc);
```

Add classmappings, decoration and language support:
```csharp
state.AddExtension(new ExampleLanguageExtension(new ExampleParser()));
state.AddExtension(new ClassMappingExtension("even", "word-even"));
state.AddExtension(new ClassMappingExtension("odd", "word-odd"));
state.AddExtension(new TooltipDecoration());
```

Render the `EditorView` component with the newly created `EditorState`:
```html
<EditorView EditorState="state"></EditorView>
```

# Extensions
## Language Support
Implement `LanguageExtension` and provide an implementation for `ILanguageParser` to add language support to the editor.

### ILanguageParser
Interface for parsing a list of parts from a document to a collection of language 'nodes' (e.g. Keywords, strings, comments, etc) <br>
See `ExampleParser` for an example implementation.

## Class Mapping
A simple class to map between node types parsed by your parser and css class names.

## Decorations
Decorations are used to decorate nodes with custom Blazor Components. <br>
Decorations can replace an existing node, or be rendered before/after the node. <br>
See `TooltipDecoration` for an example implementation.