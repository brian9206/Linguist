using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text;
using ImGuiNET;

namespace Linguist
{
    public class PluginUI : IDisposable
    {
        private readonly Configuration _configuration;
        private bool _isSettingsVisible;

        public bool IsSettingsVisible
        {
            get => _isSettingsVisible;
            set => _isSettingsVisible = value;
        }

        public PluginUI(Configuration configuration)
        {
            _configuration = configuration;
        }
        
        public void Draw()
        {
            DrawSettingsWindow();
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
        
        public void Dispose()
        {
        }
    }
}