# BaseUtilities
C# Base utilities set

Used by EDDiscovery and other programs as a base set of c# utilities

This module is the base module - it includes no other modules.

It includes the following projects:

* BaseUtilities - requiring package Newtonsoft.json
* Audio (for audio control) - requiring package CSCore, project BaseUtilities (relative ref)
* DirectInput (for joystick/keyboard control) - requiring package SharpDX, project BaseUtilities (relative ref)

Check this out and you can use the test harness via the visual studio test explorer.  The harness is not extensive.  It requires NUnit to function.

The vsproj has had its Packages/HintPath manually changed to use $(SolutionDir) as the base folder.

# GIT

Useful git commands for managing this https://chrisjean.com/git-submodules-adding-using-removing-and-updating/

The git repo which includes this submodule is called parent, and with this BaseUtilities included.

cd c:\code\parent\BaseUtilities

git status

HEAD detached at 01f236a

nothing to commit, working tree clean

meaning its not attached to the branch, and the code is at 01f236a.

git fetch       - update repo

git checkout master - checkout the local version of master

git pull - update to the remote version of master.

cd c:\code\parent

you need to commit the parent now, this updates the commit ID of the submodule to the updated commit point.
