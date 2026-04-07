using System;
using LabApi.Loader.Features.Plugins;
using SecretAPI.Features.UserSettings;

namespace SCP076_2;

public class AbelPlugin : Plugin<AbelConfig>
{
    public static AbelPlugin Instance { get; private set; } = null!;

    public override string Name => "SCP-076-2";
    public override string Description => "Adds SCP-076-2 Abel as a playable SCP";
    public override string Author => "EviZi1337";
    public override Version Version => new(1, 0, 0);
    public override Version RequiredApiVersion => new(LabApi.Features.LabApiProperties.CompiledVersion);

    public AbelHandler Handler { get; private set; } = null!;

    private DashKeybind _dashKeybind = null!;
    private DoorBreakerKeybind _doorKeybind = null!;

    public override void Enable()
    {
        Instance = this;
        Handler = new AbelHandler(this);
        Handler.Subscribe();

        _dashKeybind = new DashKeybind();
        _doorKeybind = new DoorBreakerKeybind();
        CustomSetting.Register(_dashKeybind, _doorKeybind);
    }

    public override void Disable()
    {
        Handler.Unsubscribe();
        CustomSetting.UnRegister(_dashKeybind, _doorKeybind);

        Handler = null!;
        _dashKeybind = null!;
        _doorKeybind = null!;
        Instance = null!;
    }
}