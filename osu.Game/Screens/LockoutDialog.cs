// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens
{
    public partial class LockoutDialog : PopupDialog
    {
        [Resolved]
        private IDialogOverlay dialogOverlay { get; set; } = null!;

        [Resolved]
        private LockoutManager lockoutManager { get; set; } = null!;

        private readonly OsuPasswordTextBox textBox = new OsuPasswordTextBox
        {
            PlaceholderText = @"Unlock password",
            RelativeSizeAxes = Axes.X,
            Y = 240,
            Width = 0.8f,
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre
        };

        public LockoutDialog()
        {
            BodyText = @"Play session expired, please input unlock key to start a new session.";

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogDangerousButton
                {
                    Text = @"Unlock",
                    Action = () =>
                    {
                        lockoutManager.Unlock(textBox.Text);
                        textBox.Hide();
                    },
                    Margin = new MarginPadding { Top = 50 }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(textBox);
            ChangeInternalChildDepth(textBox, 0);
        }
    }
}
