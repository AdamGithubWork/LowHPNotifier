using Dalamud.Configuration;
using System;

namespace LowHPNotifier;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public int CriticalHPThreshold { get; set; } = 30;
    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;
    public int HPDifferenceThreshold { get; set; } = 25;
    // The below exists just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
