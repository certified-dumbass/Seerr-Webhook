using Dreamstreaming.SeerrDiscord.Models;

namespace Dreamstreaming.SeerrDiscord.Services;

public class MessageTemplateService
{
    public string Format(
        string template,
        SeerrWebhookPayload payload,
        string discordUserId = "")
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return string.Empty;
        }

        var mention =
            string.IsNullOrWhiteSpace(discordUserId)
                ? payload.RequestedBy
                : $"<@{discordUserId}>";

        var title =
            payload.GetDisplayTitle();

        return template
            .Replace(
                "{user}",
                payload.RequestedBy,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{mention}",
                mention,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{discordUserId}",
                discordUserId,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{title}",
                title,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{mediaType}",
                payload.MediaType,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{requestId}",
                payload.RequestId,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{jellyfinUserId}",
                payload.JellyfinUserId,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{mediaStatus}",
                payload.MediaStatus,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{jellyfinMediaId}",
                payload.JellyfinMediaId,
                StringComparison.OrdinalIgnoreCase);
    }
}