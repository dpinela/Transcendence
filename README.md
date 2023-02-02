A mod for Hollow Knight that adds 15 new charms.

Can work with or without the randomizer: if it is in use and charms are randomized,
the charms are shuffled into the pool, otherwise they are placed at set hidden
locations.

See [SPOILERS.md](SPOILERS.md) for a description of what the charms do and their vanilla locations and notch costs.

The charm icons were made by Tsira ([fiverr][], [Twitch][]).

[fiverr]: https://www.fiverr.com/share/yxvdl5
[Twitch]: https://www.twitch.tv/tsira_kura

# Configuration options

## Mod menu

The following options are available in the mod's global settings menu (under "Mods" in the pause menu):

- **Chaos Mode**: starts the game with a 0-cost Chaos Orb permanently equipped. Applies to all game modes,
  including the randomizer. ItemChangerDataLoader packs use the setting in place when they were *created*,
  instead of the current value.
- **Chaos HUD**: displays the icons of the charms (if any) that Chaos Orb is granting on screen.
- **Chaos HUD Horizontal Position**: specifies whether the HUD should be aligned horizonally to the left edge
  middle, or right edge of the screen.
- **Chaos HUD Vertical Position**: specifies whether the HUD should be aligned vertically to the top edge,
  middle, or bottom edge of the screen.
- **Chaos HUD Orientation**: selects whether HUD icons are arranged vertically or horizontally.

## Randomizer connection menu

The following options are available in the mod's connection menu, accessible from "Connections" > "Transcendence"
in the main randomizer menu. They have no effect on non-randomizer save files.

- **Add Charms**: determines whether the Transcendence charms are added to the item pool.
- **Increase Max Charm Cost By**: if Add Charms is on, increases the maximum number of charms that Salubra may
  require to buy her items, by up to 15.
- **Logic Options**: allows the abilities given by some Transcendence charms to be considered by the
  randomizer logic. See [LOGICSPOILERS.md](LOGICSPOILERS.md) for details on what each option does.

To avoid revealing the names of the charms prematurely, they are hidden in the Logic Options menu until
you obtain the charm in-game.

# Dependency Mods

- ItemChanger
- SFCore
- MagicUI

# License

The code for this mod is (un)licensed under the Unlicense.