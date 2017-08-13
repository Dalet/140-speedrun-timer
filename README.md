# Speedrun Timer Mod
![screenshot](http://i.imgur.com/gsssoOt.png)

# Installation
[Download page](https://github.com/Dalet/140-speedrun-timer/releases/)
* **Installer**
  * Select the game folder (if not auto-detected) and click install
* **Manual installation** (removed since v0.6)
  * Open the game folder and extract the DLLs in `140_Data/Managed`

# Features
* Speedrun timer with or without loads
* Quick reset
* [LiveSplit](http://livesplit.org/) synchronization: in-game time, autosplitting
* Cheats for practice

# Keybinds
|   Key   |  Description |
| ------- | ------------ |
| `R`     | Double tap to reset |
| `F1`    | Show the Real Time |
| `F2`    | Hide the timers |

### Cheats

|   Key   |  Description |
| ------- | ------------ |
| `Shift` + `F12` | Enable cheats |
| `1`-`9` | Teleport to the `n`<sup>th</sup> BeatLayerSwitch and activate it |
| `Alt` + `1`-`3` | Load the `n`<sup>th</sup> level (`Right Alt` for Mirrored) |
| `Delete` | Teleport to the current checkpoint |
| `Page Up` | Teleport to the next checkpoint |
| `Page Down` | Teleport to the previous checkpoint |
|  `Home` | Teleport to the first checkpoint |
|  `End`  | Teleport to the last checkpoint |
| `Backspace` | Flip the player color |
| `Q`         | Suicide              |
| `I`         | Toggle invincibility |

# Configuration

## Command line arguments

On Steam, they can be set via the "Set launch options..." button in the game's properties. More information about this here: https://support.steampowered.com/kb_article.php?ref=1040-JWMT-2947

| Argument (case-sensitive) | Details                            |
|---------------------------|------------------------------------|
| `-disable-livesplit-sync` | Disables LiveSplit synchronization |
