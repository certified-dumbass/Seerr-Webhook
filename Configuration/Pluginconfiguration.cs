using Dreamstreaming.SeerrDiscord.Models;
using MediaBrowser.Model.Plugins;

namespace Dreamstreaming.SeerrDiscord.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public PluginConfiguration()
    {
        PluginEnabled = true;

        SeerrWebhookToken = string.Empty;

        IncomingRequestEnabled = true;
        IncomingRequestWebhook = string.Empty;
        IncomingRequestEmoji = "🎬";
        IncomingRequestTitle = "Nieuwe request";
        IncomingRequestMessage = "{user} heeft **{title}** aangevraagd.";

        AvailableRequestEnabled = true;
        AvailableRequestWebhook = string.Empty;
        AvailableRequestEmoji = "✅";
        AvailableRequestTitle = "Request beschikbaar";
        AvailableRequestMessage =
            "{mention} jouw request **{title}** is nu beschikbaar!";

        MentionDiscordUser = true;

        SendAvailableWithoutMapping = true;

        ShowPoster = true;
        ShowMediaType = true;
        UseEmbeds = true;

        UserMappings = new List<UserDiscordMapping>();
    }

    public bool PluginEnabled { get; set; }

    public string SeerrWebhookToken { get; set; }

    // Incoming requests

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

    // Users

    public List<UserDiscordMapping> UserMappings { get; set; }
}