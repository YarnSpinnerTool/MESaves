# Mass Effect 2 Save Importer for Yarn Spinner

Mass Effect 2 Save Importer for Yarn Spinner is an example Unity project that demonstrates reading save-game files from *Mass Effect 2: Legendary Edition*. Using the importer, games can import save games, and make use of the player's current state.

Mass Effect 2 Save Importer for Yarn Spinner requires Unity 6 and [Yarn Spinner 3](https://docs.yarnspinner.dev/coming-in-v3#installing-the-beta-in-unity) (currently in Beta).

## Demonstration

- Open the project in Unity 6, and open the scene `SaveLoader.unity`.
- Using the WASD keys, walk up to the character and press Space to talk to them.
- The character will talk about things the player has done.

You can use your own save file from *Mass Effect 2: Legendary Edition* by following these steps:

- Locate your save file. It's generally located in your `Documents` folder, in `BioWare/Mass Effect Legendary Edition/Save/ME2`, and has the file extension `.pcsav`.
- Copy this file into the Unity project. It will be imported as an `ME Save Data` asset.
- In Unity, select the Save File Loader object, and drag and drop your save into the the Save Data field.
- Run the game again. Depending on the contents of the save file, you'll see different responses from the character when you talk to them.

## Using Data in the Save File

After a save file has been loaded, Yarn Spinner scripts can access information contained within the save file. The `GeneratedVariables.yarn` file contains the list of all boolean and integer values stored in the Mass Effect 2 save game, as well as the Mass Effect 1 plot record (from any imported save game) embedded within the file.

To use a plot variable in your Yarn script, simply use it like you would any other Yarn variable.

```
=> NPC: Wow, Shepard! I can't believe you let the Council die! <<if $LE2_ME1_Plots_for_ME2_CH4_Star_Citadel_Council_Dead>>
=> NPC: Nice one on saving the council, Shepard! <<if $LE2_ME1_Plots_for_ME2_CH4_Star_Citadel_Council_Alive>>
```

You can also retrieve the first name of the player character like this:

```
<<declare $player_name = get_me_name()  as string>>

NPC: Greetings, Commander {$player_name} Shepard!
```

You can use the player's Paragon and Renegade scores to control whether conversation options are available. In the sample, the `#paragon` and `#renegade` hashtags will make the options display in a blue or red colour, respectively.

```
-> Paragon Choice <<if $LE2_Utility_Player_Info_Paragon > 600>> #paragon
-> Renegade Choice <<if $LE2_Utility_Player_Info_Renegade > 600>> #renegade
-> Neutral Choice
```

Some useful examples of variables:

- The player's Paragon and Renegade score are available via the following variables:
  - `$LE2_Utility_Player_Info_Paragon` stores the Paragon score.
  - `$LE2_Utility_Player_Info_Renegade` stores the Renegade score.
- To determine whether a character is a member of the squad or not, access the following variables:
  - `$LE2_Utility_Henchmen_In_Squad_Vixen`
  - `$LE2_Utility_Henchmen_In_Squad_Leading`
  - `$LE2_Utility_Henchmen_In_Squad_Convict`
  - `$LE2_Utility_Henchmen_In_Squad_Geth`
  - `$LE2_Utility_Henchmen_In_Squad_Thief`
  - `$LE2_Utility_Henchmen_In_Squad_Garrus`
  - `$LE2_Utility_Henchmen_In_Squad_Assassin`
  - `$LE2_Utility_Henchmen_In_Squad_Tali`
  - `$LE2_Utility_Henchmen_In_Squad_Professor`
  - `$LE2_Utility_Henchmen_In_Squad_Grunt`
  - `$LE2_Utility_Henchmen_In_Squad_Mystic`
  - `$LE2_Utility_Henchmen_In_Squad_Veteran`
  - There are similar variables for controlling whether a character's loyalty mission has been completed, or whether they have been killed.
- Specific outcomes for storylines can be read. For example, the outcome of Tali's treason trial can be fetched via the following variables:
  - `$LE2_Loyalty_Missions_Tali_Loyalty_Trial_Resolution_Gave_Evidence`
  - `$LE2_Loyalty_Missions_Tali_Loyalty_Trial_Resolution_Pled_Guilty`
  - `$LE2_Loyalty_Missions_Tali_Loyalty_Trial_Resolution_Innocent_Charmed`
  - `$LE2_Loyalty_Missions_Tali_Loyalty_Trial_Resolution_Innocent_Intimidate`

The names of the variables are taken from [Electronic Arts' release of the Mass Effect plot databases](https://github.com/electronicarts/MELE_ModdingSupport/). 

## Performance

The `GeneratedVariables.yarn` file is very large, and can cause long compilation times for your Yarn scripts. You can speed up compilation by removing the variables you don't need from the file.

## Legal Notices

Mass Effect is a trademark of Electronic Arts Inc.

Yarn Spinner is a trademark of Secret Lab Pty. Ltd.
