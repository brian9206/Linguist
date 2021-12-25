using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text;
using Dalamud.Interface;
using ImGuiNET;
using Linguist.Utilities;

namespace Linguist
{
    public class PluginUI : IDisposable
    {
        private readonly Configuration _configuration;
        private bool _isSettingsVisible;
        private bool _isTranslationPopupVisible;

        [DllImport("user32.dll")]
        private static extern ushort GetAsyncKeyState(int key);

        public bool IsSettingsVisible
        {
            get => _isSettingsVisible;
            set => _isSettingsVisible = value;
        }
        
        public bool IsTranslationPopupVisible
        {
            get => _isTranslationPopupVisible;
            set => _isTranslationPopupVisible = value;
        }

        public PluginUI(Configuration configuration)
        {
            _configuration = configuration;
        }
        
        public void Draw()
        {
            DrawSettingsWindow();
            DrawTranslationWindow();
        }

        private void DrawSettingsWindow()
        {
            if (!IsSettingsVisible)
                return;
            
            ImGui.SetNextWindowSize(new Vector2(250, 600), ImGuiCond.Appearing);
            if (ImGui.Begin("Linguist", ref _isSettingsVisible, ImGuiWindowFlags.None))
            {
                var isEnable = _configuration.IsEnable;
                if (ImGui.Checkbox("Enable Chat Translation", ref isEnable))
                {
                    _configuration.IsEnable = isEnable;
                    _configuration.Save();
                }
                
                ImGui.Separator();

                var languages = new[]
                {
                    "en", "zh_TW"
                };

                var language = Math.Max(0, Array.IndexOf(languages, _configuration.Language));

                if (ImGui.Combo("Language", ref language, languages, languages.Length))
                {
                    _configuration.Language = languages[language];
                    _configuration.Save();
                }
                
                ImGui.Separator();
                
                ImGui.Text("Hotkey to open translation popup:");

                var modifier = _configuration.Modifier;
                if (VirtualKeySelect("Modifier", ref modifier))
                {
                    _configuration.Modifier = modifier;
                    _configuration.Save();
                }
                
                var key = _configuration.Key;
                if (VirtualKeySelect("Key", ref key))
                {
                    _configuration.Key = key;
                    _configuration.Save();
                }
                    
                
                ImGui.Separator();
                
                ImGui.Text("Chat type(s) to translate:");
                
                var types = Enum.GetValues<XivChatType>().Skip(4);

                foreach (var type in types)
                {
                    var typeEnable = _configuration.ChatTypes.Contains(type);
                    if (ImGui.Checkbox(type.ToString(), ref typeEnable))
                    {
                        if (typeEnable)
                        {
                            if (!_configuration.ChatTypes.Contains(type))
                                _configuration.ChatTypes.Add(type);
                        }
                        else
                        {
                            if (_configuration.ChatTypes.Contains(type))
                                _configuration.ChatTypes.Remove(type);
                        }

                        _configuration.Save();
                    }
                }
            }
        }
        
        private bool VirtualKeySelect(string text, ref VirtualKey chosen)
        {
            if (ImGui.BeginCombo(text, chosen.GetFancyName()))
            {
                foreach (var key in Enum.GetValues<VirtualKey>().Where(x => x != VirtualKey.LBUTTON))
                {
                    if (ImGui.Selectable(key.GetFancyName(), key == chosen))
                    {
                        chosen = key;
                        return true;
                    }
                }

                ImGui.EndCombo();
            }

            return false;
        }
        
        private bool _isTranslationPopupOpened;
        private string _textToTranslate = string.Empty;
        private string _textTranslated = string.Empty;

        private void DrawTranslationWindow()
        {
            if (!_isTranslationPopupOpened && 
                (IsTranslationPopupVisible || 
                (_configuration.Modifier == VirtualKey.NO_KEY || (GetAsyncKeyState((int) _configuration.Modifier) & 0x8000) != 0) && 
                (GetAsyncKeyState((int) _configuration.Key) & 0x8000) != 0))
            {
                _textToTranslate = ImGui.GetClipboardText();
                TranslateText();
                ImGui.OpenPopup("Translation");
                _isTranslationPopupOpened = true;
                IsTranslationPopupVisible = true;
            }

            var size = new Vector2(600, 100);
            var pos = (ImGuiHelpers.MainViewport.Size - size) / 2 - new Vector2(0, 100);

            ImGui.SetNextWindowSize(size, ImGuiCond.Appearing);
            ImGui.SetNextWindowPos(pos, ImGuiCond.Appearing);
            if (ImGui.BeginPopup("Translation", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (!ImGui.IsAnyItemActive() && !ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    ImGui.SetKeyboardFocusHere(0);
                
                if (ImGui.InputTextMultiline(string.Empty, ref _textToTranslate, 1024, new Vector2(585, 42), ImGuiInputTextFlags.AutoSelectAll))
                    TranslateText();
                
                ImGui.TextWrapped(_textTranslated);
                ImGui.EndPopup();
            }
            else if (_isTranslationPopupOpened)
            {
                _isTranslationPopupOpened = false;
                IsTranslationPopupVisible = false;
            }
        }

        private void TranslateText()
        {
            _textTranslated = "Translating...";
            
            Task.Run(() =>
            {
                _textTranslated = Translator.Translate(_configuration.Language, _textToTranslate);
            });
        }

        public void Dispose()
        {
        }
    }
}