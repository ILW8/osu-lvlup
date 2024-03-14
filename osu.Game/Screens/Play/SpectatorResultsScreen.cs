// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Play
{
    public partial class SpectatorResultsScreen : SoloResultsScreen
    {
        public SpectatorResultsScreen(ScoreInfo score)
            : base(score)
        {
        }
    }
}
