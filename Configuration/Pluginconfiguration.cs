using Dreamstreaming.SeerrDiscord.Models;
using MediaBrowser.Model.Plugins;

namespace Dreamstreaming.SeerrDiscord.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public PluginConfiguration()
    {
        PluginEnabled = true;

        SeerrWebhookToken = string.Empty;

        // New requests
        IncomingRequestEnabled = true;
        IncomingRequestWebhook = string.Empty;
        IncomingRequestEmoji = "🎬";
        IncomingRequestTitle = "New request";
        IncomingRequestMessage =
            "{user} requested **{title}**.";

        // Available requests
        AvailableRequestEnabled = true;
        AvailableRequestWebhook = string.Empty;
        AvailableRequestEmoji = "✅";
        AvailableRequestTitle = "Request available";
        AvailableRequestMessage =
            "{mention} your request **{title}** is now available!";

        MentionDiscordUser = true;
        SendAvailableWithoutMapping = true;

        // Discord appearance
        ShowPoster = true;
        ShowMediaType = true;
        UseEmbeds = true;

        // User mappings start empty.
        // Jellyfin users are added manually through
        // the "Import Users" button in the configuration page.
        UserMappings = new List<UserDiscordMapping>();
    }

    public bool PluginEnabled { get; set; }

    public string SeerrWebhookToken { get; set; }

    // New requests

    public bool IncomingRequestEnabled { get; set; }

    public string IncomingRequestWebhook { get; set; }

    public string IncomingRequestEmoji { get; set; }

    public string IncomingRequestTitle { get; set; }

    public string IncomingRequestMessage { get; set; }

    // Available requests

    public bool AvailableRequestEnabled { get; set; }

    public string AvailableRequestWebhook { get; set; }

    public string AvailableRequestEmoji { get; set; }

    public string AvailableRequestTitle { get; set; }

    public string AvailableRequestMessage { get; set; }

    public bool MentionDiscordUser { get; set; }

    public bool SendAvailableWithoutMapping { get; set; }

    // Discord appearance

    public bool UseEmbeds { get; set; }

    public bool ShowPoster { get; set; }

    public bool ShowMediaType { get; set; }

    // User mappings

    public List<UserDiscordMapping> UserMappings { get; set; }
}