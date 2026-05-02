using System;
using System.Collections.Generic;
using TopSpeed.Localization;
using TopSpeed.Server.Config;

namespace TopSpeed.Server.Moderation
{
    internal readonly struct ModerationNameEntry
    {
        public ModerationNameEntry(uint playerId, string name)
        {
            PlayerId = playerId;
            Name = name ?? string.Empty;
        }

        public uint PlayerId { get; }
        public string Name { get; }
    }

    internal readonly struct NameValidationResult
    {
        private NameValidationResult(bool accepted, string normalizedName, string rejectReasonCode, string rejectMessage)
        {
            Accepted = accepted;
            NormalizedName = normalizedName ?? string.Empty;
            RejectReasonCode = rejectReasonCode ?? string.Empty;
            RejectMessage = rejectMessage ?? string.Empty;
        }

        public bool Accepted { get; }
        public string NormalizedName { get; }
        public string RejectReasonCode { get; }
        public string RejectMessage { get; }

        public static NameValidationResult Allow(string normalizedName)
        {
            return new NameValidationResult(true, normalizedName ?? string.Empty, string.Empty, string.Empty);
        }

        public static NameValidationResult Reject(string reasonCode, string message)
        {
            return new NameValidationResult(false, string.Empty, reasonCode, message);
        }
    }

    internal static class NameModeration
    {
        private const int MaxAllowedLetterRun = 2;

        public static NameValidationResult Validate(
            ServerModerationSettings settings,
            string? rawName,
            uint currentPlayerId,
            IEnumerable<ModerationNameEntry> existingNames)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var normalized = (rawName ?? string.Empty).Trim();

            if (normalized.Length > settings.MaxNameLength)
            {
                return NameValidationResult.Reject(
                    "name_too_long",
                    LocalizationService.Format(
                        LocalizationService.Mark("Your call sign is too long. The server does not allow names exceeding {0} characters. Please try a different name."),
                        settings.MaxNameLength));
            }

            if (settings.BlockRepeatedLettersInName && HasRepeatedLetterRun(normalized, MaxAllowedLetterRun + 1))
            {
                return NameValidationResult.Reject(
                    "name_repeated_letters",
                    LocalizationService.Mark("Your call sign was rejected because it contains a letter repeated too many times in a row. Please try a different name."));
            }

            if (!settings.AllowDuplicateNames && !string.IsNullOrWhiteSpace(normalized))
            {
                if (HasDuplicateName(existingNames, currentPlayerId, normalized))
                {
                    return NameValidationResult.Reject(
                        "name_duplicate",
                        LocalizationService.Mark("This call sign is already in use on this server. Please choose a different name."));
                }
            }

            return NameValidationResult.Allow(normalized);
        }

        private static bool HasDuplicateName(IEnumerable<ModerationNameEntry> existingNames, uint currentPlayerId, string normalizedName)
        {
            foreach (var entry in existingNames)
            {
                if (entry.PlayerId == currentPlayerId)
                    continue;

                var existing = (entry.Name ?? string.Empty).Trim();
                if (existing.Length == 0)
                    continue;

                if (string.Equals(existing, normalizedName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool HasRepeatedLetterRun(string value, int minRunLength)
        {
            if (string.IsNullOrWhiteSpace(value) || minRunLength <= 1)
                return false;

            var previous = '\0';
            var run = 0;

            for (var i = 0; i < value.Length; i++)
            {
                var ch = value[i];
                if (!char.IsLetter(ch))
                {
                    previous = '\0';
                    run = 0;
                    continue;
                }

                var normalized = char.ToLowerInvariant(ch);
                if (normalized == previous)
                {
                    run++;
                }
                else
                {
                    previous = normalized;
                    run = 1;
                }

                if (run >= minRunLength)
                    return true;
            }

            return false;
        }
    }
}
