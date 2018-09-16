# Marching cubes

Marching cubes algorithm implemented in Monogame.

## Compile

* Restore nuget packages for solution and compile in Visual Studio 

First compile might fail as it sets up ntfs junction so submodule gets access to the package folder. Second compile should always work.

You should not have to install monogame, unless you are missing some common DirectX redistributables.
 
## Sample results
 
![MRI result](/mri.png?raw=true)

![Geometry cutting](/geo.png?raw=true)

## Features

* Multiple datasets (mri scan and sphere)
* Can render then entire finished model in its final state
* Can also render the construction of data in realtime
    * "F5 mode" selects one cube at a time, figures out its match and adds the triangles.
    * In this mode user can pause (space) and single step through the creation with "E" (use Mousewheel to speed up/slow down)
* VoxelGen tool allows generating data set (for now only sphere is provided as sample implementation)

## Known issues

MRI data has some holes in the surface (you can finetune the isolevel in MarchingCubesScene). Higher isolevel (e.g. 128 [default]) means more detail while lower isolevel (e.g. 80) means smoother surfaces.

I assume the holes are due to imprecision of the dataset (downloaded from [Paul Bourke](http://paulbourke.net/geometry/polygonise/). Other datasets built from mathematical geometry do not show these issues.
