using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using SecretAPI.Features.UserSettings;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;
using Random = UnityEngine.Random;
// BRO IF I SEE ONE MORE NULL REFERENCE EXCEPTION IM QUITTING CODING
namespace SCP076_2;

public class AbelHandler
{
    private readonly AbelPlugin _plugin;

    public Player? Abel { get; private set; }
    private ushort _bladeSerial;
    private bool _bladeReturning;
    private CoroutineHandle _bladeWatchHandle;

    private DateTime _dashCooldownEnd = DateTime.MinValue;
    private DateTime _doorCooldownEnd = DateTime.MinValue;
    private CoroutineHandle _doorCoroutine;

    public AbelHandler(AbelPlugin plugin)
    {
        _plugin = plugin;
    }

    public void Subscribe()
    {
        ServerEvents.RoundStarted += OnRoundStarted;
        ServerEvents.RoundRestarted += OnRoundRestarted;
        PlayerEvents.Spawned += OnPlayerSpawned;
        PlayerEvents.Hurting += OnHurting;
        PlayerEvents.PickingUpItem += OnPickingUpItem;
        PlayerEvents.PickedUpItem += OnPickedUpItem;
        PlayerEvents.DroppedItem += OnDroppedItem;
        PlayerEvents.Dying += OnDying;
        PlayerEvents.ChangingItem += OnChangingItem;
        PlayerEvents.Scp1509Resurrecting += OnScp1509Resurrecting;
        PlayerEvents.Left += OnPlayerLeft;
        PlayerEvents.ChangedRole += OnChangedRole;
    }

    public void Unsubscribe()
    {
        ServerEvents.RoundStarted -= OnRoundStarted;
        ServerEvents.RoundRestarted -= OnRoundRestarted;
        PlayerEvents.Spawned -= OnPlayerSpawned;
        PlayerEvents.Hurting -= OnHurting;
        PlayerEvents.PickingUpItem -= OnPickingUpItem;
        PlayerEvents.PickedUpItem -= OnPickedUpItem;
        PlayerEvents.DroppedItem -= OnDroppedItem;
        PlayerEvents.ChangingItem -= OnChangingItem;
        PlayerEvents.Scp1509Resurrecting -= OnScp1509Resurrecting;
        PlayerEvents.Dying -= OnDying;
        PlayerEvents.Left -= OnPlayerLeft;
        PlayerEvents.ChangedRole -= OnChangedRole;
    }

    private void ResetState()
    {
        if (Abel != null)
        {
            Abel.Scale = Vector3.one;
            Abel.CustomInfo = string.Empty;
            CustomSetting.SendSettingsToPlayer(Abel);
        }

        if (_bladeWatchHandle.IsRunning)
            Timing.KillCoroutines(_bladeWatchHandle);
        if (_doorCoroutine.IsRunning)
            Timing.KillCoroutines(_doorCoroutine);

        Abel = null;
        _bladeSerial = 0;
        _bladeReturning = false;
        _dashCooldownEnd = DateTime.MinValue;
        _doorCooldownEnd = DateTime.MinValue;
    }

    public void TriggerDash(Player player)
    {
        var cfg = _plugin.Config;
        double remaining = (_dashCooldownEnd - DateTime.UtcNow).TotalSeconds;

        if (remaining > 0)
        {
            player.SendHint($"<color=red>Dash cooldown: {remaining:F1}s</color>", 1.5f);
            return;
        }

        _dashCooldownEnd = DateTime.UtcNow.AddSeconds(cfg.DashCooldown);
        player.EnableEffect<CustomPlayerEffects.MovementBoost>(cfg.DashIntensity, cfg.DashDuration);
        player.SendHint("<color=red><b>Dash!</b></color>", cfg.DashDuration);
        Logger.Debug($"Abel dashing: {player.Nickname}");
    }

    public void TriggerDoorBreak(Player player)
    {
        var cfg = _plugin.Config;
        double remaining = (_doorCooldownEnd - DateTime.UtcNow).TotalSeconds;

        if (remaining > 0)
        {
            player.SendHint($"<color=red>Door break cooldown: {remaining:F1}s</color>", 1.5f);
            return;
        }

        _doorCooldownEnd = DateTime.UtcNow.AddSeconds(cfg.DoorBreakerCooldown);

        if (_doorCoroutine.IsRunning)
            Timing.KillCoroutines(_doorCoroutine);

        _doorCoroutine = Timing.RunCoroutine(ForceDoors(player, cfg.DoorBreakerRadius, cfg.DoorBreakerDuration));
        Logger.Debug($"Door break triggered: {player.Nickname}");
    }

//sqrmagnitude is faster because math is scary at 4am
    private IEnumerator<float> ForceDoors(Player abel, float radius, float activeDuration)
    {
        float elapsed = 0f;
        float sqrRadius = radius * radius;
        abel.SendHint("<color=red><b>Door break active!</b></color>", activeDuration);

        while (elapsed < activeDuration)
        {
            if (Abel != abel)
                yield break;

            foreach (var door in Door.List)
            {
                if (door.IsDestroyed)
                    continue;

                if ((door.Position - abel.Position).sqrMagnitude > sqrRadius)
                    continue;

                if (door is BreakableDoor breakable && !breakable.IsDestroyed)
                {
                    if (!breakable.Base.IsDestroyed)
                    {
                        breakable.TryBreak();
                        Logger.Debug($"Broke door {door.DoorName}");
                    }
                }
                else if (!door.IsOpened)
                {
                    door.IsLocked = false;
                    door.IsOpened = true;
                    Logger.Debug($"Forced open {door.DoorName}");
                }
            }

            yield return Timing.WaitForSeconds(0.2f);
            elapsed += 0.2f;
        }

        Logger.Debug("Door break ended");
    }

    private void OnRoundStarted()
    {
        Timing.RunCoroutine(DelayedAbelRoll());
        Logger.Debug("Round started, scheduling Abel roll");
    }

    private IEnumerator<float> DelayedAbelRoll()
    {
        yield return Timing.WaitForSeconds(2f);

        if (Random.value > _plugin.Config.SpawnChance)
        {
            Logger.Debug("No Abel this round");
            yield break;
        }
        var candidates = Player.ReadyList
            .Where(p => p.Role == RoleTypeId.ClassD || p.Role == RoleTypeId.Scientist || p.Role == RoleTypeId.FacilityGuard)
            .ToList();

        if (candidates.Count == 0)
        {
            Logger.Debug("No candidates for Abel");
            yield break;
        }

        Abel = candidates[Random.Range(0, candidates.Count)];
        Abel.SetRole(RoleTypeId.Tutorial, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);
        Logger.Debug($"Abel chosen: {Abel.Nickname}");
    }

    private void OnRoundRestarted()
    {
        ResetState();
        Logger.Debug("Round over, resetting Abel state");
    }

    private void OnPlayerLeft(PlayerLeftEventArgs ev)
    {
        if (Abel == null || ev.Player != Abel)
            return;

        ResetState();
        Logger.Debug("Abel left the server, cleaning up");
    }

    private void OnChangedRole(PlayerChangedRoleEventArgs ev)
    {
        if (Abel == null || ev.Player != Abel)
            return;

        if (ev.NewRole.RoleTypeId == RoleTypeId.Tutorial)
            return;

        ResetState();
        Logger.Debug($"Abel changed role to {ev.NewRole.RoleTypeId}, resetting");
    }

    private void OnPlayerSpawned(PlayerSpawnedEventArgs ev)
    {
        if (Abel == null || ev.Player != Abel)
            return;

        Timing.RunCoroutine(DelayedAbelSetup(ev.Player));
        Logger.Debug($"Abel spawning: {ev.Player.Nickname}");
    }

    private IEnumerator<float> DelayedAbelSetup(Player player)
    {
        yield return Timing.WaitForSeconds(0.1f);

        if (Abel != player)
            yield break;

        player.ClearInventory();

        var room = Room.Get(RoomName.Hcz127).FirstOrDefault();
        if (room != null)
            player.Position = room.Position + Vector3.up * 1.5f;

        player.Scale = new Vector3(1.15f, 1.15f, 1.15f);
        player.MaxHealth = _plugin.Config.MaxHealth;
        player.Health = _plugin.Config.MaxHealth;
        player.MaxArtificialHealth = _plugin.Config.ArtificialHealth;
        player.ArtificialHealth = _plugin.Config.ArtificialHealth;

        var blade = player.AddItem(ItemType.SCP1509);
        if (blade != null)
        {
            _bladeSerial = blade.Serial;

            //zero out shield regen we don't want 1509 handing abel free ah
            if (blade is Scp1509Item scp1509)
            {
                scp1509.ShieldRegenRate = 0f;
                scp1509.EquippedHS = 0f;
                scp1509.RevivedPlayerAOEBonusAHP = 0f;
            }

            player.CurrentItem = blade;
        }

        player.CustomInfo = "SCP-076-2";
        CustomSetting.SendSettingsToPlayer(player);
        player.SendBroadcast(_plugin.Config.SpawnBroadcast, 5, shouldClearPrevious: true);
        player.SendHint(_plugin.Config.SpawnHint, _plugin.Config.SpawnHintDuration);

        if (_bladeWatchHandle.IsRunning)
            Timing.KillCoroutines(_bladeWatchHandle);

        _bladeWatchHandle = Timing.RunCoroutine(BladeWatcher());
        Logger.Debug("Abel ready");
    }
//no linq here were gigachad developers (im literally crying)
    private IEnumerator<float> BladeWatcher()
    {
        while (Abel != null)
        {
            yield return Timing.WaitForSeconds(0.3f);

            if (_bladeSerial == 0)
                continue;

            Item? bladeItem = null;
            foreach (var item in Abel.Items)
            {
                if (item.Serial != _bladeSerial)
                    continue;
                bladeItem = item;
                break;
            }

            if (bladeItem == null)
                continue;

            if (Abel.CurrentItem?.Serial != _bladeSerial)
            {
                Abel.CurrentItem = bladeItem;
                Logger.Debug("BladeWatcher: forced Abel to hold blade");
            }

            if (Abel.ArtificialHealth > _plugin.Config.ArtificialHealth)
                Abel.ArtificialHealth = _plugin.Config.ArtificialHealth;
        }
    }

    private void OnScp1509Resurrecting(PlayerScp1509ResurrectingEventArgs ev)
    {
        if (ev.Player != Abel)
            return;

        ev.IsAllowed = false;
        Logger.Debug("Blocked 1509 resurrection for Abel");
    }

    private void OnPickingUpItem(PlayerPickingUpItemEventArgs ev)
    {
        if (Abel == null || ev.Player != Abel)
            return;
        if (_bladeSerial != 0 && ev.Pickup?.Serial == _bladeSerial)
            return;

        ev.IsAllowed = false;
    }

    private void OnChangingItem(PlayerChangingItemEventArgs ev)
    {
        if (Abel == null || ev.Player != Abel)
            return;

        if (ev.NewItem == null || ev.NewItem.Serial != _bladeSerial)
            ev.IsAllowed = false;
    }

    private void OnHurting(PlayerHurtingEventArgs ev)
    {
        if (Abel == null || ev.Attacker != Abel)
            return;

        if (ev.Player.IsSCP)
        {
            ev.IsAllowed = false;
            return;
        }

        if (ev.DamageHandler is StandardDamageHandler sdh)
            sdh.Damage = _plugin.Config.BladeDamage;
    }

    private void OnPickedUpItem(PlayerPickedUpItemEventArgs ev)
    {
        if (_bladeSerial == 0 || ev.Item?.Serial != _bladeSerial || ev.Player == Abel)
            return;

        ev.Player.SendHint(_plugin.Config.BladePickupHint, 3f);
        Logger.Debug($"{ev.Player.Nickname} yoinked Abel's blade");

        if (!_bladeReturning)
            Timing.RunCoroutine(ReturnBladeFromPlayer(ev.Player));
    }

    private void OnDroppedItem(PlayerDroppedItemEventArgs ev)
    {
        if (_bladeSerial == 0 || ev.Pickup?.Serial != _bladeSerial || ev.Player != Abel)
            return;

        Logger.Debug("Abel dropped blade, scheduling return");

        if (!_bladeReturning)
            Timing.RunCoroutine(ReturnBladePickup(ev.Pickup));
    }

    private void OnDying(PlayerDyingEventArgs ev)
    {
        if (Abel == null || ev.Player != Abel)
            return;

        Announcer.Message(_plugin.Config.CassieAnnouncement, string.Empty);
        ResetState();
        Logger.Debug("Abel is cooked");
    }

    //snatch the blade back idk what to write here fr
    private IEnumerator<float> ReturnBladeFromPlayer(Player holder)
    {
        _bladeReturning = true;
        yield return Timing.WaitForSeconds(3f);

        if (holder == null || Abel == null)
        {
            _bladeReturning = false;
            yield break;
        }

        Item? bladeItem = null;
        foreach (var item in holder.Items)
        {
            if (item.Serial != _bladeSerial)
                continue;
            bladeItem = item;
            break;
        }

        if (bladeItem != null)
        {
            holder.RemoveItem(bladeItem);
            Logger.Debug($"Yanked blade from {holder.Nickname}");
        }

        holder.SendHint(_plugin.Config.BladeReclaimedHint, 3f);

        var newBlade = Abel.AddItem(ItemType.SCP1509);
        if (newBlade != null)
        {
            _bladeSerial = newBlade.Serial;

            if (newBlade is Scp1509Item scp1509)
            {
                scp1509.ShieldRegenRate = 0f;
                scp1509.RevivedPlayerAOEBonusAHP = 0f;
            }

            Abel.CurrentItem = newBlade;
            Logger.Debug($"Blade back with Abel, new serial {_bladeSerial}");
        }

        _bladeReturning = false;
    }

    private IEnumerator<float> ReturnBladePickup(Pickup pickup)
    {
        _bladeReturning = true;
        yield return Timing.WaitForSeconds(3f);

        if (Abel == null)
        {
            _bladeReturning = false;
            yield break;
        }

        if (pickup != null && pickup.IsSpawned)
            pickup.Destroy();

        var newBlade = Abel.AddItem(ItemType.SCP1509);
        if (newBlade != null)
        {
            _bladeSerial = newBlade.Serial;

            if (newBlade is Scp1509Item scp1509)
            {
                scp1509.ShieldRegenRate = 0f;
                scp1509.RevivedPlayerAOEBonusAHP = 0f;
            }

            Abel.CurrentItem = newBlade;
            Logger.Debug($"Blade returned after drop, serial {_bladeSerial}");
        }

        _bladeReturning = false;
    }
}