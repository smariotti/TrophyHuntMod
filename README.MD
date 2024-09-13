# TrophyHuntMod

This is a BepInEx mod for Valheim for streamers doing the Valheim Trophy Hunt that displays discovered/undiscovered trophies at the bottom edge of the screen along with a computed score for the Trophy Hunt based on current scoring rules.

Available here:

https://thunderstore.io/c/valheim/p/oathorse/TrophyHuntMod/

Requires a BepInEx install.

https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/


## Installation (manual)

Two Options:
- Use r2modman to automatically install it from Thunderstore and launch Valheim. R2modman is the mod manager available for download at https://thunderstore.io
- Manual: Simply copy the contents of the archive into the BepinEx/Plugins directory. This is usually found somewhere like 'C:\Program Files (x86)\Steam\steamapps\common\Valheim\BepInEx\plugins' if you've installed BepInEx according to the instructions. (https://www.nitroserv.com/en/guides/installing-mods-on-valheim-with-bepinex)

## What's New?

v0.3.3
- Added `/scorescale` chat console command to alter the score text size. 1.0 is the default, can go as low or high as you like. Use `/scorescale 1.5` to increase the text size by 50%. Thanks @turbero.
- Added `/trophyspacing` chat console command to pack them closer together or farther apart at the bottom of the screen. Negative values pack them tighter, positive ones space them out. 1.5 looks pretty good for me at 1920x1080 running in a window. YMMV. Thanks @Daizzer.
- Animate the discovered trophies upwards while they pulsate and flash to make them **even more** obvious and eye-catching for players.

## Previous Changes

v0.3.2
- Fixed UI text overrun on overall Luck tooltip for long enemy names
- Don't display luck rating in Trophy or Luck-O-Meter tooltips if not enough enemies have been killed to really tell
- added `/showtrophies` chat console command to toggle show/hide of trophy icons, Score, Deaths and Luck counters still display

v0.3.1
- Simplified the Luck-o-Meter and added luckiest and unluckiest trophies to the tooltip

v0.3.0
- Added "Luck-o-Meter" as suggested by @da_Keepa. 
	- Luck icon is on the left of the HUD above the Deaths counter
	- Hover text shows luck percentage as well as overall Luck Rating
	- Individual Trophy icons how show Luck Rating for that type of trophy
	- Luck is calculated as actual drop rates versus documented droprates. Overall luck is aggregate luck for all trophy capabable enemies that have died at least once.
- Trophy Rush Changes (Experimental Feature)
	- Fixed a bug that was preventing trophies from spawning in all circumstances
	- Added TrophyRush button to Main Menu below new logo position
- Made trophy icons animate a little longer when they're picked up
- Fixed a display-only bug where -10 would display as your starting score if you were playing another character, logged out, and made a new one. This just displayed wrong, and would correct itself on next score update and didn't ACTUALLY count against your score.
- Adjusted un-found Trophy icons in the tray to be more readable, is this better? Let me know
- Reduced size of TrophyHuntMod logo and moved it to the right side of the screen as suggested by @Kr4ken92 to play nicer with other mods

v0.2.4
- Swanky new Main Menu logo displaying "Trophy Hunt!" and the mod's version number
- Added tooltips to the Trophy icons at the bottom screen if you pause the game (ESC) and hover the mouse over them.
	- Only available in-game at the Pause (ESC) menu (not in-play with the Inventory screen open!)
	- This displays:
		- Trophy name
		- The number of enemies killed that could drop that Trophy
		- Number of trophies actually dropped by those enemies
		- Actual drop percentage
		- Wiki-documented drop percentage
- Experimental F5 console command `trophyrush` at the main menu, which enables Trophy Rush Mode.
	- Trophy Rush mode causes every enemy that WOULD drop a Trophy to drop a Trophy 100% of the time. This was suggested by @FizzyP as a potential new trophy hunt contest type so it's in there for experimentation.
	- This can only be enabled at the Main Menu via the F5 console command
	- Unfound Trophies will be colored RED in the hud to indicate Trophy Rush is enabled.
	- NOTE! This is the ONLY feature of TrophyHuntMod which modifies the behavior of Valheim. Please use with caution!

v0.2.3
- Removed "TrophyDraugrFem" from the trophy list since it's not supported in the game and does not drop.
- Decreased default HUD trophy size slightly
- TrophyHuntMod now detects whether it's the only mod running and reports this to the log file and displays the score in light blue instead of yellow.
	Yellow score means it's the only mod, which is required for the Trophy Hunt events.
	Light Blue score means other mods are present.
- Corrected the readme which listed the trophy HUD scaling command as `trophysize` instead of `/trophyscale` which is the correct command.

v0.2.2
- Increased the base size of trophies so they read better on screen for the stream audience.
- Added `/trophyscale` console command to allow the user to scale the size of the trophies at the bottom of the screen. Default is 1.0, and can be set as low as 0.1 and as high as you like. This will help adjust trophies to be more readable for streamers at some screen sizes.
	To increase the size of the trophies, hit <enter> to bring up the Chat Console and type `/trophyscale 1.5` for example. This would increase the trophy sizes by 50%
- Made the animation that plays when you collect a trophy more visible by flashing it on and off as well as animating the size. This makes it easier for runners to know when they picked one up without hunting for it on the trophy bar at the bottom.

## Trophy Hunt Mod Features

Displays a tray at the bottom of the game screen with the computed Trophy Hunt score on the left, and each Trophy running to the right. Trophies are grouped by Biome, and are displayed in silhouette when not yet acquired, and in full color when acquired.

A death counter appears to the left of the health and food bar, as deaths count against point totals in Trophy Hunt.

### Console Commands

`/trophyhunt`

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

`/showpath`

	This will display pins on the in-game Map showing the path that the Player has traveled during the session. One pin every 100 meters or so.

`/trophyscale`

	This allows the user to scale the trophy sizes (1.0 is default) for better readability at some screen resolutions. 

`/trophyspacing`

	Allows you space out the trophies to your liking. Negative values spaces them tighter, positive values space them out more. They may wrap off the end of the screen with large values.

`/scorescale`

	Allows the user to scale the Score text size (1.0 is default) for better readability at some screen resolutions.

`/showtrophies`

	Toggles the display of Trophy icons at the bottom of the screen for when you can't even, or the display conflicts with other mods

*Experimental F5 Console Command*

`trophyrush`

	Experimental F5 console command `trophyrush` at the main menu, which enables Trophy Rush Mode.
		- Trophy Rush mode causes every enemy that WOULD drop a Trophy to drop a Trophy 100% of the time. This was suggested by @FizzyP as a potential new trophy hunt contest type so it's in there for experimentation.
		- This can only be enabled at the Main Menu via the F5 console
		- Unfound Trophies will be colored RED in the hud to indicate Trophy Rush is enabled.
		- NOTE! This is the ONLY feature of TrophyHuntMod which modifies the behavior of Valheim. Please use with caution!

## Support the Valheim Speedrunning Community!
If you'd like to donate a dollar or two to the speedrunners and the Trophy Hunt Events, please consider donating via CashApp or PayPal. All the money goes directly into the prize pool for future Trophy Hunt events! 

You can learn more on the Valheim Speedrun Discord channel here: https://discord.gg/9bCBQCPH

	CashApp: $ARCHYCooper 
	PayPal: https://www.paypal.com/paypalme/expertarchy

## Known issues

## Feature Requests

- Report score and trophies to the valheim.help tracker during runs
- JLPL - Add console command to show/hide the trophy icons at the bottom of the screen
- Daizzer - /trophyspacing command


## Where to Find
You can find the github at: https://github.com/smariotti/TrophyHuntMod

Note, this was originally built with Jotunn, using their example mod project structure, though Jotunn is no longer a requirement to run it. You just need to have BepInEx installed.
