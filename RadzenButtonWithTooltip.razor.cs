using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Threading.Tasks;
using System;
using Microsoft.JSInterop;

namespace Radzen;

public partial class RadzenButtonWithTooltip : RadzenComponent
{
    [Parameter]
    public string? Link { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> Click { get; set; }

    [Parameter]
    public string? BusyText { get; set; }

    [Parameter]
    public ButtonSize Size { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public ButtonStyle ButtonStyle { get; set; }

    [Parameter]
    public ButtonType ButtonType { get; set; }

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool IsBusy { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public Variant Variant { get; set; }

    [Parameter]
    public string? TooltipText { get; set; }

    [Parameter]
    public int? TooltipDelay { get; set; }

    /// <summary>
    /// Default to int.MaxValue.
    /// </summary>
    [Parameter]
    public int? TooltipDuration { get; set; } = int.MaxValue;

    [Parameter]
    public RenderFragment? TooltipContent { get; set; }

    /// <summary>
    /// Default to TooltipPosition.Top.
    /// </summary>
    [Parameter]
    public TooltipPosition TooltipPosition { get; set; } = TooltipPosition.Top;

    [Parameter]
    public bool CloseTooltipOnDocumentClick { get; set; } = true;

    [Parameter]
    public string? TooltipStyle { get; set; }

    [Parameter]
    public string? TooltipCssClass { get; set; }

    [Parameter]
    public bool HideTooltipOnMouseLeave { get; set; } = true;

    /// <summary>
    /// Default to 'Open link in a new tab'.
    /// </summary>
    [Parameter]
    public string? OpenLinkInANewTabText { get; set; } = "Open link in a new tab";

    /// <summary>
    /// Default to 'Copy link'.
    /// </summary>
    [Parameter]
    public string? CopyLinkText { get; set; } = "Copy link";

    [Inject]
    public NavigationManager NM { get; set; } = null!;

    async Task InternalClick(MouseEventArgs args)
    {
        if (args.Button == 0)
        {
            HideTooltip();
            await Click.InvokeAsync(args);
        }
        else if (!string.IsNullOrWhiteSpace(Link) && args.Button == 1)
        {
            await JS.InvokeVoidAsync("window.open", Link, "_blank");
        }
    }

    void ShowContextMenuWithItems(MouseEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(Link)) return;

        CMS.Open(
            args,
            [
                new ContextMenuItem() { Text = OpenLinkInANewTabText, Value = 1, Icon = "tab" },
                new ContextMenuItem() { Text = CopyLinkText, Value = 2, Icon = "link" }
            ],
            OnMenuItemClick);
    }

    async void OnMenuItemClick(MenuItemEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(Link)) return;

        try
        {
            if (args.Value.Equals(1))
            {
                await JS.InvokeVoidAsync("window.open", Link, "_blank");
            }
            else if (args.Value.Equals(2))
            {
                var link = NM.ToAbsoluteUri(Link);
                await JS.InvokeVoidAsync("navigator.clipboard.writeText", link.ToString());
            }
        }
        catch (Exception)
        {
            // Normal behavior
        }

        CMS.Close();
    }

    private void ShowTooltip(ElementReference elementReference)
    {
        if (!string.IsNullOrWhiteSpace(TooltipText))
        {
            try
            {
                TS.Open(elementReference, TooltipText, new TooltipOptions
                {
                    Position = TooltipPosition,
                    Delay = TooltipDelay,
                    Duration = TooltipDuration,
                    CloseTooltipOnDocumentClick = CloseTooltipOnDocumentClick,
                    Style = TooltipStyle,
                    CssClass = TooltipCssClass,
                    Text = TooltipText,
                });
            }
            catch
            {
                // Normal behavior
            }
        }
    }

    private void HideTooltip(ElementReference? elementReference = default)
    {
        if (HideTooltipOnMouseLeave)
        {
            try
            {
                TS.Close();
            }
            catch
            {
                // Normal behavior
            }
        }
    }

    public new void Dispose()
    {
        TS?.Close();
    }
}