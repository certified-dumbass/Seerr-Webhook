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
    /// Imports Jellyfin users into the plugin configuration.
    /// Existing Discord mappings are preserved.
    /// </summary>
    /// <returns>The import result.</returns>
    [HttpPost("ImportUsers")]
    [HttpPost("SyncUsers")]
    public ActionResult ImportUsers()
    {
        try
        {
            var addedUsers =
                _userSyncService.SyncUsers();

            var totalUsers =
                Plugin.Instance?
                    .Configuration
                    .UserMappings?
                    .Count
                ?? 0;

            return Ok(new
            {
                success = true,
                addedUsers,
                totalUsers,
                message =
                    $"{addedUsers} new Jellyfin user(s) imported."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                new
                {
                    success = false,
                    message =
                        "Failed to import Jellyfin users.",
                    error =
                        ex.Message
                });
        }
    }

    /// <summary>
    /// Returns all currently imported Jellyfin users
    /// and their Discord mappings.
    /// </summary>
    /// <returns>The imported user mappings.</returns>
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

        configuration.UserMappings ??=
            new();

        var importedMappings =
            configuration.UserMappings
                .OrderBy(x =>
                    x.JellyfinUsername)
                .Select(mapping =>
                    new
                    {
                        jellyfinUserId =
                            mapping.JellyfinUserId
                                .ToString(),

                        jellyfinUsername =
                            mapping.JellyfinUsername,

                        discordUserId =
                            mapping.DiscordUserId
                            ?? string.Empty,

                        enabled =
                            mapping.Enabled
                    })
                .ToList();

        return Ok(importedMappings);
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
            configuration.IncomingRequestEmoji?.Trim()
            ?? string.Empty;

        var title =
            configuration.IncomingRequestTitle?.Trim()
            ?? string.Empty;

        var header =
            BuildHeader(
                emoji,
                title);

        var message =
            $"{header}\n\n" +
            "TestUser requested **Test Movie**.";

        await _discordWebhookService.SendMessageAsync(
            configuration.IncomingRequestWebhook,
            message,
            cancellationToken);

        return Ok(new
        {
            success = true,
            message =
                "Incoming request test message sent."
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
            configuration.AvailableRequestEmoji?.Trim()
            ?? string.Empty;

        var title =
            configuration.AvailableRequestTitle?.Trim()
            ?? string.Empty;

        var header =
            BuildHeader(
                emoji,
                title);

        var message =
            $"{header}\n\n" +
            "TestUser, your request **Test Movie** is now available!";

        await _discordWebhookService.SendMessageAsync(
            configuration.AvailableRequestWebhook,
            message,
            cancellationToken);

        return Ok(new
        {
            success = true,
            message =
                "Available request test message sent."
        });
    }

    /// <summary>
    /// Creates a clean message header from an optional emoji and title.
    /// </summary>
    private static string BuildHeader(
        string emoji,
        string title)
    {
        if (string.IsNullOrWhiteSpace(emoji))
        {
            return title;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return emoji;
        }

        return $"{emoji} {title}";
    }
}