using PtzJoystickControl.Application.Commands;
using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Services;

namespace PtzJoystickControl.Application.Services;

public class CommandsService : ICommandsService
{
    private readonly WebSocketHandler _webSocketHandler;

    public CommandsService(WebSocketHandler webSocketHandler)
    {
        _webSocketHandler = webSocketHandler;
    }
    public IEnumerable<ICommand> GetCommandsForGamepad(IGamepad gamepad)
    {
        return new ICommand[]
        {
            new PanCommand(gamepad),
            new TiltCommand(gamepad),
            new ZoomCommand(gamepad),
            new FocusMoveCommand(gamepad),
            new FocusModeCommand(gamepad),
            new FocusLockCommand(gamepad),
            new PresetCommand(gamepad),
            new PresetRecallSpeedComamnd(gamepad),
            new SelectCameraCommand(gamepad, _webSocketHandler),
            new PowerCommand(gamepad)
        };
    }
}
