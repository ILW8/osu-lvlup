// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.BeatmapSet;

namespace osu.Game.Screens.Select
{
    public partial class BeatmapDetails : Container
    {
        private const float spacing = 10;
        private const float transition_duration = 250;

        private readonly MetadataSection description, source, tags;
        private readonly LoadingLayer loading;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private SongSelect? songSelect { get; set; }

        private IBeatmapInfo? beatmapInfo;

        public IBeatmapInfo? BeatmapInfo
        {
            get => beatmapInfo;
            set
            {
                if (value == beatmapInfo) return;

                beatmapInfo = value;

                Scheduler.AddOnce(updateStatistics);
            }
        }

        public BeatmapDetails()
        {
            CornerRadius = 10;
            Masking = true;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black.Opacity(0.3f),
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = spacing },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension()
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Horizontal,
                                Children = new Drawable[]
                                {
                                    new OsuScrollContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 250,
                                        Width = 1.0f,
                                        ScrollbarVisible = false,
                                        Padding = new MarginPadding { Left = spacing / 2 },
                                        Child = new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            LayoutDuration = transition_duration,
                                            LayoutEasing = Easing.OutQuad,
                                            Children = new[]
                                            {
                                                description = new MetadataSectionDescription(query => songSelect?.Search(query)),
                                                source = new MetadataSectionSource(query => songSelect?.Search(query)),
                                                tags = new MetadataSectionTags(query => songSelect?.Search(query)),
                                            },
                                        },
                                    },
                                },
                            },
                        }
                    }
                },
                loading = new LoadingLayer(true)
            };
        }

        private void updateStatistics()
        {
            description.Metadata = BeatmapInfo?.DifficultyName ?? string.Empty;
            source.Metadata = BeatmapInfo?.Metadata.Source ?? string.Empty;
            tags.Metadata = BeatmapInfo?.Metadata.Tags ?? string.Empty;

            // for now, let's early abort if an OnlineID is not present (should have been populated at import time).
            if (BeatmapInfo == null || BeatmapInfo.OnlineID <= 0 || api.State.Value == APIState.Offline)
            {
                updateMetrics();
                return;
            }

            var requestedBeatmap = BeatmapInfo;

            var lookup = new GetBeatmapRequest(requestedBeatmap);

            lookup.Success += res =>
            {
                Schedule(() =>
                {
                    if (beatmapInfo != requestedBeatmap)
                        // the beatmap has been changed since we started the lookup.
                        return;

                    updateMetrics();
                });
            };

            lookup.Failure += _ =>
            {
                Schedule(() =>
                {
                    if (beatmapInfo != requestedBeatmap)
                        // the beatmap has been changed since we started the lookup.
                        return;

                    updateMetrics();
                });
            };

            api.Queue(lookup);
            loading.Show();
        }

        private void updateMetrics()
        {
            loading.Hide();
        }
    }
}
