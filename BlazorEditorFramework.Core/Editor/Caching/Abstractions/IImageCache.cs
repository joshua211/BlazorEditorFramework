namespace BlazorEditorFramework.Core.Editor.Caching.Abstractions;

public interface IImageCache
{
    ValueTask<string> GetObjectUrlAsync(string url);
}