using Dreamstreaming.SeerrDiscord.Models;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dreamstreaming.SeerrDiscord.Services;

public class JellyfinUserSyncService : BackgroundService
{
    private readonly IUserManager _userManager;
    private readonly ILogger<JellyfinUserSyncService> _logger;

    public JellyfinUserSyncService(
        IUserManager userManager,
        ILogger<JellyfinUserSyncService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public int SyncUsers()
    {
        var plugin = Plugin.Instance;

        if (plugin is null)
        {
            return 0;
        }

        var configuration = plugin.Configuration;

        configuration.UserMappings ??= new List<UserDiscordMapping>();

        // Jellyfin 10.11.8
        var jellyfinUsers = _userManager
            .Users
            .ToList();

        var addedUsers = 0;
        var changed = false;

        foreach (var jellyfinUser in jellyfinUsers)
        {
            var existingMapping = configuration.UserMappings
                .FirstOrDefault(x =>
                    x.JellyfinUserId == jellyfinUser.Id);

            if (existingMapping is null)
            {
                configuration.UserMappings.Add(
                    new UserDiscordMapping
                    {
                        JellyfinUserId = jellyfinUser.Id,
                        JellyfinUsername = jellyfinUser.Username,
                        DiscordUserId = string.Empty,
                        Enabled = true
                    });

                addedUsers++;
                changed = true;

                _logger.LogInformation(
                    "Added Jellyfin user {Username} ({UserId}) to Seerr Discord mappings.",
                    jellyfinUser.Username,
                    jellyfinUser.Id);

                continue;
            }

            // Als iemand zijn Jellyfin-naam verandert,
            // behouden we de Discord-koppeling en wijzigen
            // we alleen de weergegeven gebruikersnaam.
            if (!string.Equals(
                    existingMapping.JellyfinUsername,
                    jellyfinUser.Username,
                    StringComparison.Ordinal))
            {
                _logger.LogInformation(
                    "Updating Jellyfin username from {OldUsername} to {NewUsername}.",
                    existingMapping.JellyfinUsername,
                    jellyfinUser.Username);

                existingMapping.JellyfinUsername =
                    jellyfinUser.Username;

                changed = true;
            }
        }

        if (changed)
        {
            plugin.UpdateConfiguration(configuration);
        }

        return addedUsers;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        // Direct scannen zodra de plugin/server opstart.
        try
        {
            SyncUsers();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Initial Jellyfin user synchronization failed.");
        }

        // Daarna iedere 30 seconden controleren
        // of er nieuwe Jellyfin-users zijn.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(30),
                    stoppingToken);

                SyncUsers();
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Jellyfin user synchronization failed.");
            }
        }
    }
}