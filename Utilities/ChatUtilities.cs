using System.Linq;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace Linguist.Utilities
{
    public static class ChatUtilities
    {
        public static bool IsChatLocalPlayer(SeString sender, XivChatType type, SeString message)
        {
            var playerPayload = sender.Payloads.SingleOrDefault(x => x is PlayerPayload) as PlayerPayload;
            if (type is XivChatType.StandardEmote) playerPayload = message.Payloads.FirstOrDefault(x => x is PlayerPayload) as PlayerPayload;
            return playerPayload == default(PlayerPayload);
        }
    }
}