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

v0.5.6
- Fixed a bug where `/showalltrophystats` would persist when switching characters and/or game modes
- Enabled reporting of gamemode to Tracker backend at valheim.help for future experiments and embiggenings
- Added clarifying UI text for Trophy Rush if using it on an existing world.

## Previous Changes

v0.5.5
- Added Biome Completion Bonuses!
	- Implemented this so we can try it out and see what we think. Using suggested score values from @Archy
	- Adds additional points for completing *all* of the trophies in a given Biome (bosses and Hildir quest trophies excluded)
	    ```+20  Meadows
	    +40  Black Forest
	    +40  Swamp
	    +60  Mountains
	    +60  Plains
	    +80  Mistlands
	    +100 Ashlands
	- Added festive animation to Trophy icons for when you complete a Biome. You spin me right 'round, baby, right 'round.
	- Thanks to @Warband for the suggestion of Biome Bonuses!
- Added Biome Bonus tally to Score tooltip
- Removed "Total Score" from Score tooltip since it was redundant and took up space needed for Biome Bonus tallying
- Fixed a bug with Score tooltip where it got cut off on the left side of the screen.

v0.5.0
- Official support for two Game Modes (**Trophy Hunt**, and **Trophy Rush**) in UI and HUD
	- "Toggle Trophy Rush" button on Main Menu replaced with "Toggle Game Mode" which cycles between Trophy Hunt and Trophy Rush game modes
	- Game Mode Rules are listed under the game mode on the Main Menu including Logout and Death penalties
	- Trophy Rush
		- Creating new world with this option enabled will default to Trophy Rush settings in World Modifiers
			- Resources x3
			- Very Hard Combat
			- Logout Penalty: -5 points
			- Death Penalty: -5 points
		- Once the world is created, you can still change World Modifiers for the world however you like, but having Trophy Rush enabled when creating a New World will create a world with the above modifiers by default.
	- Trophy Hunt
		- Standard "Normal Settings" world modifiers are applied as per normal for Valheim
			- Resources x1
			- Normal Combat
			- Logout Penalty: -10 points
			- Death Penalty: -20 points
	- Wrote code to report game mode to online Tracker along with other data, disabled for now
	- No longer color score green in Trophy Rush showing that it's invalid for tournament play, since this is being considered for a tournament variant. Unfound trophy icons are still dark red, indicating Trophy Rush is enabled.
- Reordered the Trophy icons in the HUD by moving Ocean (Serpent) and the four Hildir's Quest trophies to the end of the list. Thanks for the suggestion @Warband
- Added Score Tooltip showing score breakdown and penalty costs for the current game mode (Trophy Hunt or Trophy Rush)
- Show All Trophy Stats Feature
	- Added `/showalltrophystats` Chat Console command to replace the poorly-named and hard to find `/showallenemydeaths` console command. Thanks @Spazzyjones and @Threadmenace for having a hard time finding it.
	- Removed the button from the Main Menu since the command line toggle is available in game.
	- Fixed a bug where the tooltip would lose its shit and flash erratically when hovering over a trophy with `/showalltrophystats` enabled.
- Fixed visual bug with Logouts count where it wouldn't update when you first started playing a new character if you'd previous had a logout on another one (actual score was still correct, though)
- Fixed formatting error with Luck-O-Meter tooltip on luckiest and unluckiest percentages where it would display "a million trillion" decimal places.
- Fixed Luck-O-Meter tooltip getting clipped off the left edge of the screen
- Increased Score font size slightly
- Made trophy pickup animation even more eye catching for better stream visibility

v0.4.1
- A few visual tweaks to the Relog counter icon
- Fixed a bug with animating Trophy offset

v0.4.0
- Killed/Trophy-drop Tooltips Nerfed!
	- These were discussed and determined to be OP.
	- Killed/Trophy Drop tooltips *now use PLAYER enemy kills and PLAYER trophy pickups* rather than world enemy deaths and world trophy drops!
		- This was done to prevent a cheese where you could monitor world trophy drops after, say, dragging Growths over to a Fuling village, letting madness ensue, and then checking world trophy drops to see if you should run over there and hunt for trophies.
		- For a Trophy to count towards stats in the tooltips, it must be picked up into your inventory
	- Luck-O-Meter now only counts Player enemy kills and picked up Trophy drops when calculating Luck Rating, Luckiest and Unluckies mobs
	- BUT! You can now use `/showallenemydeaths` Chat Console command to see all the info including world kills and world trophy drops (this adds the data we had before back to the tooltips).
		- WARNING: This invalidates your run for Tournament play and colors your score bright green to indicate this. EX: Use this at the end of a run to inspect actual world drops.
	- Added a new button on the main menu "Show All Enemy Deaths" to enable/disable this prior to gameplay for the console-command shy.
	- Note that once it's been enabled your run is invalid even if you disable it again (score remains bright green.)
- Added "Logs: X" text to HUD to display how many Relogs have been done. Much requested. Note that `/trophyhunt` also still displays this information in the chat console and log file.
- Added `/ignorelogouts` Chat Console command to make it so that logouts no longer count against your score. (@gregscottbailey request)
	- WARNING: This invalidates the run for Tournament play. "Logs:" text will display dimmed and score bright green if you use this.
- Changed Trophy animation when you get your first trophy to read better on streams (Yes, again. I think it's less jarring AND more visible now.)
- Repositioned Deaths counter on the HUD to read better and take up less space
- Removed Luck-O-Meter "Luck" text and repositioned icon in HUD
- Reworked Luck-O-Meter tooltip to make it easier to read and less deluxe
- Added black outline to Score text to make it more readable against light backgrounds like the Mountains or staring at the sun.
- Fixed a bug where logging out, deleting your character, creating a new character with the same name and entering play would retain old player data for kills, trophy drops and /showpath pins (thanks @da_Keepa and @Xmal!)
- Fixed a bug with Luck Rating where my logic was reversed and it would display luck ratings if no luck was calculable yet
- Fixed a bug where animating trophy would drift upwards on screen if pausing while it was flashing

v0.3.3
- Added `/scorescale` chat console command to alter the score text size. 1.0 is the default, can go as low or high as you like. Use `/scorescale 1.5` to increase the text size by 50%. Thanks @turbero.
- Added `/trophyspacing` chat console command to pack them closer together or farther apart at the bottom of the screen. Negative values pack them tighter, positive ones space them out. 1.5 looks pretty good for me at 1920x1080 running in a window. YMMV. Thanks @Daizzer.
- Animate the discovered trophies upwards while they pulsate and flash to make them **even more** obvious and eye-catching for players.

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

`/showalltrophystats` 

	Chat Console command to see all the info including world kills and world trophy drops (this adds the data we had before back to the tooltips).
	- WARNING: This invalidates your run for Tournament play and colors your score bright green to indicate this.

`/ignorelogouts`
	
	Chat Console command to make it so that logouts no longer count against your score. 
	- WARNING: This invalidates your run for Tournament play and colors your score bright green and fades Logs: text to gray to indicate this.

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
- Dropshadow or add dark background field to Score (Weih (Henrik))
- Collect player kills/drops as default, enable all kills/drops as options


## Where to Find
You can find the github at: https://github.com/smariotti/TrophyHuntMod

Note, this was originally built with Jotunn, using their example mod project structure, though Jotunn is no longer a requirement to run it. You just need to have BepInEx installed.
