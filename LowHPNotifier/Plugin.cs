using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using LowHPNotifier.Windows;
using System.Collections.Generic;
namespace LowHPNotifier;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IPartyList PartyList { get; private set; } = null!;
    [PluginService]
    internal static IToastGui ToastGui { get; private set; } = null!;
    [PluginService]
    internal static IFramework Framework { get; private set; } = null!;
    private const string CommandName = "/hp";
    private readonly HashSet<string> alertedPlayers = new();

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("LowHPNotifier");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Framework.Update += OnFrameworkUpdate;
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // You might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        // Tell the UI system that we want our windows to be drawn through the window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // This adds a button to the plugin installer entry of this plugin which allows
        // toggling the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        // Adds another button doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [SamplePlugin] ===A cool log message from Sample Plugin===
        Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");
    }

    public void Dispose()
    {
        // Unregister all actions to not leak anything during disposal of plugin
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
        Framework.Update -= OnFrameworkUpdate;
    }

    private void OnCommand(string command, string args)
    {
        // In response to the slash command, toggle the display status of our main ui
        MainWindow.Toggle();
    }
    
    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
    private void OnFrameworkUpdate(IFramework framework)
    {
        float lowestHP = 100f;
        float secondLowestHP = 100f;
        int livingMembers = 0;
        string lowestHPPlayer = "";

        foreach (var member in PartyList)
        {
            if (member.MaxHP == 0 || member.CurrentHP == 0)
                continue;

            float hpPercent = (float)member.CurrentHP / member.MaxHP * 100f;

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
        }

        bool shouldAlert =
            livingMembers >= 2 &&
            lowestHP < Configuration.CriticalHPThreshold &&
            (secondLowestHP - lowestHP) >= Configuration.HPDifferenceThreshold;

        if (shouldAlert)
        {
            if (alertedPlayers.Add(lowestHPPlayer))
            {
                ToastGui.ShowNormal(
                    $"LOW HP: {lowestHPPlayer} ({lowestHP:F1}%)"
                );
            }
        }
        else
        {
            alertedPlayers.Clear();
        }
    }
}
