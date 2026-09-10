namespace Dreamstreaming.SeerrDiscord.Models;

public class UserDiscordMapping
{
    public Guid JellyfinUserId { get; set; }

    public string JellyfinUsername { get; set; } = string.Empty;

    public string DiscordUserId { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;
}