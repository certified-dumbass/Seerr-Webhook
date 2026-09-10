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
        serviceCollection.AddHttpClient();

        serviceCollection.AddSingleton<DiscordWebhookService>();
        serviceCollection.AddSingleton<MessageTemplateService>();

        serviceCollection.AddSingleton<JellyfinUserSyncService>();

        serviceCollection.AddHostedService(
            serviceProvider =>
                serviceProvider.GetRequiredService<JellyfinUserSyncService>());
    }
}