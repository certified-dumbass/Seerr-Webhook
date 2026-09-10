using Dreamstreaming.SeerrDiscord.Configuration;
using Dreamstreaming.SeerrDiscord.Models;
using Dreamstreaming.SeerrDiscord.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dreamstreaming.SeerrDiscord.Controllers;

[ApiController]
[Route("Dreamstreaming/SeerrDiscord/Webhook")]
[AllowAnonymous]
public class SeerrReceiverController : ControllerBase
{
    private readonly DiscordWebhookService _discordWebhookService;
    private readonly MessageTemplateService _messageTemplateService;

    public SeerrReceiverController(
        DiscordWebhookService discordWebhookService,
        MessageTemplateService messageTemplateService)
    {
        _discordWebhookService = discordWebhookService;
        _messageTemplateService = messageTemplateService;
    }

    [HttpPost]
    public async Task<ActionResult> ReceiveWebhook(
        [FromBody] SeerrWebhookPayload payload,
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
            return Ok(new
            {
                success = true,
                ignored = true,
                reason = "Plugin is disabled."
            });
        }

        if (!ValidateWebhookToken(
                configuration.SeerrWebhookToken))
        {
            return Unauthorized(
                "Invalid Seerr webhook token.");
        }

        var rawNotificationType =
            payload.GetNotificationType();

        if (string.IsNullOrWhiteSpace(
                rawNotificationType))
        {
            return BadRequest(
                "notificationType or event is missing.");
        }

        var notificationType =
            rawNotificationType
                .Trim()
                .ToUpperInvariant();

        switch (notificationType)
        {
            case "MEDIA_PENDING":
            case "MEDIA_AUTO_APPROVED":
            case "MEDIA_APPROVED":

                await HandleIncomingRequest(
                    configuration,
                    payload,
                    cancellationToken);

                break;

            case "MEDIA_AVAILABLE":

                await HandleAvailableRequest(
                    configuration,
                    payload,
                    cancellationToken);

                break;

            default:

                return Ok(new
                {
                    success = true,
                    ignored = true,
                    notificationType
                });
        }

        return Ok(new
        {
            success = true,
            notificationType
        });
    }

    private async Task HandleIncomingRequest(
        PluginConfiguration configuration,
        SeerrWebhookPayload payload,
        CancellationToken cancellationToken)
    {
        if (!configuration.IncomingRequestEnabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(
                configuration.IncomingRequestWebhook))
        {
            return;
        }

        var body =
            _messageTemplateService.Format(
                configuration.IncomingRequestMessage,
                payload);

        var title =
            BuildTitle(
                configuration.IncomingRequestEmoji,
                configuration.IncomingRequestTitle);

        await _discordWebhookService.SendSeerrMessageAsync(
            configuration.IncomingRequestWebhook,
            title,
            body,
            payload,
            configuration.UseEmbeds,
            configuration.ShowPoster,
            configuration.ShowMediaType,
            cancellationToken);
    }

    private async Task HandleAvailableRequest(
        PluginConfiguration configuration,
        SeerrWebhookPayload payload,
        CancellationToken cancellationToken)
    {
        if (!configuration.AvailableRequestEnabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(
                configuration.AvailableRequestWebhook))
        {
            return;
        }

        var mapping =
            FindUserMapping(
                configuration,
                payload);

        var discordUserId =
            mapping is not null &&
            mapping.Enabled
                ? mapping.DiscordUserId
                : string.Empty;

        if (string.IsNullOrWhiteSpace(
                discordUserId) &&
            !configuration.SendAvailableWithoutMapping)
        {
            return;
        }

        if (!configuration.MentionDiscordUser)
        {
            discordUserId =
                string.Empty;
        }

        var body =
            _messageTemplateService.Format(
                configuration.AvailableRequestMessage,
                payload,
                discordUserId);

        var title =
            BuildTitle(
                configuration.AvailableRequestEmoji,
                configuration.AvailableRequestTitle);

        await _discordWebhookService.SendSeerrMessageAsync(
            configuration.AvailableRequestWebhook,
            title,
            body,
            payload,
            configuration.UseEmbeds,
            configuration.ShowPoster,
            configuration.ShowMediaType,
            cancellationToken);
    }

    private static UserDiscordMapping? FindUserMapping(
        PluginConfiguration configuration,
        SeerrWebhookPayload payload)
    {
        configuration.UserMappings ??=
            new List<UserDiscordMapping>();

        /*
         * First try to match using the real Jellyfin User ID.
         * This is the most reliable method.
         */
        if (Guid.TryParse(
                payload.JellyfinUserId,
                out var jellyfinUserId))
        {
            var mappingById =
                configuration.UserMappings
                    .FirstOrDefault(x =>
                        x.JellyfinUserId ==
                        jellyfinUserId);

            if (mappingById is not null)
            {
                return mappingById;
            }
        }

        /*
         * Fallback:
         * If Seerr does not provide a Jellyfin User ID,
         * try to match using the Jellyfin username.
         */
        if (!string.IsNullOrWhiteSpace(
                payload.RequestedBy))
        {
            return configuration.UserMappings
                .FirstOrDefault(x =>
                    string.Equals(
                        x.JellyfinUsername,
                        payload.RequestedBy,
                        StringComparison.OrdinalIgnoreCase));
        }

        return null;
    }

    private bool ValidateWebhookToken(
        string configuredToken)
    {
        /*
         * No token configured?
         * Accept the webhook without authentication.
         *
         * A token is recommended when the webhook endpoint
         * is publicly accessible.
         */
        if (string.IsNullOrWhiteSpace(
                configuredToken))
        {
            return true;
        }

        /*
         * Check the Authorization header first.
         *
         * Supported:
         *
         * Authorization: Bearer TOKEN
         *
         * and:
         *
         * Authorization: TOKEN
         */
        var authorization =
            Request.Headers.Authorization
                .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(
                authorization))
        {
            var suppliedToken =
                authorization.Trim();

            if (suppliedToken.StartsWith(
                    "Bearer ",
                    StringComparison.OrdinalIgnoreCase))
            {
                suppliedToken =
                    suppliedToken[7..]
                        .Trim();
            }

            if (string.Equals(
                    suppliedToken,
                    configuredToken,
                    StringComparison.Ordinal))
            {
                return true;
            }
        }

        /*
         * Also support a custom header:
         *
         * X-Seerr-Token: TOKEN
         */
        var customToken =
            Request.Headers["X-Seerr-Token"]
                .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(
                customToken))
        {
            if (string.Equals(
                    customToken.Trim(),
                    configuredToken,
                    StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string BuildTitle(
        string emoji,
        string title)
    {
        var cleanEmoji =
            emoji?.Trim()
            ?? string.Empty;

        var cleanTitle =
            title?.Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(
                cleanEmoji))
        {
            return cleanTitle;
        }

        if (string.IsNullOrWhiteSpace(
                cleanTitle))
        {
            return cleanEmoji;
        }

        return $"{cleanEmoji} {cleanTitle}";
    }
}