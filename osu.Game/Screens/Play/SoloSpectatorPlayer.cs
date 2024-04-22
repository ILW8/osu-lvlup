// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Screens;
using osu.Game.Online.Spectator;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Screens.Play
{
    public partial class SoloSpectatorPlayer : SpectatorPlayer
    {
        private readonly Score score;

        protected override UserActivity InitialActivity => new UserActivity.SpectatingUser(Score.ScoreInfo);

        public SoloSpectatorPlayer(Score score)
            : base(score, new PlayerConfiguration { AllowUserInteraction = false })
        {
            this.score = score;
        }

        private void userBeganPlaying(int userId, SpectatorState state)
        {
            if (userId != score.ScoreInfo.UserID) return;

            Schedule(() =>
            {
                if (this.IsCurrentScreen()) this.Exit();
            });
        }
    }
}
