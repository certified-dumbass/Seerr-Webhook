using Dreamstreaming.SeerrDiscord.Models;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Dreamstreaming.SeerrDiscord.Services;

public class JellyfinUserSyncService
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
            _logger.LogWarning(
                "Seerr Discord plugin instance is not available.");

            return 0;
        }

        var configuration =
            plugin.Configuration;

        configuration.UserMappings ??=
            new List<UserDiscordMapping>();

        var jellyfinUsers =
            _userManager
                .Users
                .ToList();

        var addedUsers = 0;
        var changed = false;

        foreach (var jellyfinUser in jellyfinUsers)
        {
            var existingMapping =
                configuration.UserMappings
                    .FirstOrDefault(x =>
                        x.JellyfinUserId ==
                        jellyfinUser.Id);

            if (existingMapping is null)
            {
                configuration.UserMappings.Add(
                    new UserDiscordMapping
                    {
                        JellyfinUserId =
                            jellyfinUser.Id,

                        JellyfinUsername =
                            jellyfinUser.Username,

                        DiscordUserId =
                            string.Empty,

                        Enabled =
                            true
                    });

                addedUsers++;
                changed = true;

                _logger.LogInformation(
                    "Imported Jellyfin user {Username} ({UserId}) into Seerr Discord mappings.",
                    jellyfinUser.Username,
                    jellyfinUser.Id);

                continue;
            }

            // Keep an existing Discord mapping intact.
            // Only update the displayed Jellyfin username
            // when the Jellyfin account has been renamed.
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
            plugin.UpdateConfiguration(
                configuration);
        }

        _logger.LogInformation(
            "Jellyfin user import completed. {AddedUsers} new user(s) added.",
            addedUsers);

        return addedUsers;
    }
}