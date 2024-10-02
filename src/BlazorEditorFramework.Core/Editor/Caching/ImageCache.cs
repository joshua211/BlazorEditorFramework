using BlazorEditorFramework.Core.Editor.Caching.Abstractions;
using Microsoft.JSInterop;

namespace BlazorEditorFramework.Core.Editor.Caching;

// TODO maybe make this thread safe?
public class ImageCache : IImageCache
{
    private readonly Dictionary<string, string> cache;
    private readonly IJSRuntime jsRuntime;

    public ImageCache(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
        cache = new();
    }

    public async ValueTask<string> GetObjectUrlAsync(string url)
    {
        var normalizedUrl = ToNormalizedUrl(url);
        if (cache.TryGetValue(normalizedUrl, out var cachedUrl))
        {
            return cachedUrl;
        }

        await using var stream = File.Open(normalizedUrl, FileMode.Open, FileAccess.Read, FileShare.Read);
        var dotnetImageStream = new DotNetStreamReference(stream);

        var objectUrl = await jsRuntime.InvokeAsync<string>("createObjectUrl", dotnetImageStream);
        cache[normalizedUrl] = objectUrl;
        dotnetImageStream.Dispose();

        return objectUrl;
    }

    private string ToNormalizedUrl(string url)
    {
        return Path.GetFullPath(url);
    }
}