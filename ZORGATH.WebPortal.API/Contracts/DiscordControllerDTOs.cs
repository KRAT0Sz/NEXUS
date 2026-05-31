namespace ZORGATH.WebPortal.API.Contracts;

public record GetDiscordLinkedAccountDTO(string? DiscordId, string? DiscordUsername, string? DiscordAvatarHash, DateTimeOffset? LinkedAt);

public record LinkDiscordRequestDTO(string State);

public record RegisterUserViaDiscordDTO(string Token, string Name, string Password, string ConfirmPassword);
