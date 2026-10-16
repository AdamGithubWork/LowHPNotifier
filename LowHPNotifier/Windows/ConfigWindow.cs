using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace LowHPNotifier.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    // We give this window a constant ID using ###.
    // This allows for labels to be dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("A Wonderful Configuration Window###With a constant ID")
    {
        Flags = ImGuiWindowFlags.None;

        Size = new Vector2(450, 250);
        SizeCondition = ImGuiCond.FirstUseEver;

        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
    }

    public override void Draw()
    {
    }
}
