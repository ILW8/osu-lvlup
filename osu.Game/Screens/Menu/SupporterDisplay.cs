// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Screens.Menu
{
    public partial class SupporterDisplay : CompositeDrawable
    {
        private readonly IBindable<APIUser> currentUser = new Bindable<APIUser>();

        private Box backgroundBox = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Height = 40;

            AutoSizeAxes = Axes.X;
            AutoSizeDuration = 1000;
            AutoSizeEasing = Easing.OutQuint;

            Masking = true;
            CornerExponent = 2.5f;
            CornerRadius = 15;

            InternalChildren = new Drawable[]
            {
                backgroundBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.4f,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentUser.BindTo(api.LocalUser);

            this
                .FadeOut()
                .Delay(1000)
                .FadeInFromZero(800, Easing.OutQuint);

            scheduleDismissal();
        }

        protected override bool OnClick(ClickEvent e)
        {
            dismissalDelegate?.Cancel();

            this.FadeOut(500, Easing.OutQuint);
            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            backgroundBox.FadeTo(0.6f, 500, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            backgroundBox.FadeTo(0.4f, 500, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        private ScheduledDelegate? dismissalDelegate;

        private void scheduleDismissal()
        {
            dismissalDelegate?.Cancel();
            dismissalDelegate = Scheduler.AddDelayed(() =>
            {
                // If the user is hovering they may want to interact with the link.
                // Give them more time.
                if (IsHovered)
                {
                    scheduleDismissal();
                    return;
                }

                dismissalDelegate?.Cancel();

                AutoSizeEasing = Easing.In;
                this
                    .Delay(200)
                    .FadeOut(750, Easing.Out);
            }, 6000);
        }
    }
}
