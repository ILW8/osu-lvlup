// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Screens;
using osu.Game.Screens.Select;
using osu.Game.Users;

namespace osu.Game.Configuration
{
    [Cached]
    public partial class LockoutManager : Component
    {
        public TimeSpan FreePlayLength { get; set; } = new TimeSpan(0, 0, 5);

        private const string password = @"a";

        [Resolved]
        private IDialogOverlay dialogOverlay { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OsuGame game { get; set; } = null!;

        private IBindable<APIUser> user = null!;
        private readonly IBindable<UserActivity> activity = new Bindable<UserActivity>();

        public readonly BindableBool IsUnlocked = new BindableBool();

        [BackgroundDependencyLoader]
        private void load()
        {
            Scheduler.AddDelayed(presentLockoutOverlay, FreePlayLength.TotalMilliseconds);

            user = api.LocalUser.GetBoundCopy();
            user.BindValueChanged(u =>
            {
                activity.UnbindBindings();
                activity.BindTo(u.NewValue.Activity);
            }, true);
        }

        private void presentLockoutOverlay()
        {
            IsUnlocked.Value = false;

            switch (activity.Value)
            {
                case UserActivity.InGame or null:
                    Scheduler.AddDelayed(presentLockoutOverlay, 200);
                    return;

                case UserActivity.WatchingReplay:
                    game.PerformFromScreen(_ => { presentLockoutOverlay(); }, new[] { typeof(SongSelect) }); // return to song select and present overlay
                    return;
            }

            if (dialogOverlay.CurrentDialog != null && dialogOverlay.CurrentDialog.GetType() == typeof(LockoutDialog))
            {
                dialogOverlay.CurrentDialog.PerformAction<PopupDialogDangerousButton>();
                return;
            }

            dialogOverlay.Push(new LockoutDialog());
        }

        public bool Unlock(string attemptedPassword)
        {
            bool success = attemptedPassword == password;

            if (!success)
            {
                presentLockoutOverlay();
                return success;
            }

            Scheduler.AddDelayed(presentLockoutOverlay, FreePlayLength.TotalMilliseconds);
            IsUnlocked.Value = true;
            return success;
        }
    }
}
