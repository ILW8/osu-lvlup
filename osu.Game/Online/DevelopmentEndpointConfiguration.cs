// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online
{
    public class DevelopmentEndpointConfiguration : EndpointConfiguration
    {
        public DevelopmentEndpointConfiguration()
        {
            WebsiteRootUrl = APIEndpointUrl = @"https://127.0.0.1";
            APIClientSecret = @"3LP2mhUrV89xxzD1YKNndXHEhWWCRLPNKioZ9ymT";
            APIClientID = "5";
            SpectatorEndpointUrl = $@"{APIEndpointUrl}/signalr/spectator";
            MultiplayerEndpointUrl = $@"{APIEndpointUrl}/signalr/multiplayer";
            MetadataEndpointUrl = $@"{APIEndpointUrl}/signalr/metadata";
        }
    }
}
