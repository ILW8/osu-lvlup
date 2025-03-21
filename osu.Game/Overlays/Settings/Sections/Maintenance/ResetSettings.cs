// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class ResetAllSettingsDialog : DangerousActionDialog
    {
        public ResetAllSettingsDialog(Action resetAction)
        {
            HeaderText = @"Reset all settings?";
            BodyText = @"Keybinds will not be reset. Reset them in the keybinds configuration menu.";
            DangerousAction = resetAction;
        }
    }

    public partial class ResetSettings : SettingsSubsection
    {
        protected override LocalisableString Header => @"Settings";

        [Resolved(CanBeNull = true)]
        private FirstRunSetupOverlay? firstRunSetupOverlay { get; set; }

        [BackgroundDependencyLoader]
        private void load(IDialogOverlay? dialogOverlay, OsuConfigManager config)
        {
            Add(new DangerousSettingsButton
            {
                Text = @"Reset all settings to default",
                Action = () =>
                {
                    dialogOverlay?.Push(new ResetAllSettingsDialog(() =>
                    {
                        Scheduler.Add(() =>
                        {
                            var settingKeys = Enum.GetValues(typeof(OsuSetting)).Cast<OsuSetting>();

                            foreach (var settingKey in settingKeys)
                            {
                                if (!config.TypeMapping.TryGetValue(settingKey, out var settingType))
                                    continue;

                                Type bindableType;

                                // yuck
                                if (settingType.IsGenericType)
                                {
                                    bindableType = settingType.GenericTypeArguments[0];
                                }
                                else if (settingType.BaseType?.IsGenericType ?? false)
                                {
                                    bindableType = settingType.BaseType.GenericTypeArguments[0];
                                }
                                else
                                {
                                    continue;
                                }

                                Type configManagerType = config.GetType();
                                var getBindableMethod = configManagerType.GetMethod("GetBindable");
                                var typedGetBindableMethod = getBindableMethod?.MakeGenericMethod(bindableType);
                                object? hopefullyTheRightBindable = typedGetBindableMethod?.Invoke(config, new object[] { settingKey });

                                switch (hopefullyTheRightBindable)
                                {
                                    case IBindable bindable:
                                        if (bindable.Disabled)
                                            continue;

                                        var theBindableType = bindable.GetType();
                                        var setDefaultMethod = theBindableType.GetMethod("SetDefault");
                                        setDefaultMethod?.Invoke(bindable, null);

                                        break;

                                    default:
                                        // welp something went wrong
                                        continue;
                                }
                            }

                            firstRunSetupOverlay?.Show();
                        });
                    }));
                }
            });
        }
    }
}
