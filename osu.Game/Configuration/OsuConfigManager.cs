// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Configuration.Tracking;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods.Input;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using osu.Game.Skinning;
using osu.Game.Users;

namespace osu.Game.Configuration
{
    public class OsuConfigManager : IniConfigManager<OsuSetting>, IGameplaySettings
    {
        public OsuConfigManager(Storage storage)
            : base(storage)
        {
            Migrate();
        }

        public Dictionary<OsuSetting, Type> TypeMapping = new Dictionary<OsuSetting, Type>();

        protected override void InitialiseDefaults()
        {
            // UI/selection defaults
            // TypeMapping[OsuSetting.Ruleset] = SetDefault(OsuSetting.Ruleset, string.Empty).GetType();
            TypeMapping[OsuSetting.Ruleset] = SetDefault(OsuSetting.Ruleset, string.Empty).GetType();
            TypeMapping[OsuSetting.Skin] = SetDefault(OsuSetting.Skin, SkinInfo.ARGON_SKIN.ToString()).GetType();

            TypeMapping[OsuSetting.BeatmapDetailTab] = SetDefault(OsuSetting.BeatmapDetailTab, PlayBeatmapDetailArea.TabType.Local).GetType();
            TypeMapping[OsuSetting.BeatmapDetailModsFilter] = SetDefault(OsuSetting.BeatmapDetailModsFilter, false).GetType();

            TypeMapping[OsuSetting.ShowConvertedBeatmaps] = SetDefault(OsuSetting.ShowConvertedBeatmaps, true).GetType();
            TypeMapping[OsuSetting.DisplayStarsMinimum] = SetDefault(OsuSetting.DisplayStarsMinimum, 0.0, 0, 10, 0.1).GetType();
            TypeMapping[OsuSetting.DisplayStarsMaximum] = SetDefault(OsuSetting.DisplayStarsMaximum, 10.1, 0, 10.1, 0.1).GetType();

            TypeMapping[OsuSetting.SongSelectGroupingMode] = SetDefault(OsuSetting.SongSelectGroupingMode, GroupMode.All).GetType();
            TypeMapping[OsuSetting.SongSelectSortingMode] = SetDefault(OsuSetting.SongSelectSortingMode, SortMode.Title).GetType();

            TypeMapping[OsuSetting.RandomSelectAlgorithm] = SetDefault(OsuSetting.RandomSelectAlgorithm, RandomSelectAlgorithm.RandomPermutation).GetType();
            TypeMapping[OsuSetting.ModSelectHotkeyStyle] = SetDefault(OsuSetting.ModSelectHotkeyStyle, ModSelectHotkeyStyle.Sequential).GetType();
            TypeMapping[OsuSetting.ModSelectTextSearchStartsActive] = SetDefault(OsuSetting.ModSelectTextSearchStartsActive, true).GetType();

            TypeMapping[OsuSetting.ChatDisplayHeight] = SetDefault(OsuSetting.ChatDisplayHeight, ChatOverlay.DEFAULT_HEIGHT, 0.2f, 1f).GetType();

            TypeMapping[OsuSetting.BeatmapListingCardSize] = SetDefault(OsuSetting.BeatmapListingCardSize, BeatmapCardSize.Normal).GetType();

            TypeMapping[OsuSetting.ProfileCoverExpanded] = SetDefault(OsuSetting.ProfileCoverExpanded, true).GetType();

            TypeMapping[OsuSetting.ToolbarClockDisplayMode] = SetDefault(OsuSetting.ToolbarClockDisplayMode, ToolbarClockDisplayMode.Full).GetType();

            TypeMapping[OsuSetting.SongSelectBackgroundBlur] = SetDefault(OsuSetting.SongSelectBackgroundBlur, true).GetType();

            // Online settings
            TypeMapping[OsuSetting.Username] = SetDefault(OsuSetting.Username, string.Empty).GetType();
            TypeMapping[OsuSetting.Token] = SetDefault(OsuSetting.Token, string.Empty).GetType();

#pragma warning disable CS0618 // Type or member is obsolete
            // this default set MUST remain despite the setting being deprecated, because `SetDefault()` calls are implicitly used to declare the type returned for the lookup.
            // if this is removed, the setting will be interpreted as a string, and `Migrate()` will fail due to cast failure.
            // can be removed 20240618
            TypeMapping[OsuSetting.AutomaticallyDownloadWhenSpectating] = SetDefault(OsuSetting.AutomaticallyDownloadWhenSpectating, false).GetType();
#pragma warning restore CS0618 // Type or member is obsolete
            TypeMapping[OsuSetting.AutomaticallyDownloadMissingBeatmaps] = SetDefault(OsuSetting.AutomaticallyDownloadMissingBeatmaps, false).GetType();

            var savePasswordDefault = SetDefault(OsuSetting.SavePassword, false);
            TypeMapping[OsuSetting.SavePassword] = savePasswordDefault.GetType();
            savePasswordDefault.ValueChanged += enabled =>
            {
                if (enabled.NewValue)
                    SetValue(OsuSetting.SaveUsername, true);
                else
                    GetBindable<string>(OsuSetting.Token).SetDefault();
            };

            var saveUsernameDefault = SetDefault(OsuSetting.SaveUsername, true);
            TypeMapping[OsuSetting.SaveUsername] = saveUsernameDefault.GetType();
            saveUsernameDefault.ValueChanged += enabled =>
            {
                if (!enabled.NewValue)
                {
                    GetBindable<string>(OsuSetting.Username).SetDefault();
                    SetValue(OsuSetting.SavePassword, false);
                }
            };

            TypeMapping[OsuSetting.ExternalLinkWarning] = SetDefault(OsuSetting.ExternalLinkWarning, true).GetType();
            TypeMapping[OsuSetting.PreferNoVideo] = SetDefault(OsuSetting.PreferNoVideo, false).GetType();

            TypeMapping[OsuSetting.ShowOnlineExplicitContent] = SetDefault(OsuSetting.ShowOnlineExplicitContent, false).GetType();

            TypeMapping[OsuSetting.NotifyOnUsernameMentioned] = SetDefault(OsuSetting.NotifyOnUsernameMentioned, true).GetType();
            TypeMapping[OsuSetting.NotifyOnPrivateMessage] = SetDefault(OsuSetting.NotifyOnPrivateMessage, true).GetType();

            // Audio
            TypeMapping[OsuSetting.VolumeInactive] = SetDefault(OsuSetting.VolumeInactive, 0.25, 0, 1, 0.01).GetType();

            TypeMapping[OsuSetting.MenuVoice] = SetDefault(OsuSetting.MenuVoice, true).GetType();
            TypeMapping[OsuSetting.MenuMusic] = SetDefault(OsuSetting.MenuMusic, true).GetType();
            TypeMapping[OsuSetting.MenuTips] = SetDefault(OsuSetting.MenuTips, true).GetType();

            TypeMapping[OsuSetting.AudioOffset] = SetDefault(OsuSetting.AudioOffset, 0, -500.0, 500.0, 1).GetType();

            // Input
            TypeMapping[OsuSetting.MenuCursorSize] = SetDefault(OsuSetting.MenuCursorSize, 1.0f, 0.5f, 2f, 0.01f).GetType();
            TypeMapping[OsuSetting.GameplayCursorSize] = SetDefault(OsuSetting.GameplayCursorSize, 1.0f, 0.1f, 2f, 0.01f).GetType();
            TypeMapping[OsuSetting.GameplayCursorDuringTouch] = SetDefault(OsuSetting.GameplayCursorDuringTouch, false).GetType();
            TypeMapping[OsuSetting.AutoCursorSize] = SetDefault(OsuSetting.AutoCursorSize, false).GetType();

            TypeMapping[OsuSetting.MouseDisableButtons] = SetDefault(OsuSetting.MouseDisableButtons, false).GetType();
            TypeMapping[OsuSetting.MouseDisableWheel] = SetDefault(OsuSetting.MouseDisableWheel, false).GetType();
            TypeMapping[OsuSetting.ConfineMouseMode] = SetDefault(OsuSetting.ConfineMouseMode, OsuConfineMouseMode.DuringGameplay).GetType();

            TypeMapping[OsuSetting.TouchDisableGameplayTaps] = SetDefault(OsuSetting.TouchDisableGameplayTaps, false).GetType();

            // Graphics
            TypeMapping[OsuSetting.ShowFpsDisplay] = SetDefault(OsuSetting.ShowFpsDisplay, false).GetType();

            TypeMapping[OsuSetting.ShowStoryboard] = SetDefault(OsuSetting.ShowStoryboard, true).GetType();
            TypeMapping[OsuSetting.BeatmapSkins] = SetDefault(OsuSetting.BeatmapSkins, true).GetType();
            TypeMapping[OsuSetting.BeatmapColours] = SetDefault(OsuSetting.BeatmapColours, true).GetType();
            TypeMapping[OsuSetting.BeatmapHitsounds] = SetDefault(OsuSetting.BeatmapHitsounds, true).GetType();

            TypeMapping[OsuSetting.CursorRotation] = SetDefault(OsuSetting.CursorRotation, true).GetType();

            TypeMapping[OsuSetting.MenuParallax] = SetDefault(OsuSetting.MenuParallax, true).GetType();

            // See https://stackoverflow.com/a/63307411 for default sourcing.
            TypeMapping[OsuSetting.Prefer24HourTime] = SetDefault(OsuSetting.Prefer24HourTime, !CultureInfoHelper.SystemCulture.DateTimeFormat.ShortTimePattern.Contains(@"tt")).GetType();

            // Gameplay
            TypeMapping[OsuSetting.PositionalHitsoundsLevel] = SetDefault(OsuSetting.PositionalHitsoundsLevel, 0.2f, 0, 1).GetType();
            TypeMapping[OsuSetting.DimLevel] = SetDefault(OsuSetting.DimLevel, 0.7, 0, 1, 0.01).GetType();
            TypeMapping[OsuSetting.BlurLevel] = SetDefault(OsuSetting.BlurLevel, 0, 0, 1, 0.01).GetType();
            TypeMapping[OsuSetting.LightenDuringBreaks] = SetDefault(OsuSetting.LightenDuringBreaks, true).GetType();

            TypeMapping[OsuSetting.HitLighting] = SetDefault(OsuSetting.HitLighting, true).GetType();

            TypeMapping[OsuSetting.HUDVisibilityMode] = SetDefault(OsuSetting.HUDVisibilityMode, HUDVisibilityMode.Always).GetType();
            TypeMapping[OsuSetting.ShowHealthDisplayWhenCantFail] = SetDefault(OsuSetting.ShowHealthDisplayWhenCantFail, true).GetType();
            TypeMapping[OsuSetting.FadePlayfieldWhenHealthLow] = SetDefault(OsuSetting.FadePlayfieldWhenHealthLow, true).GetType();
            TypeMapping[OsuSetting.KeyOverlay] = SetDefault(OsuSetting.KeyOverlay, false).GetType();
            TypeMapping[OsuSetting.ReplaySettingsOverlay] = SetDefault(OsuSetting.ReplaySettingsOverlay, true).GetType();
            TypeMapping[OsuSetting.ReplayPlaybackControlsExpanded] = SetDefault(OsuSetting.ReplayPlaybackControlsExpanded, true).GetType();
            TypeMapping[OsuSetting.GameplayLeaderboard] = SetDefault(OsuSetting.GameplayLeaderboard, true).GetType();
            TypeMapping[OsuSetting.AlwaysPlayFirstComboBreak] = SetDefault(OsuSetting.AlwaysPlayFirstComboBreak, true).GetType();

            TypeMapping[OsuSetting.FloatingComments] = SetDefault(OsuSetting.FloatingComments, false).GetType();

            TypeMapping[OsuSetting.ScoreDisplayMode] = SetDefault(OsuSetting.ScoreDisplayMode, ScoringMode.Standardised).GetType();

            TypeMapping[OsuSetting.IncreaseFirstObjectVisibility] = SetDefault(OsuSetting.IncreaseFirstObjectVisibility, true).GetType();
            TypeMapping[OsuSetting.GameplayDisableWinKey] = SetDefault(OsuSetting.GameplayDisableWinKey, true).GetType();

            // Update
            TypeMapping[OsuSetting.ReleaseStream] = SetDefault(OsuSetting.ReleaseStream, ReleaseStream.Lazer).GetType();

            TypeMapping[OsuSetting.Version] = SetDefault(OsuSetting.Version, string.Empty).GetType();

            TypeMapping[OsuSetting.ShowFirstRunSetup] = SetDefault(OsuSetting.ShowFirstRunSetup, true).GetType();

            TypeMapping[OsuSetting.ScreenshotFormat] = SetDefault(OsuSetting.ScreenshotFormat, ScreenshotFormat.Jpg).GetType();
            TypeMapping[OsuSetting.ScreenshotCaptureMenuCursor] = SetDefault(OsuSetting.ScreenshotCaptureMenuCursor, false).GetType();

            TypeMapping[OsuSetting.SongSelectRightMouseScroll] = SetDefault(OsuSetting.SongSelectRightMouseScroll, false).GetType();

            TypeMapping[OsuSetting.Scaling] = SetDefault(OsuSetting.Scaling, ScalingMode.Off).GetType();
            TypeMapping[OsuSetting.SafeAreaConsiderations] = SetDefault(OsuSetting.SafeAreaConsiderations, true).GetType();
            TypeMapping[OsuSetting.ScalingBackgroundDim] = SetDefault(OsuSetting.ScalingBackgroundDim, 0.9f, 0.5f, 1f).GetType();

            TypeMapping[OsuSetting.ScalingSizeX] = SetDefault(OsuSetting.ScalingSizeX, 0.8f, 0.2f, 1f).GetType();
            TypeMapping[OsuSetting.ScalingSizeY] = SetDefault(OsuSetting.ScalingSizeY, 0.8f, 0.2f, 1f).GetType();

            TypeMapping[OsuSetting.ScalingPositionX] = SetDefault(OsuSetting.ScalingPositionX, 0.5f, 0f, 1f).GetType();
            TypeMapping[OsuSetting.ScalingPositionY] = SetDefault(OsuSetting.ScalingPositionY, 0.5f, 0f, 1f).GetType();

            TypeMapping[OsuSetting.UIScale] = SetDefault(OsuSetting.UIScale, 1f, 0.8f, 1.6f, 0.01f).GetType();

            TypeMapping[OsuSetting.UIHoldActivationDelay] = SetDefault(OsuSetting.UIHoldActivationDelay, 200.0, 0.0, 500.0, 50.0).GetType();

            TypeMapping[OsuSetting.IntroSequence] = SetDefault(OsuSetting.IntroSequence, IntroSequence.Triangles).GetType();

            TypeMapping[OsuSetting.MenuBackgroundSource] = SetDefault(OsuSetting.MenuBackgroundSource, BackgroundSource.Skin).GetType();
            TypeMapping[OsuSetting.SeasonalBackgroundMode] = SetDefault(OsuSetting.SeasonalBackgroundMode, SeasonalBackgroundMode.Sometimes).GetType();

            TypeMapping[OsuSetting.DiscordRichPresence] = SetDefault(OsuSetting.DiscordRichPresence, DiscordRichPresenceMode.Full).GetType();

            TypeMapping[OsuSetting.EditorDim] = SetDefault(OsuSetting.EditorDim, 0.25f, 0f, 0.75f, 0.25f).GetType();
            TypeMapping[OsuSetting.EditorWaveformOpacity] = SetDefault(OsuSetting.EditorWaveformOpacity, 0.25f, 0f, 1f, 0.25f).GetType();
            TypeMapping[OsuSetting.EditorShowHitMarkers] = SetDefault(OsuSetting.EditorShowHitMarkers, true).GetType();
            TypeMapping[OsuSetting.EditorAutoSeekOnPlacement] = SetDefault(OsuSetting.EditorAutoSeekOnPlacement, true).GetType();
            TypeMapping[OsuSetting.EditorLimitedDistanceSnap] = SetDefault(OsuSetting.EditorLimitedDistanceSnap, false).GetType();
            TypeMapping[OsuSetting.EditorShowSpeedChanges] = SetDefault(OsuSetting.EditorShowSpeedChanges, false).GetType();

            TypeMapping[OsuSetting.MultiplayerRoomFilter] = SetDefault(OsuSetting.MultiplayerRoomFilter, RoomPermissionsFilter.All).GetType();

            TypeMapping[OsuSetting.LastProcessedMetadataId] = SetDefault(OsuSetting.LastProcessedMetadataId, -1).GetType();

            TypeMapping[OsuSetting.ComboColourNormalisationAmount] = SetDefault(OsuSetting.ComboColourNormalisationAmount, 0.2f, 0f, 1f, 0.01f).GetType();
            TypeMapping[OsuSetting.UserOnlineStatus] = SetDefault<UserStatus?>(OsuSetting.UserOnlineStatus, null).GetType();
        }

        protected override bool CheckLookupContainsPrivateInformation(OsuSetting lookup)
        {
            switch (lookup)
            {
                case OsuSetting.Token:
                    return true;
            }

            return false;
        }

        public void Migrate()
        {
            // arrives as 2020.123.0
            string rawVersion = Get<string>(OsuSetting.Version);

            if (rawVersion.Length < 6)
                return;

            string[] pieces = rawVersion.Split('.');

            // on a fresh install or when coming from a non-release build, execution will end here.
            // we don't want to run migrations in such cases.
            if (!int.TryParse(pieces[0], out int year)) return;
            if (!int.TryParse(pieces[1], out int monthDay)) return;

            // ReSharper disable once UnusedVariable
            int combined = (year * 10000) + monthDay;

            // migrations can be added here using a condition like:
            // if (combined < 20220103) { performMigration() }
            if (combined < 20230918)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                SetValue(OsuSetting.AutomaticallyDownloadMissingBeatmaps, Get<bool>(OsuSetting.AutomaticallyDownloadWhenSpectating)); // can be removed 20240618
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        public override TrackedSettings CreateTrackedSettings()
        {
            // these need to be assigned in normal game startup scenarios.
            Debug.Assert(LookupKeyBindings != null);
            Debug.Assert(LookupSkinName != null);

            return new TrackedSettings
            {
                new TrackedSetting<bool>(OsuSetting.ShowFpsDisplay, state => new SettingDescription(
                    rawValue: state,
                    name: GlobalActionKeyBindingStrings.ToggleFPSCounter,
                    value: state ? CommonStrings.Enabled.ToLower() : CommonStrings.Disabled.ToLower(),
                    shortcut: LookupKeyBindings(GlobalAction.ToggleFPSDisplay))
                ),
                new TrackedSetting<bool>(OsuSetting.MouseDisableButtons, disabledState => new SettingDescription(
                    rawValue: !disabledState,
                    name: GlobalActionKeyBindingStrings.ToggleGameplayMouseButtons,
                    value: disabledState ? CommonStrings.Disabled.ToLower() : CommonStrings.Enabled.ToLower(),
                    shortcut: LookupKeyBindings(GlobalAction.ToggleGameplayMouseButtons))
                ),
                new TrackedSetting<bool>(OsuSetting.GameplayLeaderboard, state => new SettingDescription(
                    rawValue: state,
                    name: GlobalActionKeyBindingStrings.ToggleInGameLeaderboard,
                    value: state ? CommonStrings.Enabled.ToLower() : CommonStrings.Disabled.ToLower(),
                    shortcut: LookupKeyBindings(GlobalAction.ToggleInGameLeaderboard))
                ),
                new TrackedSetting<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode, visibilityMode => new SettingDescription(
                    rawValue: visibilityMode,
                    name: GameplaySettingsStrings.HUDVisibilityMode,
                    value: visibilityMode.GetLocalisableDescription(),
                    shortcut: new TranslatableString(@"_", @"{0}: {1} {2}: {3}",
                        GlobalActionKeyBindingStrings.ToggleInGameInterface,
                        LookupKeyBindings(GlobalAction.ToggleInGameInterface),
                        GlobalActionKeyBindingStrings.HoldForHUD,
                        LookupKeyBindings(GlobalAction.HoldForHUD)))
                ),
                new TrackedSetting<ScalingMode>(OsuSetting.Scaling, scalingMode => new SettingDescription(
                        rawValue: scalingMode,
                        name: GraphicsSettingsStrings.ScreenScaling,
                        value: scalingMode.GetLocalisableDescription()
                    )
                ),
                new TrackedSetting<string>(OsuSetting.Skin, skin =>
                {
                    string skinName = string.Empty;

                    if (Guid.TryParse(skin, out var id))
                        skinName = LookupSkinName(id);

                    return new SettingDescription(
                        rawValue: skinName,
                        name: SkinSettingsStrings.SkinSectionHeader,
                        value: skinName,
                        shortcut: new TranslatableString(@"_", @"{0}: {1}",
                            GlobalActionKeyBindingStrings.RandomSkin,
                            LookupKeyBindings(GlobalAction.RandomSkin))
                    );
                }),
                new TrackedSetting<float>(OsuSetting.UIScale, scale => new SettingDescription(
                        rawValue: scale,
                        name: GraphicsSettingsStrings.UIScaling,
                        value: $"{scale:N2}x"
                        // TODO: implement lookup for framework platform key bindings
                    )
                ),
            };
        }

        public Func<Guid, string> LookupSkinName { private get; set; } = _ => @"unknown";

        public Func<GlobalAction, LocalisableString> LookupKeyBindings { get; set; } = _ => @"unknown";

        IBindable<float> IGameplaySettings.ComboColourNormalisationAmount => GetOriginalBindable<float>(OsuSetting.ComboColourNormalisationAmount);
        IBindable<float> IGameplaySettings.PositionalHitsoundsLevel => GetOriginalBindable<float>(OsuSetting.PositionalHitsoundsLevel);
    }

    // IMPORTANT: These are used in user configuration files.
    // The naming of these keys should not be changed once they are deployed in a release, unless migration logic is also added.
    public enum OsuSetting
    {
        Ruleset,
        Token,
        MenuCursorSize,
        GameplayCursorSize,
        AutoCursorSize,
        GameplayCursorDuringTouch,
        DimLevel,
        BlurLevel,
        EditorDim,
        LightenDuringBreaks,
        ShowStoryboard,
        KeyOverlay,
        GameplayLeaderboard,
        PositionalHitsoundsLevel,
        AlwaysPlayFirstComboBreak,
        FloatingComments,
        HUDVisibilityMode,

        ShowHealthDisplayWhenCantFail,
        FadePlayfieldWhenHealthLow,

        /// <summary>
        /// Disables mouse buttons clicks during gameplay.
        /// </summary>
        MouseDisableButtons,
        MouseDisableWheel,
        ConfineMouseMode,

        /// <summary>
        /// Globally applied audio offset.
        /// This is added to the audio track's current time. Higher values will cause gameplay to occur earlier, relative to the audio track.
        /// </summary>
        AudioOffset,

        VolumeInactive,
        MenuMusic,
        MenuVoice,
        MenuTips,
        CursorRotation,
        MenuParallax,
        Prefer24HourTime,
        BeatmapDetailTab,
        BeatmapDetailModsFilter,
        Username,
        ReleaseStream,
        SavePassword,
        SaveUsername,
        DisplayStarsMinimum,
        DisplayStarsMaximum,
        SongSelectGroupingMode,
        SongSelectSortingMode,
        RandomSelectAlgorithm,
        ModSelectHotkeyStyle,
        ShowFpsDisplay,
        ChatDisplayHeight,
        BeatmapListingCardSize,
        ToolbarClockDisplayMode,
        SongSelectBackgroundBlur,
        Version,
        ShowFirstRunSetup,
        ShowConvertedBeatmaps,
        Skin,
        ScreenshotFormat,
        ScreenshotCaptureMenuCursor,
        SongSelectRightMouseScroll,
        BeatmapSkins,
        BeatmapColours,
        BeatmapHitsounds,
        IncreaseFirstObjectVisibility,
        ScoreDisplayMode,
        ExternalLinkWarning,
        PreferNoVideo,
        Scaling,
        ScalingPositionX,
        ScalingPositionY,
        ScalingSizeX,
        ScalingSizeY,
        ScalingBackgroundDim,
        UIScale,
        IntroSequence,
        NotifyOnUsernameMentioned,
        NotifyOnPrivateMessage,
        UIHoldActivationDelay,
        HitLighting,
        MenuBackgroundSource,
        GameplayDisableWinKey,
        SeasonalBackgroundMode,
        EditorWaveformOpacity,
        EditorShowHitMarkers,
        EditorAutoSeekOnPlacement,
        DiscordRichPresence,

        [Obsolete($"Use {nameof(AutomaticallyDownloadMissingBeatmaps)} instead.")] // can be removed 20240318
        AutomaticallyDownloadWhenSpectating,

        ShowOnlineExplicitContent,
        LastProcessedMetadataId,
        SafeAreaConsiderations,
        ComboColourNormalisationAmount,
        ProfileCoverExpanded,
        EditorLimitedDistanceSnap,
        ReplaySettingsOverlay,
        ReplayPlaybackControlsExpanded,
        AutomaticallyDownloadMissingBeatmaps,
        EditorShowSpeedChanges,
        TouchDisableGameplayTaps,
        ModSelectTextSearchStartsActive,
        UserOnlineStatus,
        MultiplayerRoomFilter
    }
}
