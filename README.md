# dVRC
A Unity tool to Recover your lost VRChat Assets using [AssetRipper](https://github.com/AssetRipper/AssetRipper)

> # ⚠️ THERE IS TO BE NO MALICIOUS USE OF THIS TOOL! ⚠️
>
> This tool is designed to **recover your lost assets**.
>
> *Please don't ask, or redesign this tool, to rip assets that aren't your own!*

## Setup and How to Use

1) [Download the Latest Release](https://github.com/200Tigersbloxed/dVRC/releases/latest/download/dVRC.unitypackage)
2) Import into a project with the VRCSDK
    + Support for the VRChat Creator Companion is limited
3) Open the VRChat SDK Window
    + Login if Required
4) Navigate to the Content Manager tab and make sure all your assets load
5) Open dVRC > Main Window
6) Download AssetRipper
7) Select either an Avatar or World to download
8) Download the Asset
9) Return to the Home Page of the Main Window
10) Select the Asset under the Manage Files tab
11) Depending on what you want to do, follow the next steps
    + Preview Asset
      1) Load into Scene
    + Remove the Downloaded Asset
      1) Delete Asset from Disk
    + Extract (rip) the Files to a Unity Project
      1) Extract Assets to Folder
             + Make sure the directory is NOT inside of a current Unity Project
             + Make sure the Folder you are extracting to is empty
             + AssetRipper will DELETE ALL FILES in the selected Directory
      2) Open the Extracted Assets in Unity
      3) Delete all Scripts and Shader files (if the shaders are broken)
      4) Reimport all old Scripts and Shaders and fix any broken Assets

## Will I be banned for using this tool?

Detection of using this tool would be difficult (so long as the SDK is up-to-date), as it uses the [VRChat SDK's Built-In Asset Downloader](https://github.com/200Tigersbloxed/dVRC/blob/main/Editor/VRCAsset.cs#L99), but *known* use of this tool may lead to punishment. The tool is designed to be a *recovery tool*, only recovering **assets you've uploaded**; there is no way to rip other user's assets using this tool. **This tool in no way *modifies* the VRChat SDK.**
