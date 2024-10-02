namespace BlazorEditorFramework.Core.Editor.Extensions.BaseTypes;

public class ClassMappingExtension : Extension
{
    public ClassMappingExtension(string node, string classString)
    {
        Node = node;
        ClassString = classString;
    }

    public string Node { get; private set; }
    public string ClassString { get; private set; }
}