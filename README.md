# schedule-mono-example
Example template and instructions for modding Schedule I Mono version


## Requirements
- The game requires your mod to target the net48 framework, this is defined in the ExampleMod.csproj file TargetFramework property and net48 must be installed before building the library. Visual Studio will prompt you to install it if not present.
- Then install the 2 required packages, Visual Studio toolbar -> Project -> Manage NuGet Packages
  - Search for LavaGang.MelonLoader and install it
  - Search for Lib.Harmony and install it

- Now you must additionally have MelonLoader installed for the game, if it is not installed do it now.
- Opt in to the Alternate or Alternate Beta branch for Schedule I in Steam and wait for it to finish installation.
  - Then you must navigate to C:\Program Files (x86)\Steam\steamapps\common\Schedule I\Schedule I_Data\Managed
  - From here you will need two files: Assembly-CSharp.dll and UnityEngine.CoreModule.dll, copy these two files into the libs folder. (Also specified in the .csproj file)

- After these steps are done, you are ready to code your own logics. See the Template MainMod.cs file for the basic requirements for Harmony and MelonLoader.
