using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace LowHPNotifier.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly string goatImagePath;
    private readonly Plugin plugin;

    // We give this window a hidden ID using ##.
    // The user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, string goatImagePath)
        : base("My Amazing Window##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.goatImagePath = goatImagePath;
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("Smart Low-HP Notifier");

        ImGui.Separator();

        ImGui.Text("Party Members:");

        foreach (var member in Plugin.PartyList)
        {
            float hpPercent = member.MaxHP > 0 ? (float)member.CurrentHP / member.MaxHP * 100: 0;

            ImGui.Text($"{member.Name.TextValue}: {member.CurrentHP} / {member.MaxHP} HP ({hpPercent:F1}%)");
            if (member.CurrentHP > 0 && hpPercent < 30)
            {
                ImGui.SameLine();
                ImGui.Text("LOW HP!");
            }
        }
    }
}
