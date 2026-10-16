
using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace LowHPNotifier.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    // Prevent repeated notifications while the alert is active
    private bool hasAlerted = false;

    public MainWindow(Plugin plugin, string goatImagePath)
        : base("LowHPNotifier")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("Smart Low-HP Notifier");

        if (ImGui.Button("Settings"))
        {
            plugin.ToggleConfigUi();
        }

        ImGui.Separator();

        // Critical HP threshold slider
        int criticalThreshold = plugin.Configuration.CriticalHPThreshold;

        if (ImGui.SliderInt("Critical HP Threshold", ref criticalThreshold, 1, 100))
        {
            plugin.Configuration.CriticalHPThreshold = criticalThreshold;
            plugin.Configuration.Save();
        }

        // HP difference threshold slider
        int differenceThreshold = plugin.Configuration.HPDifferenceThreshold;

        if (ImGui.SliderInt("HP Difference Threshold", ref differenceThreshold, 1, 100))
        {
            plugin.Configuration.HPDifferenceThreshold = differenceThreshold;
            plugin.Configuration.Save();
        }

        ImGui.Separator();
        ImGui.Text("Party Members:");

        float lowestHP = 100f;
        float secondLowestHP = 100f;
        int livingMembers = 0;
        string lowestHPPlayer = "";

        foreach (var member in Plugin.PartyList)
        {
            if (member.MaxHP == 0)
                continue;

            float hpPercent = (float)member.CurrentHP / member.MaxHP * 100f;

            ImGui.Text(
                $"{member.Name.TextValue}: {member.CurrentHP} / {member.MaxHP} HP ({hpPercent:F1}%)"
            );

            // Ignore dead players
            if (member.CurrentHP == 0)
                continue;

            livingMembers++;

            if (hpPercent < lowestHP)
            {
                secondLowestHP = lowestHP;
                lowestHP = hpPercent;
                lowestHPPlayer = member.Name.TextValue;
            }
            else if (hpPercent < secondLowestHP)
            {
                secondLowestHP = hpPercent;
            }

            // Basic low-HP indicator
            if (hpPercent < plugin.Configuration.CriticalHPThreshold)
            {
                ImGui.SameLine();
                ImGui.Text("LOW HP!");
            }
        }

        ImGui.Separator();

        if (livingMembers >= 2)
        {
            float hpDifference = secondLowestHP - lowestHP;

            ImGui.Text($"Lowest party HP: {lowestHP:F1}%");
            ImGui.Text($"Second-lowest HP: {secondLowestHP:F1}%");
            ImGui.Text($"HP Difference: {hpDifference:F1} percentage points");
        }
        else
        {
            ImGui.Text("Not enough living party members to compare HP.");
        }

        // Smart low-HP detection
        bool shouldAlert =
            livingMembers >= 2 &&
            lowestHP < plugin.Configuration.CriticalHPThreshold &&
            (secondLowestHP - lowestHP) >= plugin.Configuration.HPDifferenceThreshold;

        if (shouldAlert)
        {
            ImGui.Text($"ISOLATED LOW HP DETECTED: {lowestHPPlayer}!");

            // Show toast only once per alert event
            if (!hasAlerted)
            {
                Plugin.ToastGui.ShowNormal(
                    $"LOW HP: {lowestHPPlayer} ({lowestHP:F1}%)"
                );

                hasAlerted = true;
            }
        }
        else
        {
            // Allow another alert after the condition clears
            hasAlerted = false;
        }
    }

}
