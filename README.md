# DS2S META
 Dark Souls 2 Scholar of the First Sin testing/debugging/practice tool  
 Based on a CE table by [Lord Radai](https://github.com/LordRadai)  
 
 Inspired by [DS-Gadget 3.0](https://github.com/JKAnderson/DS-Gadget) and uses [Property Hook](https://github.com/JKAnderson/PropertyHook) by [TKGP](https://github.com/JKAnderson/).  
 
 META stands for:  
M  
E  
T  
A  
 
# WARNING  
 There has been absolutely NO testing on this tool and online use. For offline use only. Use online at your own risk.  
 Even actions not locked behind a setting are considered unsafe for online use.  
 YOU HAVE BEEN WARNED  

## Requirements 
* [.NET 6] (https://dotnet.microsoft.com/en-us/download/dotnet/6.0)  

## Installing  
* Extract contents of zip archive to it's own folder. You may have to run as admin if DS2S META crashes  

## Thank You  
**Nordgaren/Pseudostripy** Main developers of META\
**[Nordgaren](https://github.com/Nordgaren)** especially for helping me out on some horrible bugs!\
**[TKGP](https://github.com/JKAnderson/)** Author of [DS Gadget](https://github.com/JKAnderson/DS-Gadget) and [Property Hook](https://github.com/JKAnderson/PropertyHook)\
**[Lord Radai](https://github.com/LordRadai)** Author of the CE table used for this tool and good mentor\
**[R3sus](https://github.com/r3sus)** Contributor and absolute wealth of knowledge  

## Libraries
Nord's fork of [Property Hook](https://github.com/Nordgaren/PropertyHook) by [TKGP](https://github.com/JKAnderson/)  

[Costura](https://github.com/Fody/Costura) by [Fody](https://github.com/Fody) team  

[Octokit](https://github.com/octokit/octokit.net) by [Octokit](https://github.com/octokit) team

[GlobalHotkeys](https://github.com/mrousavy/Hotkeys) by [Marc Rousavy](https://github.com/mrousavy)  

[SpeedhackWithExports](https://github.com/Nordgaren/SpeedhackWithExports) - My fork of [Speedhack](https://github.com/absoIute/Speedhack) by [absoIute](https://github.com/absoIute)   

# Change Log 

### META v0.7 [Combiner]:
#### v0.7.0.0
* Merge DS2 META Vanilla into single program DS2S combined!
* Variety Stability upgrades (crash fixes) on meta/ds2 open/close
* Finish the full merge of leftover params from Hook into ParamMan
* Rewrite the param item getter methods properly to use game defs
* Variety front-end updates to simplify META screen where possible
* Rewrite of all the assembly templates for equiv vanilla versions
* Fix to make rest-after-warp actually implemented correctly (finally)
* Add feature for "Remove Souls"
* Rando: possible softlock prevention on fang key dude

#### Fix full feature support (Sotfs v1.02 / v1.03, Vanilla v1.02 / v1.11 / v1.12) for:
* Version identifier showing Game/BBJ mod versions
* Unlock all bonfires
* Item Give (normal & silent)
* No Gravity
* No Collision
* ApplySpecialEffect (e.g. Restore humanity)
* Warp to bonfire or map default
* New Test Character
* Give / Remove Souls
* Stable position / Teleport

### META v0.6 [.net6]
#### v0.6.1.1
* Hotfixes to hotkey implementation and lag

#### v0.6.1.0

* Overhaul to hotkey implementation for full-screen DS2

#### v0.6.0.8
* Rando minor softlock and bug fixes

#### v0.6.0.7
* New Test Character feature added
* Rando: Enemy drops randomized (and bug fixes)
* Rando refactoring for future proofing and generlaization
* Rando bug fixes: Rerandomization bugs / shared loot fixes
* Rando balances: key decisions / remove over-abundances torches etc.

#### v0.6.0.6
* META compatibility with all bbj versions and newest patches
* First label version showing active mods via META

#### v0.6.0.5
* Crash hotfixes for new DS2 patch for SOTFS
* Extra user information and logging during auto-updater
* Stability improvements: Variety of other META crash fixes

#### v0.6.0.4
* MVVM implementation and progress on DmgCalc tab
* Rapier OHKO feature
* Auto-updater ongoing fixes & robustness testing
* Rando: Character creation now randomized
* Rando bug fixes: Trades shops / NPC shops after move / bad pickups / sotflocks / Gesture crashes

#### v0.6.0.3 & v0.6.0.2
* META Auto-updater / config loader
* Rando bug fixes: busted item spots / shop prices

#### v0.6.0.1:
* Implemented a Param Manager for more generalized param loading
* Start of (incomplete/placeholder) dmg calc code based on Radai's formula
* Randomizer performance optimizations

#### v0.6.0.0
* Upgrade Meta to C# .NET 6.0 and fix associated transitional pains

### META v0.5 [Rando]
#### Randomizer Alpha A.1

* Added descriptions of all items and all keys in txt files
* Vanilla item-leak bug fixed on all lots/shops
* Empty lots fixed
* Shops fixed when NPCs move
* Fixed Reinforcements and Infusions being applied to consumables
* Fixed Trade logic (a little)
* Fixed over-maximum reinforcements - now checked versus in game max param
* Item prices randomized even for two items of same ID within a shop
* Item prices adjusted depending on type
* Number of items sold in shops adjusted/fixed for balancing
* Fixed many locations worth of fixed descriptions/mis-typed backend info
* Fixed item duplications that shouldn't be there
* Added QOL messages for what happens when you click randomize
* Updated Frontend for seed generation and general convenience
* Fixed possibly missing estus bug
* Fixed Soul of Nadalia logic
* Fix Alsanna & Darklurker/Dungeon Logic
* Fixed Soul of a Giant bug (previously only 3 placed)

### Randomizer Pre-Alpha A.0
* Added Randomizer to META (sotfs only)
* First version of randomizer with non game-breaking bugs

### META v0.4 [Stable] (first stable release and pseudo takes over main development from Nord)
### Beta 0.4  

* Fixed game crash when closing META  

* Fixed crash related to bad Hunters Hat ID  

* Added fast quit hotkey  

### Beta 0.3  

* Modify Speed on the Player tab is now equivilent to Cheat Engine Speedhack. If you want to just modify your own speed, use the speeds in Internals tab.  

### Beta 0.2  

* New Hotkey system using GlobalHotkeys library. Should fix issue with input delayin game

* Optimized GetHeld method, which should make looking up items in player inventory faster. Could use feedback from anyone who previously couldn't use the live inventory update feature.  

* Updated Resources/Equipment/DS2SItemCategories.txt to be more like the other editable text files.  

* Added check to make sure you have right version of the game loaded.  

* Changed to using Stable position in the player position restore system  

### Beta 0.1  

* Cosmetic changes, and added bonfire control  

* Getting ready for 1.0 

* Checkbox to live update max quantity (Scan inventory constantly. CPU intensive).  

### Beta 0.0.3.2

* Unoofed storing position. (Fixed crash)  

### Beta 0.0.3.1

* Bonfire menu fixed so it displays actual ascetic level  

* Fixed wrong offset for Max Held, causing max item to be broken  

### Beta 0.0.3
**Please update all files in your Resource folder**  

* Bonfire warp. Not enabled during multiplayer or searching for multiplayer.  

* Restrict now works as expected (I think)  

* Some spelling errors fixed  

* Items tab Quantity and Upgrade no longer default to min/max val  

* Item name and category updates  

* New Item category for weapon reskins. Not Online Safe.  

* Search box text now hilights when searchbox selected  

* Item Box no longer crashes cause tool due to empty item  

* Misc vanity changes  

* Max available to spawn in is now more accurate when you use an item - Tied to max and restrict checkbox  

* Items now check usage params. Option to unlock spawning in undroppable items.  

### Beta 0.0.2  

* Fixed Give Souls func  

* Added reset/max level buttons  

* Blank search now stores a position

* Items refresh on reload

### Beta 0.0.1  

* Items max quantity now looks at how many items are in your inventory  

* Individual speed factors  

* Search all now toggles filter  

* Misc cosmetic changes to positioning  

### Beta 0.0.0.8  

* Revamped Bonfire tab code and look

* Added Up, Down, Collision and Speed hotkeys

### Beta 0.0.0.7  

* Fixed bonfire unlock script no longer breaks Fire Keepers Dwelling Bonfire (Have to rest at it to unlock it)

* Added Internals tab to display character equipment info and other

* Bonfires tab works now kinda. Will make it nice looking later.

### Beta 0.0.0.6  

* Baby Jump DLL Compatability  

### Beta 0.0.0.5  

* Item Quantity, Infusion and Max level read from params  

* Unlock All Bonfires  

* Edit hollow level   

* Fixed max HP bug  


### Beta 0.0.0.4   

* Mostly working item infusion/upgrade menu. Everything should be accurate except melee weapons categories. Report if any are wrong.  

* Split goods into consumables, ammo, upgrade materials and useable items.  

### Beta 0.0.0.3   

* Fixed Update Message meme  

### Beta 0.0.0.2  

* Added Online notification and text colors.  

* Implimented Stored Positions start. These positions will end up changing later, most likely, so they will break eventually.  

### Beta 0.0.0.1  
* Initial beta release. Player, Stats and Item tab  
