﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public partial class ToolbarUserButton : ToolbarOverlayToggleButton
    {
        public ToolbarUserButton()
        {
            ButtonContent.AutoSizeAxes = Axes.X;
        }
    }
}
