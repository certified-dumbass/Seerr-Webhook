using System.Text.Json.Serialization;

namespace Dreamstreaming.SeerrDiscord.Models;

public class SeerrWebhookPayload
{
    [JsonPropertyName("notificationType")]
    public string NotificationType { get; set; } = string.Empty;

    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = string.Empty;

    [JsonPropertyName("requestedBy")]
    public string RequestedBy { get; set; } = string.Empty;

    [JsonPropertyName("jellyfinUserId")]
    public string JellyfinUserId { get; set; } = string.Empty;

    [JsonPropertyName("mediaType")]
    public string MediaType { get; set; } = string.Empty;

    [JsonPropertyName("mediaStatus")]
    public string MediaStatus { get; set; } = string.Empty;

    [JsonPropertyName("jellyfinMediaId")]
    public string JellyfinMediaId { get; set; } = string.Empty;

    /// <summary>
    /// Returns the best available title for the media.
    /// </summary>
    public string GetDisplayTitle()
    {
        if (!string.IsNullOrWhiteSpace(Title))
        {
            return Title.Trim();
        }

        if (!string.IsNullOrWhiteSpace(Subject))
        {
            return Subject.Trim();
        }

        return "Unknown title";
    }

    /// <summary>
    /// Returns the notification type supplied by Seerr.
    /// Falls back to the event field when notificationType
    /// is not available.
    /// </summary>
    public string GetNotificationType()
    {
        if (!string.IsNullOrWhiteSpace(NotificationType))
        {
            return NotificationType.Trim();
        }

        if (!string.IsNullOrWhiteSpace(Event))
        {
            return Event.Trim();
        }

        return string.Empty;
    }
}