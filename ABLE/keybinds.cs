using LabApi.Features.Wrappers;
using SecretAPI.Features.UserSettings;
using UnityEngine;

namespace SCP076_2;
internal static class AbelSettingsHeader
{
    //dont even ask me how this works i was literally hallucinating
    public static readonly CustomHeader Header = new CustomHeader("SCP-076-2", hint: "Abilities for SCP-076-2 (Abel)");
}
public class DashKeybind : CustomKeybindSetting
{
    public override CustomHeader Header => AbelSettingsHeader.Header;

    protected override CustomSetting CreateDuplicate() => new DashKeybind();
    public DashKeybind() : base(7601, "Dash", KeyCode.LeftShift, true, false, string.Empty) { }

    protected override void HandleSettingUpdate()
    {
        // let him cook (or crash, idc)
        if (IsPressed && KnownOwner != null && AbelPlugin.Instance?.Handler?.Abel == KnownOwner)
            AbelPlugin.Instance?.Handler?.TriggerDash(KnownOwner);
    }

    protected override bool CanView(Player player) => AbelPlugin.Instance?.Handler?.Abel == player;
}
public class DoorBreakerKeybind : CustomKeybindSetting
{
    // this logic is actually cursed, delete this later (i wont)
    protected override void HandleSettingUpdate()
    {
        if (IsPressed && KnownOwner != null && AbelPlugin.Instance?.Handler?.Abel == KnownOwner)
            AbelPlugin.Instance?.Handler?.TriggerDoorBreak(KnownOwner);
    }

    public DoorBreakerKeybind() : base(7602, "Force Doors", KeyCode.F, true, false, string.Empty) { }

    public override CustomHeader Header => AbelSettingsHeader.Header;

    protected override CustomSetting CreateDuplicate() => new DoorBreakerKeybind();

    // if u ping me for this on discord im actually blocking u fr
    protected override bool CanView(Player player) => AbelPlugin.Instance?.Handler?.Abel == player;
}