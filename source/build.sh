#!/bin/bash

dotnet build && cp bin/Debug/netstandard2.1/TownOfUs.dll "/home/kbinswanger/.local/share/Steam/steamapps/common/Among Us (aTown of Us)/BepInEx/plugins/" && cp bin/Debug/netstandard2.1/TownOfUs.dll  ~/Downloads/TOU/BepInEx/plugins/ && cp bin/Debug/netstandard2.1/TownOfUs.dll "/home/kbinswanger/VirtualBox VMs/Among Us/Shared Folders/AmongUs/BepInEx/plugins"
#dotnet build && cp bin/Debug/netstandard2.1/TownOfUs.dll  ~/Downloads/TOU/BepInEx/plugins/ && cp bin/Debug/netstandard2.1/TownOfUs.dll "/home/kbinswanger/VirtualBox VMs/Among Us/Shared Folders/AmongUs/BepInEx/plugins"
