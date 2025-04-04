# Mass Effect 2 Save Importer for Yarn Spinner



Mass Effect 2 Save Importer for Yarn Spinner is an example Unity project that demonstrates reading save-game files from *Mass Effect 2: Legendary Edition*. Using the importer, games can import save games, and make use of the player's current state.

Mass Effect 2 Save Importer for Yarn Spinner requires Unity 6 and [Yarn Spinner 3](https://docs.yarnspinner.dev/coming-in-v3#installing-the-beta-in-unity) (currently in Beta).

[![Screenshot 2025-04-01 at 12 25 18 pm png](https://github.com/user-attachments/assets/bb1be135-ea5f-4923-bf1b-02337b9edb0d)](https://youtu.be/9jIZqszuGI8)


## Demo Project

- Open the project in Unity 6, and open the scene `SaveLoader.unity`.
- Using the WASD keys, walk up to the character and press Space to talk to them.
- The character will talk about things the player has done.

You can use your own save file from *Mass Effect 2: Legendary Edition* by following these steps:

- Locate your save file. It's generally located in your `Documents` folder, in `BioWare/Mass Effect Legendary Edition/Save/ME2`, and has the file extension `.pcsav`.
- Copy this file into the Unity project. It will be imported as an `ME Save Data` asset.
- In Unity, select the Save File Loader object, and drag and drop your save into the the Save Data field.
- Run the game again. Depending on the contents of the save file, you'll see different responses from the character when you talk to them.

## Accessing Save Data

After a save file has been loaded, Yarn Spinner scripts can access information contained within the save file. The `GeneratedVariables.yarn` file contains the list of all boolean and integer values stored in the Mass Effect 2 save game, as well as the Mass Effect 1 plot record (from any imported save game) embedded within the file.

To use a plot variable in your Yarn script, simply use it like you would any other Yarn variable.

```
=> NPC: Wow, Shepard! I can't believe you chose Morinth over Samara! <<if $MassEffect2_Companions_Samara_LoyaltyMission_ChoseMorinth>>
=> NPC: Nice one siding with Samara, Shephard!<<if $MassEffect2_Companions_Samara_LoyaltyMission_ChoseSamara>>
```

You can also retrieve the first name of the player character like this:

```
<<declare $player_name = get_me_name()  as string>>

NPC: Greetings, Commander {$player_name} Shepard!
```

You can use the player's Paragon and Renegade scores to control whether conversation options are available. In the sample, the `#paragon` and `#renegade` hashtags will make the options display in a blue or red colour, respectively.

```
-> Paragon Choice <<if $MassEffect2_PlayerInfo_ParagonPoints > 600>> #paragon
-> Renegade Choice <<if $MassEffect2_PlayerInfo_RenegadePoints > 600>> #renegade
-> Neutral Choice
```

Some useful examples of variables:

- The player's Paragon and Renegade score are available via the following variables:
  - `$MassEffect2_PlayerInfo_ParagonPoints` stores the Paragon score.
  - `$MassEffect2_PlayerInfo_RenegadePoints` stores the Renegade score.
- To determine whether a character is a member of the squad or not, access the following variables:
  - `$MassEffect2_Companions_Garrus_InSquad`
  - `$MassEffect2_Companions_Grunt_InSquad`
  - `$MassEffect2_Companions_Jack_InSquad`
  - `$MassEffect2_Companions_Jacob_InSquad`
  - `$MassEffect2_Companions_Kasumi_InSquad`
  - `$MassEffect2_Companions_Legion_InSquad`
  - `$MassEffect2_Companions_Miranda_InSquad`
  - `$MassEffect2_Companions_Mordin_InSquad`
  - `$MassEffect2_Companions_Samara_InSquad`
  - `$MassEffect2_Companions_Tali_InSquad`
  - `$MassEffect2_Companions_Thane_InSquad`
  - `$MassEffect2_Companions_Wilson_InSquad`
  - `$MassEffect2_Companions_Zaeed_InSquad`
  - There are similar variables for controlling whether a character's loyalty mission has been completed (`$MassEffect2_Companions_<name>_IsLoyal`), or whether they have been killed (`$MassEffect2_Companions_<name>_IsDead`).
- Specific outcomes for storylines can be read. For example, the outcome of Tali's treason trial can be fetched via the following boolean variables (exactly one of which will be true if the `$MassEffect2_Companions_Tali_LoyaltyMission_Trial_Resolution` variable is true, declaring that this arc was completed):
  - `$MassEffect2_Companions_Tali_LoyaltyMission_Trial_Resolution_GaveEvidence`
  - `$MassEffect2_Companions_Tali_LoyaltyMission_Trial_Resolution_InnocentCharmed`
  - `$MassEffect2_Companions_Tali_LoyaltyMission_Trial_Resolution_InnocentIntimidate`
  - `$MassEffect2_Companions_Tali_LoyaltyMission_Trial_Resolution_PledGuilty`

The names of the variables are adapted from [Electronic Arts' release of the Mass Effect plot databases](https://github.com/electronicarts/MELE_ModdingSupport/). 

## A Note on Performance

The `GeneratedVariables.yarn` file is very large, and can cause long compilation times for your Yarn scripts. You can speed up compilation by removing the variables you don't need from the file.

## Legal Notices

Mass Effect is a trademark of Electronic Arts Inc.

Yarn Spinner is a trademark of Secret Lab Pty. Ltd.
