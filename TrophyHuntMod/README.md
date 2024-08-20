# TrophyHuntMod

This is a BepInEx mod for Valheim for streamers doing the Valheim Trophy Hunt that displays discovered/undiscovered trophies at the bottom edge of the screen along with a computed score for the Trophy Hunt based on current scoring rules.

Available here:

https://thunderstore.io/c/valheim/p/oathorse/TrophyHuntMod/

Requires a BepInEx install.

https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/


## Installation (manual)

Simply copy the contents of the archive into the BepinEx/Plugins directory. This is usually found somewhere like 'C:\Program Files (x86)\Steam\steamapps\common\Valheim\BepInEx\plugins'

## Features

Displays a tray at the bottom of the game screen with the computed Trophy Hunt score on the left, and each Trophy running to the right. Trophies are grouped by Biome, and are displayed in silhouette when not yet acquired, and in full color when acquired.

A death counter appears to the left of the health and food bar, as deaths count against point totals in Trophy Hunt.

### Console Command: `/trophyhunt`

The Chat console (and F5 console) both support the console command `/trophyhunt` which prints out the Trophy Hunt scoring in detail like so:

```
[Trophy Hunt Scoring]
Trophies:
  TrophyBoar: Score: 10 Biome: Meadows
  TrophyDeer: Score: 10 Biome: Meadows
  TrophyNeck: Score: 10 Biome: Meadows
  TrophyEikthyr: Score: 40 Biome: Meadows
  TrophyGreydwarf: Score: 20 Biome: Forest
Trophy Score Total: 90
Penalties:
  Deaths: 2 Score: -40
  Logouts: 0 Score: 0
Total Score: 50
```

### Console Command: `/showpath`

This will display pins on the in-game Map showing the path that the Player has traveled during the session. One pin every 100 meters or so.

## Support the Valheim Speedrunning Community!
If you'd like to donate a dollar or two to the speedrunners and the Trophy Hunt Events, please consider donating via CashApp or PayPal. All the money goes directly into the prize pool for future Trophy Hunt events! 

You can learn more on the Valheim Speedrun Discord channel here: https://discord.gg/9bCBQCPH

	CashApp: $ARCHYCooper 
	PayPal: https://www.paypal.com/paypalme/expertarchy


## Known issues
None at the moment

## Where to Find
You can find the github at: https://github.com/smariotti/TrophyHuntMod

Note, this was originally built with Jotunn, using their example mod project structure, though Jotunn is no longer a requirement to run it. You just need to have BepInEx installed.
