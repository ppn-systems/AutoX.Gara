using ChatApp.Models;

namespace ChatApp.Selectors;

/// <summary>
/// Selects the correct DataTemplate based on whether the message is from the
/// current user (right-aligned) or from someone else (left-aligned).
/// </summary>
public class MessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate MyMessageTemplate { get; set; } = null!;
    public DataTemplate TheirMessageTemplate { get; set; } = null!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        return item is Message { IsFromMe: true }
            ? MyMessageTemplate
            : TheirMessageTemplate;
    }
}
