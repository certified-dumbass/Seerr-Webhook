using Dreamstreaming.SeerrDiscord.Services;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dreamstreaming.SeerrDiscord.Controllers;

[ApiController]
[Route("Dreamstreaming/SeerrDiscord")]
[Authorize(Policy = "RequiresElevation")]
public class SeerrWebhookController : ControllerBase
{
    private readonly JellyfinUserSyncService _userSyncService;
    private readonly IUserManager _userManager;
    private readonly DiscordWebhookService _discordWebhookService;

    public SeerrWebhookController(
        JellyfinUserSyncService userSyncService,
        IUserManager userManager,
        DiscordWebhookService discordWebhookService)
    {
        _userSyncService = userSyncService;
        _userManager = userManager;
        _discordWebhookService = discordWebhookService;
    }

    /// <summary>
    /// Synchronizes Jellyfin users with the plugin configuration.
    /// </summary>
    /// <returns>The synchronization result.</returns>
    [HttpPost("SyncUsers")]
    public ActionResult SyncUsers()
    {
        var addedUsers = _userSyncService.SyncUsers();

        return Ok(new
        {
            success = true,
            addedUsers
        });
    }

    /// <summary>
    /// Returns all Jellyfin users and their Discord mappings.
    /// </summary>
    /// <returns>The Jellyfin users.</returns>
    [HttpGet("Users")]
    public ActionResult GetUsers()
    {
        var configuration =
            Plugin.Instance?.Configuration;

        if (configuration is null)
        {
            return StatusCode(
                500,
                "Plugin configuration is not available.");
        }

        // Jellyfin 10.11.8 exposes users through IUserManager.Users.
        var jellyfinUsers =
            _userManager.Users;

        var users = jellyfinUsers
            .Select(user =>
            {
                var mapping =
                    configuration.UserMappings
                        .FirstOrDefault(x =>
                            x.JellyfinUserId == user.Id);

                return new
                {
                    jellyfinUserId =
                        user.Id.ToString(),

                    jellyfinUsername =
                        user.Username,

                    discordUserId =
                        mapping?.DiscordUserId
                        ?? string.Empty,

                    enabled =
                        mapping?.Enabled
                        ?? true
                };
            })
            .OrderBy(x =>
                x.jellyfinUsername)
            .ToList();

        return Ok(users);
    }

    /// <summary>
    /// Sends a test message to the incoming request Discord webhook.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>The test result.</returns>
    [HttpPost("TestIncoming")]
    public async Task<ActionResult> TestIncoming(
        CancellationToken cancellationToken)
    {
        var configuration =
            Plugin.Instance?.Configuration;

        if (configuration is null)
        {
            return StatusCode(
                500,
                "Plugin configuration is not available.");
        }

        if (!configuration.PluginEnabled)
        {
            return BadRequest(
                "The Seerr Discord plugin is disabled.");
        }

        if (string.IsNullOrWhiteSpace(
                configuration.IncomingRequestWebhook))
        {
            return BadRequest(
                "Incoming request Discord webhook is not configured.");
        }

        var emoji =
            configuration.IncomingRequestEmoji;

        var title =
            configuration.IncomingRequestTitle;

        var message =
            $"{emoji} {title}\n\n" +
            "TestUser heeft **Test Movie** aangevraagd.";

        await _discordWebhookService.SendMessageAsync(
            configuration.IncomingRequestWebhook,
            message,
            cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Incoming request test message sent."
        });
    }

    /// <summary>
    /// Sends a test message to the available request Discord webhook.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>The test result.</returns>
    [HttpPost("TestAvailable")]
    public async Task<ActionResult> TestAvailable(
        CancellationToken cancellationToken)
    {
        var configuration =
            Plugin.Instance?.Configuration;

        if (configuration is null)
        {
            return StatusCode(
                500,
                "Plugin configuration is not available.");
        }

        if (!configuration.PluginEnabled)
        {
            return BadRequest(
                "The Seerr Discord plugin is disabled.");
        }

        if (string.IsNullOrWhiteSpace(
                configuration.AvailableRequestWebhook))
        {
            return BadRequest(
                "Available request Discord webhook is not configured.");
        }

        var emoji =
            configuration.AvailableRequestEmoji;

        var title =
            configuration.AvailableRequestTitle;

        var message =
            $"{emoji} {title}\n\n" +
            "TestUser, jouw request **Test Movie** is nu beschikbaar!";

        await _discordWebhookService.SendMessageAsync(
            configuration.AvailableRequestWebhook,
            message,
            cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Available request test message sent."
        });
    }
}