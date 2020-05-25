# Parkitool

A set of tools that help with modding Parkitect. Setting up `.csproj` downloading parkitect assemblies and configuring output path to the mods directory. 

## Configuration

### parkitect.json
```
{
  "name": "<mod_name>",
  "folder": "<mod_folder>",
  "version": "v1.0.0",
  "workshop": "<workshop_id>",
  "author": "<author>",
  "description": null,
  "preview": "<preview_image>",
  "assemblies": [
    "System",
    "System.Core",
    "System.Data",
    "UnityEngine",
    "UnityEngine.AssetBundleModule",
    "UnityEngine.CoreModule",
    "Parkitect",
    "UnityEngine.PhysicsModule"
    ...
    <assemblies>
  ],
  "assets": [
    "assetbundle/**",
    ...
    <assets>
  ]
}
```

## Commands

### `parkitool install -u <steam_username> -p <steam_password>`

Downloads parkitect into a hidden folder and setups up hint paths to point to parkitect assemblies. output path is determine first by folder then by project name.


```
Connecting to Steam3... Done!
Logging '<steam_username>' into Steam3...Disconnected from Steam
Please enter your 2 factor auth code from your authenticator app: tj7ry
Retrying Steam3 connection... Done!
Logging '<steam_username>' into Steam3... Done!
Download Parkitect ...
Got 179 licenses for account!
Got session token!
Got AppInfo for 453090
Using app branch: 'public'.
Got depot key for 453094 result: OK
Downloading depot 453094 - Parkitect Linux
Got CDN auth token for steamcontent.com result: OK (expires 5/31/2020 1:21:58 AM)
 00.02% .Parkitect/Game/Parkitect_Data/Managed/UnityEngine.SpriteShapeModule.dll
 00.24% .Parkitect/Game/Parkitect_Data/Managed/UnityEngine.AnimationModule.dll
 01.19% .Parkitect/Game/Parkitect_Data/Managed/Mono.Security.dll
 01.29% .Parkitect/Game/Parkitect_Data/Managed/UnityEngine.UnityWebRequestModule.dll
 01.36% .Parkitect/Game/Parkitect_Data/Managed/UnityEngine.VRModule.dll
 01.39% .Parkitect/Game/Parkitect_Data/Managed/Accessibility.dll
 01.68% .Parkitect/Game/Parkitect_Data/Managed/System.DirectoryServices.dll
 ...
 Depot 453094 - Downloaded 9112464 bytes (33220608 bytes uncompressed)
Setup Project ...
Resolved Known Standard System Assembly -- System
Resolved Known Standard System Assembly -- System.Core
Resolved Known Standard System Assembly -- System.Data
Resolved to Known System assembly but found in Parkitect Managed -- UnityEngine
Resolved to Known System assembly but found in Parkitect Managed -- UnityEngine.AssetBundleModule
Resolved to Known System assembly but found in Parkitect Managed -- UnityEngine.CoreModule
Resolved to Known System assembly but found in Parkitect Managed -- Parkitect
Resolved to Known System assembly but found in Parkitect Managed -- UnityEngine.PhysicsModule
Output Path: <mod_path>/<mod_folder>
Created Project: ./<project_name>.csproj
Completed
```

### `parkitool install -path <project_path>`

Similar configuration but assemblies are resolved to a given path to parkitect.


### `parkitool init`

setups local configuration for the tool to use through a step  by step process.  