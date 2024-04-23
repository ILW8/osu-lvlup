// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class ResetSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "Settings";

        [BackgroundDependencyLoader]
        private void load(IDialogOverlay? dialogOverlay, OsuConfigManager config)
        {
            Add(new DangerousSettingsButton
            {
                Text = "Reset all settings to default",
                Action = () =>
                {
                    dialogOverlay?.Push(new MassDeleteConfirmationDialog(() =>
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
                                        var theBindableType = bindable.GetType();

                                        // if (!theBindableType.IsGenericType)
                                        // {
                                        //     break;
                                        // }
                                        // var bindableGenericArgument = bindable.GetType().GetGenericArguments()[0];

                                        var setDefaultMethod = theBindableType.GetMethod("SetDefault");
                                        setDefaultMethod?.Invoke(bindable, null);

                                        break;

                                    default:
                                        // welp something went wrong
                                        continue;
                                }

                                // var configBindable = config.GetBindable<object>(settingKey);
                                // configBindable.Value = configBindable.Default; // type error, can't convert from object to <whatever> type
                            }
                        });
                    }));
                }
            });
        }
    }
}
