# Marching cubes

Marching cubes algorithm implemented in Monogame.

## Compile

* Restore nuget packages for solution
* then either run "RELEASE VERSION.cmd" (requires msbuild in path) to get all the necessary files in "!Releases" output dir
* or compile solution and run in Visual Studio 

First compile might fail as it sets up ntfs junction so submodule gets access to the package folder. Second compile should always work.

**Note that while monogame was used it is embedded directly into the exe using Fody.Costura**

You should not have to install monogame, unless you are missing some common DirectX redistributables.
 
## Sample result
 
![MRI result](/mri.png?raw=true)

## Features

* Has two existing modules, an mri scan and a sphere
* Can render then entire finished model in its final state
* Can also render the construction of data in realtime
    * "F5 mode" selects one cube at a time, figures out its match and adds the triangles.
    * In this mode user can pause (space) and single step through the creation with "E"
* VoxelGen tool allows generating data set (for now only sphere is provided as sample implementation)

## Known issues

* When reloading the scene it sometimes gets stuck and is rendered twice (one that follows the user movement and one that doesn't).
* Overall marching cubes result is less than stellar as the algorithm leaves plenty of holes (probably one misconfigured or missing cube config)
