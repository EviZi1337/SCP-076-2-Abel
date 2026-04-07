# SCP-076-2 - Abel

A [LabAPI](https://github.com/northwood-studios/LabAPI) plugin that yeets **SCP-076-2 (Abel)** into your server as a playable SCP using the Tutorial role. My 3rd LabAPI plugin, no I will not be taking questions.

> Needs **LabAPI** and **SecretAPI**.

---

## What does this thing even do

When a round starts, the plugin rolls the dice and converts some random Class-D, Scientist, or Facility Guard into Abel, he gets teleported into HCZ-127.

### Stats (no you can't negotiate these, touch the config)
| Property | Default |
|---|---|
| Health | 1500 HP |
| Artificial HP | 250 AHP |
| Scale | 1.15x (a little big, a lotta scary) |
| Spawn room | HCZ-127 |

### The Blade (SCP-1509)
- Hits for **75 damage** per swing
- Someone yoinks it? It gets snatched back to Abel after **3 seconds**
- Abel fumbles it himself? Also comes back in **3 seconds**
- The 1509 shield regen is zeroed out because handing Abel free AHP is unhinged behavior and I'm not allowing it

### Keybinds (in ServerSpecific settings, only Abel can see them)
| Keybind | Default Key | What it does | Cooldown |
|---|---|---|---|
| Dash | Left Shift | MovementBoost for a quick sprint, genuinely busted | 10s |
| Force Doors | F | Rips open every door within 1.5m for 10 seconds | 60s |

### When Abel gets cooked
- CASSIE announces containment (customizable, go wild)
- Plugin resets all state cleanly
- Everybody can go touch grass

---

## Installation

1. Grab the `.dll` from releases
2. Drop it in `LabAPI/plugins/`
3. Boot the server, config auto-generates
4. That's it. You're done. Why are you still reading.

---

## Dependencies

- [LabAPI](https://github.com/northwood-studios/LabAPI)
- [SecretAPI](https://github.com/obvEve/SecretAPI/tree/dev) — for the keybind system, no substitute

---

## Config

```yaml
spawn_chance: 0.4           # 0.0 - 1.0, set to 1.0 if you hate your players
max_health: 1500
artificial_health: 250
blade_damage: 75

dash_cooldown: 10
dash_duration: 2
dash_intensity: 100         # MovementBoost intensity, 0-255

door_breaker_cooldown: 60
door_breaker_duration: 10
door_breaker_radius: 1.5

cassie_announcement: "SCP 0 7 6 . 2 CONTAINEDSUCCESSFULLY"
contained_hint: "<color=red><b>SCP-076-2 has been contained.</b></color>"
spawn_hint: "<color=red><b>You are SCP-076-2 (Abel). Eliminate all threats.</b></color>"
spawn_broadcast: "<b>You are <color=red>SCP-076-2</color></b>"
blade_pickup_hint: "<color=yellow><b>You picked up Abel's blade!</b></color>"
blade_reclaimed_hint: "<color=red><b>Abel reclaimed his blade...</b></color>"
spawn_hint_duration: 5
```

---

## Disclaimer (read this fr)

This plugin works on my machine. It might work on yours. It might explode. I genuinely cannot tell you. If something breaks - and it might - I am not your guy. Open an issue if you want, I'll look at it when I feel like it, which could be never. If Abel is crashing your server, deleting your files, or personally threatening you, the correct response is to turn off your computer and uninstall SCP:SL. Not to ping me. I dont know how is it working.

I wrote this for fun. Use it however you want, modify it, break it, ship it as your own, I don't care. Just don't ping me.

---

## Author

**EviZi1337**
