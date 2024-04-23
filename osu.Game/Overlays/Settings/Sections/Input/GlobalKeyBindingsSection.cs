// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class ResetKeybindsDialog : DangerousActionDialog
    {
        public ResetKeybindsDialog(Action resetAction)
        {
            HeaderText = @"Are you sure you want to proceed?";
            BodyText = @"This will reset all keybinds to their default value";
            DangerousAction = resetAction;
        }
    }

    public partial class ResetKeyBindingsSection : SettingsSection
    {
        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.Exclamation
        };

        public override LocalisableString Header => @"Reset all keybinds";

        [BackgroundDependencyLoader]
        private void load(IDialogOverlay? dialogOverlay)
        {
            Add(new DangerousSettingsButton
            {
                Text = "Reset all settings to default",
                Action = () =>
                {
                    dialogOverlay?.Push(new ResetKeybindsDialog(() =>
                    {
                        foreach (var button in Parent.ChildrenOfType<ResetButton>())
                        {
                            button.Action.Invoke();
                        }
                    }));
                }
            });
        }
    }

    public partial class GlobalKeyBindingsSection : SettingsSection
    {
        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.Globe
        };

        public override LocalisableString Header => InputSettingsStrings.GlobalKeyBindingHeader;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRange(new[]
            {
                new GlobalKeyBindingsSubsection(string.Empty, GlobalActionCategory.General),
                new GlobalKeyBindingsSubsection(InputSettingsStrings.OverlaysSection, GlobalActionCategory.Overlays),
                new GlobalKeyBindingsSubsection(InputSettingsStrings.AudioSection, GlobalActionCategory.AudioControl),
                new GlobalKeyBindingsSubsection(InputSettingsStrings.SongSelectSection, GlobalActionCategory.SongSelect),
                new GlobalKeyBindingsSubsection(InputSettingsStrings.InGameSection, GlobalActionCategory.InGame),
                new GlobalKeyBindingsSubsection(InputSettingsStrings.ReplaySection, GlobalActionCategory.Replay),
                new GlobalKeyBindingsSubsection(InputSettingsStrings.EditorSection, GlobalActionCategory.Editor),
            });
        }
    }
}
