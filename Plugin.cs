using Dalamud.Data;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.IoC;
using Dalamud.Plugin;
using Linguist.Utilities;

namespace Linguist
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Linguist";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private DataManager DataManager { get; init; }
        private ChatGui Chat { get; init; }
        private Configuration Configuration { get; init; }
        private ChatWorker Worker { get; init; }
        private PluginUI PluginUI { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] DataManager dataManager,
            [RequiredVersion("1.0")] ChatGui chatGui)
        {
            PluginInterface = pluginInterface;
            
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);
            
            CommandManager = commandManager;
            DataManager = dataManager;
            Chat = chatGui;
            Worker = new ChatWorker(chatGui, Configuration);
            PluginUI = new PluginUI(Configuration);

            CommandManager.AddHandler("/trn", new CommandInfo(OnCommand)
            {
                HelpMessage = "Show Linguist configuration window."
            });

            Chat.ChatMessage += OnChatMessage;
            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUI;
            Worker.Run();
        }

        public void Dispose()
        {
            CommandManager.RemoveHandler("/trn");
            Worker.Dispose();
            PluginUI.Dispose();
            Chat.ChatMessage -= OnChatMessage;
        }

        private void OnCommand(string command, string arguments)
        {
            OpenConfigUI();
        }
        
        private void DrawUI()
        {
            PluginUI.Draw();
        }
        
        private void OpenConfigUI()
        {
            PluginUI.IsSettingsVisible = true;
        }
        
        private void OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (!Configuration.IsEnable)
                return;
            
            if (!Configuration.ChatTypes.Contains(type))
                return;

            // check if this is the translated message
            if (message.Payloads[0] is UIForegroundPayload && message.Payloads[1] is UIForegroundPayload)
                return;

            // do not translate local player message
            if (ChatUtilities.IsChatLocalPlayer(sender, type, message))
                return;

            // remove the message and it will be add to client once it has been translated
            isHandled = true;

            Worker.Enqueue(new ChatMessage()
            {
                Type = type,
                Message = message,
                Sender = sender,
                SenderId = senderId
            });
        }
    }
}
