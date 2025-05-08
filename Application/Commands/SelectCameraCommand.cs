using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PtzJoystickControl.Core.Model;
using PtzJoystickControl.Application.Devices;
using PtzJoystickControl.Application.Services;


namespace PtzJoystickControl.Application.Commands;

public class SelectCameraCommand : IStaticCommand, INotifyPropertyChanged, INotifyCollectionChanged
{

    private readonly WebSocketHandler _webSocketHandler;

    public SelectCameraCommand(IGamepad gamepad, WebSocketHandler webSocketHandler) : base(gamepad)
    {
        _webSocketHandler = webSocketHandler;
        Cameras = gamepad.Cameras ?? new ObservableCollection<ViscaDeviceBase>();
        gamepad.PropertyChanged += Gamepad_PropertyChanged;
        NotifyClients(Gamepad.SelectedCamera?.Name);

    }

    private void Gamepad_PropertyChanged(object? sender, PropertyChangedEventArgs? e)
    {
        if (e?.PropertyName == nameof(Gamepad.Cameras))
            Cameras = Gamepad.Cameras ?? new ObservableCollection<ViscaDeviceBase>();

        // Add this block to handle changes to SelectedCamera
        //if (e?.PropertyName == nameof(Gamepad.SelectedCamera))
        //    NotifyClients(Gamepad.SelectedCamera?.Name);
    }

    private async void NotifyClients(string? cameraName)
    {
        Console.WriteLine($"NotifyClients called with cameraName: {cameraName}");

        if (cameraName != null)
        {
            try
            {
                var message = new WebSocketMessage
                {
                    Type = "event",
                    Action = "selectedCameraChanged",
                    Payload = new { CameraName = cameraName }
                };

                string formattedMessage = WebSocketMessageFormatter.Serialize(message);

                Console.WriteLine($"Broadcasting camera change: {formattedMessage}");
                await _webSocketHandler.NotifyClientsAsync(formattedMessage);
                Console.WriteLine("Broadcast complete");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error broadcasting camera: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
        else
        {
            Console.WriteLine("Not broadcasting - camera name is null");
        }
    }

    private ObservableCollection<ViscaDeviceBase> cameras = null!;
    private ObservableCollection<ViscaDeviceBase> Cameras
    {
        get { return cameras; }
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (cameras != null)
            {
                cameras.CollectionChanged -= OnCamerasChanged;
                foreach (var camera in cameras)
                    camera.PropertyChanged -= OnDeviceChange;
            }

            cameras = value;
            
            cameras.CollectionChanged += OnCamerasChanged;
            foreach (var camera in cameras)
                camera.PropertyChanged += OnDeviceChange;
            NotifyPropertyChanged();
        }
    }

    public void OnCamerasChanged(object? o, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Move && (e.OldItems?.Contains(Gamepad.SelectedCamera) ?? false))
            Gamepad.SelectedCamera = null;

        if (e != null)
        {
            if (e.OldStartingIndex >= 0)
                foreach (ViscaIpDevice device in e.OldItems!) device.PropertyChanged -= OnDeviceChange;

            if (e.NewStartingIndex >= 0)
                foreach (ViscaIpDevice device in e.NewItems!) device.PropertyChanged += OnDeviceChange;
        }

        NotifyCollectionChanged(e!);
    }

    private void OnDeviceChange(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViscaDeviceBase.Name))
            NotifyPropertyChanged(nameof(Options));
    }

    public override string CommandName => "Select camera";

    public override string AxisParameterName => "Camera";

    public override string ButtonParameterName => "Camera";

    public override IEnumerable<CommandValueOption> Options => Cameras
        .Select((val, i) => new CommandValueOption(val.Name, i));


    public override void Execute(int value)
    {
        if (0 <= value && value < Cameras.Count())
        {
            var camera = Cameras[value];
            Gamepad.SelectedCamera = camera;
            Console.WriteLine($"Gamepad.SelectedCamera updated to: {camera.Name}");

            // Explicitly call NotifyClients to ensure the message is sent
            NotifyClients(camera.Name);
        }
        else
        {
            throw new ArgumentOutOfRangeException(
                $"Value out of range for Camera ObservableCollection. Count is {Cameras.Count()}, value was {value}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void NotifyCollectionChanged(NotifyCollectionChangedEventArgs collectionChangeEventArgs)
    {
        CollectionChanged?.Invoke(this, collectionChangeEventArgs);
    }
}
