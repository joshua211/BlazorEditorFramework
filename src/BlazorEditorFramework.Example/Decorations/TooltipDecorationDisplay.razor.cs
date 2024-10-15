using Microsoft.AspNetCore.Components;

namespace BlazorEditorFramework.Example.Decorations;

public partial class TooltipDecorationDisplay : ComponentBase
{
    private bool showTooltip;

    [Parameter] public string Text { get; set; } = string.Empty;
    [Parameter] public bool IsEven { get; set; }
    [Parameter] public int Count { get; set; }

    private void OnMouseEnter()
    {
        showTooltip = true;
    }

    private void OnMouseLeave()
    {
        showTooltip = false;
    }
}