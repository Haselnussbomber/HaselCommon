using Dalamud.Game.ClientState.GamePad;
using Dalamud.Plugin.Services;

namespace HaselCommon.Services;

public class GamepadService(IGamepadState gamepadState, IGameConfig gameConfig)
{
    // Mapping between SystemConfigOption and Dalamuds GamepadButtons
    private readonly (string, GamepadButtons)[] _mapping =
    [
        ("PadButton_Triangle", GamepadButtons.North),
        ("PadButton_Circle", GamepadButtons.East),
        ("PadButton_Cross", GamepadButtons.South),
        ("PadButton_Square", GamepadButtons.West)
    ];

    public GamepadButtons GetButton(GamepadBinding binding)
    {
        var bindingName = binding.ToString();

        foreach (var (configOption, gamepadButton) in _mapping)
        {
            if (!gameConfig.System.TryGet(configOption, out string value))
                continue;

            if (value == bindingName)
                return gamepadButton;
        }

        return GamepadButtons.South; // Default
    }

    public bool IsPressed(GamepadBinding binding)
        => gamepadState.Pressed(GetButton(binding)) == 1;
}

public enum GamepadBinding
{
    Jump,
    Accept,
    Cancel,
    Map_Sub,
    MainCommand,
    HUD_Select
}
