using System;
using System.Collections.Generic;
using TopSpeed.Data;
using TopSpeed.Localization;

namespace TopSpeed.Protocol
{
    public static partial class PackageBuild
    {
        public static string BuildTrackLoadError(IReadOnlyList<TrackTsmIssue> issues)
        {
            if (issues == null || issues.Count == 0)
                return LocalizationService.Mark("The selected custom track is invalid.");

            var first = issues[0].ToString();
            if (!string.IsNullOrWhiteSpace(first))
                return first;

            return LocalizationService.Mark("The selected custom track is invalid.");
        }
    }
}
