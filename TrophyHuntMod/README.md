# TrophyHuntMod

This is a BepInEx mod for Valheim for streamers doing the Valheim Trophy Hunt that displays discovered/undiscovered trophies at the bottom edge of the screen along with a computed score for the Trophy Hunt based on current scoring rules.

Available here:

https://thunderstore.io/c/valheim/p/oathorse/TrophyHuntMod/

Requires a BepInEx install.

https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/


## Installation (manual)

Simply copy the contents of the archive into the BepinEx/Plugins directory. This is usually found somewhere like 'C:\Program Files (x86)\Steam\steamapps\common\Valheim\BepInEx\plugins'

## What's New?

- Increased the base size of trophies so they read better on screen for the stream audience.

- Added `/trophyscale` console command to allow the user to scale the size of the trophies at the bottom of the screen. Default is 1.0, and can be set as low as 0.1 and as high as you like. This will help adjust trophies to be more readable for streamers at some screen sizes.

	To increase the size of the trophies, hit <enter> to bring up the Chat Console and type `/trophyscale 1.5` for example. This would increase the trophy sizes by 50%

- Made the animation that plays when you collect a trophy more visible by flashing it on and off as well as animating the size. This makes it easier for runners to know when they picked one up without hunting for it on the trophy bar at the bottom.

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
Remove TrophyForestTroll
	- Valheim reports two different troll trophies, TrophyFrostTroll (the one used in the game) and TrophyForestTroll (which isn't used, and doesn't drop.) Both get displayed, even though one isn't actually gettable. 

## Where to Find
You can find the github at: https://github.com/smariotti/TrophyHuntMod

Note, this was originally built with Jotunn, using their example mod project structure, though Jotunn is no longer a requirement to run it. You just need to have BepInEx installed.
