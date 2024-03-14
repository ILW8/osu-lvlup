// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Mods;
using System.Collections.Generic;

namespace osu.Game.Online.API.Requests
{
    public class GetScoresRequest : APIRequest<APIScoresCollection>, IEquatable<GetScoresRequest>
    {
        public const int MAX_SCORES_PER_REQUEST = 50;

        public GetScoresRequest(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset, BeatmapLeaderboardScope scope = BeatmapLeaderboardScope.Local, IEnumerable<IMod>? mods = null)
        {
            Fail(new NotSupportedException(@"Online functionality has been disabled"));
        }

        public bool Equals(GetScoresRequest? other)
        {
            return true;
        }

        protected override string Target => "/";
    }
}
