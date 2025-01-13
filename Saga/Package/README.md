# Saga Mode

"Saga" is a special Valheim game mode comprised of a collection of rules changes to progress through the standard game more quickly. It removes overnight waits and drudgery to get you to new content faster.

These changes fall into these rough categories:
- Faster game progression
- Faster resource gathering and production
- Faster travel

In about four hours, it's possible for a skilled player to arrive in Ashlands with appropriate gear.

Noteworthy changes:
- Bosses are optional! The progression gating boss items (Swamp Key, Wishbone, Dragon Tears, Torn Spirit, Majestic Carapace) also drop from powerful boss minions in each biome.
- Ores turn into bars when you pick them up, removing the need to smelt anything.
- Some enemies can drop helpful items like Finewood, Yggdrasil Wood, an Iron Mace and even the Megingjord to make progression faster.

No recipes are changed! No combat is changed! It's standard Valheim with a bunch of targeted modifications to go faster through the content. This collection of tweaks has been meticulously tested and repeatedly re-balanced to become greater than a sum of the parts. It's a really fun way to see Valheim quickly.

It's super fun, and a great training mode for working through recipes, production dependencies and biome progression.

## World Modifiers:
- Combat is set to Normal difficulty
- Resource Rate is set to 2x (you get double all resources picked up, dropped by enemies, or found in chests)
- Portals allow **all** items through (metals, eggs, etc.)

## Custom Rules:
- Metal Ores "insta-smelt" when you pick them up. If you pick up ore from the ground or out of a chest, it becomes the equivalent metal bar instantly.
- Mining is twice as productive
- Enhanced Enemy Loot Drops!
	- Greylings drop various useful Meadows and Black Forest items, including Finewood
	- Trolls have a very high chance to drop the Megingjord
	- Biome Boss Minions now have a chance to drop Boss Items
	  - Black Forest: Greydwarf Brute has a chance to drop Swamp Key
	  - Swamp: Oozers have a chance to drop Wishbone
	  - Mountains: Drakes have a chance to drop Dragon Tears
	  - Plains: Fuling Shaman and Berserker have a chance to drop Torn Spirit
	  - Mistlands: Seeker Soldiers have a chance to drop Majestic Carapace
	  - Hildir Mini-bosses always drop their Biome's Boss Item
	- Dverger drop a dozen or so pieces of Yggrasil wood when they die
	- Rancid Remains always drops his mace (Iron Mace, non-poisonous)
- Production buildings that take time to process **all** take only seconds
  - Fermenter
  - Beehive
  - Charcoal Kiln
  - Spinning Wheel
  - Windmill
  - Sap Extractor
  - Eitr Refiner (this also **no longer requires Soft Tissue**, just Sap)
  - NOTE: Cooking buildings are unchanged from vanilla (Cooking Station, Mead Ketill, Iron Cooking Station, Cauldron, Stone Oven)
- Pickaxes are more productive when mining
- All planted seeds and saplings grow to maturity within 10 seconds (as in previous version)
- All Player Skills are learned at level 20 and then increase normally.
- Speed Chickens: Chickens are more promiscuous, fertile and grow up really fast.

That's it!

# Three new Play Modes!

The default is "Casual Saga", a free play Vanilla Valheim feeling mode where the Saga rules are applied.

## Casual Saga

This is a free-play mode. There is no time limit, and there is no scoring. You can simply enjoy Valheim as usual but with all of the custom rules outlined above.

## Trophy Saga

This is a competitive game mode where you have four hours to collect Trophies and score as many points as possible!

In addition to Saga rules above:
- Trophies drop 100% of the time until you have one, then drop at vanilla valheim rate (except Deer, which always drops.)

### Scoring:
- Death penalty is -30 points
- Relog penalty is -10 points

- **Enemies**
  - Meadows Trophies - 10 points each
  - Black Forest & Swamp Trophies - 20 points each
  - Mountain/Plains Trophies - 30 points each
  - Mistland Trophies -  40 points each
  - Ashland Trophies - 50 points each
  - Serpent Trophy - 25 points

- **Mini-Bosses**
  - Brenna Trophy - 25 points
  - Geirrhafa Trophy - 45 points
  - Zil & Thungr Trophies - 65 points

- **Bosses**
  - Eikthyr Trophy - 40 points
  - Elder Trophy - 60 points
  - Bonemass Trophy - 80 points
  - Moder Trophy - 100 points
  - Yagluth Trophy - 120 points
  - Queen/Fader Trophy - 1000 points
    - (Queen/Fader points will get nerfed when/if someone completes)

## Culinary Saga

This is a competitive game mode where you have four hours to cook foods to earn points!

### Scoring:
- Death penalty is -30 points
- Relog penalty is -10 points

- Cook one of each food to score points. 
  - Points:
	- Meadows foods: 10 points
	- Black Forest foods: 20 points
	- Swamp foods: 30 points
	- Mountains foods: 30 points
	- Ocean foods: 40 points
	- Plains foods: 40 points
	- Mistlands foods: 50 points
	- Ashlands foods: 60 points

# Additional Info

The Saga Mode mod is an offshoot of "TrophyHuntMod" for a wider audience. TrophyHuntMod was developed for the Valheim Speedrun Discord channel's official Trophy Hunt tournaments. These tournaments award cash prizes for winning events. These events are held regularly. See the Discord channel (linked below) for more details.

It shares code with the official TrophyHuntMod but does not report scores and can't be used for official events.

## Support the Valheim Speedrunning Community!
If you'd like to donate a dollar or two to the speedrunners and the Trophy Hunt Events, please consider donating via CashApp or PayPal. All the money goes directly into the prize pool for future Trophy Hunt events! 

You can learn more on the Valheim Speedrun Discord channel here: https://discord.gg/9bCBQCPH

	CashApp: $ARCHYCooper 
	PayPal: https://www.paypal.com/paypalme/expertarchy

## Where to Find
You can find the source code on github at: https://github.com/smariotti/TrophyHuntMod

# Release Notes
v0.8.7
- Fixed bug with player path data being restored from previous save when it shouldn't have been

v0.8.6
- Fixed bug where fishing bait was dropping outside of Culinary Saga game mode

v0.8.5
- Battering Ram no longer eats through wood at an alarming rate and now operates normally.
- The first boss you kill past Eikthyr will drop a stack of Ymir Flesh
- Culinary Saga
  - Fishing without Haldor!
  - Powerful boss minions in each biome now have a chance to drop a Fishing Rod
  - Various fishing baits drop directly from the enemies whose trophies you would trade to Haldor, except for Necks, which drop basic fishing bait
	 | Bait | Dropping Enemy |
	 | -------------------- | ---------------- |
	 | Fishing Bait		    | Neck |
	 | Hot Fishing Bait	    | Charred Warrior |
	 | Cold Fishing Bait    | Fenring |
	 | Frosty Fishing Bait	|   Drake |        
	 | Mossy Fishing Bait	|   Troll |        
	 | Misty Fishing Bait	|   Lox |        
	 | Heavy Fishing Bait	|   Serpent |
	 | Stingy Fishing Bait	|   Fuling |      
	 | Sticky Fishing Bait	|   Abomination |

v0.8.4
- Casual Saga HOTFIX:
  - Fixed a bug that would glitch your inventory when you picked up a trophy, causing all sorts of madness and mayhem. D'oh!

v0.8.2
- Fixed a bug with the timer where it would reset on death/respawn to what it was at the last save

v0.8.1
- Initial release of Saga Mode standalone mod