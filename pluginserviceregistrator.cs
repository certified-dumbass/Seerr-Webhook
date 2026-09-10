using Dreamstreaming.SeerrDiscord.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Dreamstreaming.SeerrDiscord;

public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(
        IServiceCollection serviceCollection,
        IServerApplicationHost applicationHost)
    {
        // Required for sending Discord webhook requests.
        serviceCollection.AddHttpClient();

        // Discord webhook handling.
        serviceCollection.AddSingleton<DiscordWebhookService>();

        // Message placeholder/template handling.
        serviceCollection.AddSingleton<MessageTemplateService>();

        // Jellyfin user import service.
        //
        // This is intentionally NOT registered as a hosted service.
        // Jellyfin users are imported manually through the
        // "Import Users" button in the plugin configuration page.
        serviceCollection.AddSingleton<JellyfinUserSyncService>();
    }
}