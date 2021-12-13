using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

namespace Linguist
{
    public class ChatMessage
    {
        public XivChatType Type { get; set; }
        public uint SenderId { get; set; }
        public SeString Sender { get; set; }
        public SeString Message { get; set; }
    }
}