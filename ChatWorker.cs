using System;
using System.Collections.Concurrent;
using System.Threading;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Linguist.Utilities;

namespace Linguist
{
    public class ChatWorker : IDisposable
    {
        private readonly BlockingCollection<ChatMessage> _chatMessages = new();
        private readonly ChatGui _chatGui;
        private readonly Configuration _configuration;

        public ChatWorker(ChatGui chatGui, Configuration configuration)
        {
            _chatGui = chatGui;
            _configuration = configuration;
        }

        public bool IsRunning { get; private set; }

        public void Run()
        {
            if (IsRunning)
                return;

            IsRunning = true;

            var thread = new Thread(ThreadProc);
            thread.Start();
        }

        public void Enqueue(ChatMessage chatMessage)
        {
            _chatMessages.Add(chatMessage);
        }

        private void ThreadProc()
        {
            while (IsRunning)
            {
                var chatMessage = _chatMessages.Take();
                Process(chatMessage);
            }
        }

        private void Process(ChatMessage chatMessage)
        {
            var translation = Translator.Translate(_configuration.Language, chatMessage.Message.TextValue);
            
            // insert two marker
            chatMessage.Message.Payloads.Insert(0, new UIForegroundPayload(0));
            chatMessage.Message.Payloads.Insert(0, new UIForegroundPayload(48));

            if (translation != chatMessage.Message.TextValue)
            {
                chatMessage.Message.Append("  ||  ");
                chatMessage.Message.Append(translation);
            }

            _chatGui.PrintChat(new XivChatEntry()
            {
                Message = chatMessage.Message,
                Name = chatMessage.Sender,
                SenderId = chatMessage.SenderId,
                Type = chatMessage.Type
            });
        }
        
        public void Dispose()
        {
            _chatMessages?.Dispose();
            IsRunning = false;
        }
    }
}