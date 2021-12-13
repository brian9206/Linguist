using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;

namespace Linguist
{
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool IsEnable { get; set; } = true;
        public string Language { get; set; } = "en";
        public ICollection<XivChatType> ChatTypes { get; set; } = new List<XivChatType>
        {
            XivChatType.CrossParty,
            XivChatType.Party,
            XivChatType.Alliance
        };

        [NonSerialized]
        private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            pluginInterface?.SavePluginConfig(this);
        }
    }
}