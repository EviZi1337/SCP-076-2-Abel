using System.ComponentModel;

namespace SCP076_2;

public class AbelConfig
{
    [Description("Chance (0.0 - 1.0) that Abel will spawn at round start")]
    public float SpawnChance { get; set; } = 1f;

    [Description("Abel's max HP")]
    public float MaxHealth { get; set; } = 1500f;

    [Description("Abel's AHP amount")]
    public float ArtificialHealth { get; set; } = 250f;

    [Description("Damage dealt by Abel's blade per hit")]
    public float BladeDamage { get; set; } = 75f;

    [Description("Cooldown in seconds for the dash ability")]
    public float DashCooldown { get; set; } = 10f;

    [Description("How long the dash lasts in seconds")]
    public float DashDuration { get; set; } = 2f;

    [Description("MovementBoost intensity during dash (0-255)")]
    public byte DashIntensity { get; set; } = 100;

    [Description("Cooldown in seconds for the door breaker ability")]
    public float DoorBreakerCooldown { get; set; } = 60f;

    [Description("How long door break stays active in seconds")]
    public float DoorBreakerDuration { get; set; } = 10f;

    [Description("Radius in meters to force-open doors")]
    public float DoorBreakerRadius { get; set; } = 1.5f;

    [Description("Cassie announcement when Abel is contained")]
    public string CassieAnnouncement { get; set; } = "SCP 0 7 6 . 2 CONTAINEDSUCCESSFULLY";

    [Description("Hint shown to all players when Abel is contained")]
    public string ContainedHint { get; set; } = "<color=red><b>SCP-076-2 has been contained.</b></color>";

    [Description("Hint shown to Abel when he spawns")]
    public string SpawnHint { get; set; } = "<color=red><b>You are SCP-076-2 (Abel). Eliminate all threats.</b></color>";

    [Description("Broadcast shown to Abel when he spawns")]
    public string SpawnBroadcast { get; set; } = "<b>You are <color=red>SCP-076-2</color></b>";

    [Description("Hint shown when someone picks up Abel's blade")]
    public string BladePickupHint { get; set; } = "<color=yellow><b>You picked up Abel's blade!</b></color>";

    [Description("Hint shown when Abel reclaims his blade")]
    public string BladeReclaimedHint { get; set; } = "<color=red><b>Abel reclaimed his blade...</b></color>";

    [Description("Duration in seconds of the spawn hint")]
    public float SpawnHintDuration { get; set; } = 5f;
}