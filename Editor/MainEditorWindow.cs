using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if VRC_SDK_VRCSDK3
using System.Diagnostics;
using VRC.Core;
#endif

namespace dVRC.Editor
{
    public class MainEditorWindow : EditorWindow
    {
        private static MainEditorWindow Instance { get; set; }
            
        [MenuItem("dVRC/Main Window")]
        private static void ShowWindow()
        {
            Instance = GetWindow<MainEditorWindow>();
            Instance.titleContent = new GUIContent("dVRC");
        }

        private const string OutputAssetBundles = "Assets/dVRC/Output/Bundles";
        private readonly RipperHandler _ripperHandler = new RipperHandler();

        private string GetSelectedButtonText(string id, VRCAssetType assetType)
        {
            string text;
            switch (assetType)
            {
                case VRCAssetType.Avatar:
                    if (SelectedTools.SelectedAvatarId == id)
                        text = "Selected";
                    else
                        text = "Select this Avatar";
                    break;
                case VRCAssetType.World:
                    if (SelectedTools.SelectedWorldId == id)
                        text = "Selected";
                    else
                        text = "Select this World";
                    break;
                default:
                    text = "Select this Asset";
                    break;
            }
            return text;
        }

#if VRC_SDK_VRCSDK3
        private static Vector2 ShowList_ScrollView;

        private void ShowList()
        {
            ShowList_ScrollView = EditorGUILayout.BeginScrollView(ShowList_ScrollView);
            foreach (KeyValuePair<string,Texture2D> asset in VRCSdkControlPanel.ImageCache)
            {
                if (SelectedTools.GetAssetTypeFromId(asset.Key) == SelectedTools.SelectedAssetType)
                {
                    EditorGUILayout.BeginHorizontal();
                    VRCAsset vrcAsset = ReflectingTools.GetDynamicAsset(asset.Key, SelectedTools.SelectedAssetType);
                    GUILayout.Box(vrcAsset.Texture, new GUIStyle
                    {
                        fixedHeight = 64,
                        fixedWidth = 64
                    });
                    GUILayout.Label(vrcAsset.Name);
                    if (GUILayout.Button(GetSelectedButtonText(vrcAsset.Id, SelectedTools.SelectedAssetType)))
                        SelectedTools.SelectedAvatarId = vrcAsset.Id;
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Return", EditorStyles.miniButtonRight))
            {
                ShowList_ScrollView = Vector2.zero;
                SelectedTools.SelectedAssetType = VRCAssetType.Unknown;
            }
        }

        private void ShowAssetScreen(string id)
        {
            if (!VRCSdkControlPanel.ImageCache.ContainsKey(id))
            {
                SelectedTools.SelectedAvatarId = String.Empty;
                SelectedTools.SelectedWorldId = String.Empty;
                return;
            }
            VRCAsset vrcAsset = ReflectingTools.GetDynamicAsset(id, SelectedTools.SelectedAssetType);
            GUILayout.Label("Selected Asset", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Box(vrcAsset.Texture, EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label(vrcAsset.ToString(), EditorStyles.centeredGreyMiniLabel);
            if (GUILayout.Button("Download Asset"))
            {
                vrcAsset.DownloadAsset(OutputAssetBundles, null, () =>
                {
                    SelectedTools.SelectedAvatarId = String.Empty;
                    SelectedTools.SelectedWorldId = String.Empty;
                    SelectedTools.SelectedAssetType = VRCAssetType.Unknown;
                    EditorUtility.DisplayDialog("dVRC", "Finished Downloading " + vrcAsset.Name, "OK");
                });
            }
            if (GUILayout.Button("Return", EditorStyles.miniButtonRight))
            {
                SelectedTools.SelectedAvatarId = String.Empty;
                SelectedTools.SelectedWorldId = String.Empty;
            }
        }

        private string PrettyPrintFile(string file) => Path.GetFileName(file);

        private bool wasWorking;
        private string wasPath;

        private void DrawManageFileMenu()
        {
            GUILayout.Label(PrettyPrintFile(SelectedTools.SelectedFile), EditorStyles.centeredGreyMiniLabel);
            if(SelectedTools.GetAssetTypeFromFileType(SelectedTools.SelectedFile) == VRCAssetType.World && !EditorApplication.isPlaying)
                GUILayout.Label("Enter Play mode to load a World");
            else if (SelectedTools.GetAssetTypeFromFileType(SelectedTools.SelectedFile) == VRCAssetType.Avatar ||
                     (SelectedTools.GetAssetTypeFromFileType(SelectedTools.SelectedFile) == VRCAssetType.World &&
                      EditorApplication.isPlaying))
            {
                if (GUILayout.Button("Load into Scene"))
                    AssetLoader.LoadAssetBundle(SelectedTools.SelectedFile);
            }
            if (GUILayout.Button("Delete Asset from Disk"))
            {
                AssetLoader.DeleteAsset(SelectedTools.SelectedFile);
                SelectedTools.SelectedFile = String.Empty;
            }
            if (_ripperHandler.isPresent && !_ripperHandler.IsWorking)
            {
                if (GUILayout.Button("Extract Assets to Folder") && !_ripperHandler.IsWorking)
                {
                    string path = EditorUtility.OpenFolderPanel("Select a Folder", "Assets", "");
                    if(string.IsNullOrEmpty(path))
                        return;
                    if (IOTools.IsChildDirectory(Application.dataPath, path))
                        if(!EditorUtility.DisplayDialog("dVRC",
                               "You have selected a directory in which the ExportedAssets will be inserted into the current project. This may cause issues, are you sure you would like to continue?",
                               "Yes", "No"))
                            return;
                    wasWorking = false;
                    wasPath = path;
                    _ripperHandler.OnData += s =>
                    {
                        wasWorking = true;
                    };
                    _ripperHandler.Rip(Path.GetFullPath(SelectedTools.SelectedFile), path);
                }
            }
            else
                if(_ripperHandler.IsWorking)
                    GUILayout.Label("Currently Ripping...", EditorStyles.centeredGreyMiniLabel);
            if(!_ripperHandler.IsWorking)
                if(GUILayout.Button("Return", EditorStyles.miniButtonRight))
                    SelectedTools.SelectedFile = String.Empty;
            GUILayout.Label("Note: Loading Worlds is nearly pointless because of Unity.", EditorStyles.miniLabel);
        }

        private static Vector2 ManageAssets_ScrollView;
        
        private void DrawManageAssetBundles()
        {
            GUILayout.Label("or", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label("Manage Files");
            ManageAssets_ScrollView = EditorGUILayout.BeginScrollView(ManageAssets_ScrollView);
            if (Directory.Exists(OutputAssetBundles))
            {
                foreach (string file in Directory.GetFiles(OutputAssetBundles))
                {
                    if (!file.Split('.').Last().Contains("meta"))
                    {
                        if (GUILayout.Button(PrettyPrintFile(file)))
                            SelectedTools.SelectedFile = file;
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }
#endif
        private bool isDownloading;
        
        private void DrawRequestDownloadAssetRipper()
        {
            GUILayout.Label("AssetRipper is not loaded!");
            GUILayout.Label(
                "AssetRipper is used to rip Assets from an AssetBundle and load them into Unity from disk, rather than memory",
                EditorStyles.miniLabel);
            if (!isDownloading)
            {
                if (GUILayout.Button("Download AssetRipper"))
                {
                    if (!isDownloading)
                    {
                        isDownloading = true;
                        _ripperHandler.Download(() =>
                        {
                            isDownloading = false;
                            EditorUtility.DisplayDialog("dVRC", "Downloaded AssetRipper!", "OK");
                        });
                    }
                }
            }
            else
                GUILayout.Label("Downloading AssetRipper...", EditorStyles.miniBoldLabel);
            GUILayout.Label(_ripperHandler.WorkingDirectory, EditorStyles.miniLabel);
        }

        public void OnGUI()
        {
            _ripperHandler.SetWorkingDirectory();
#if VRC_SDK_VRCSDK3
            if (APIUser.IsLoggedIn)
            {
                if(!string.IsNullOrEmpty(SelectedTools.SelectedFile)){}
                else if (!string.IsNullOrEmpty(SelectedTools.SelectedAvatarId))
                    ShowAssetScreen(SelectedTools.SelectedAvatarId);
                else if (!string.IsNullOrEmpty(SelectedTools.SelectedWorldId))
                    ShowAssetScreen(SelectedTools.SelectedWorldId);
                else
                {
                    if (!ReflectingTools.DidFetchContent())
                        GUILayout.Label("Content has not been fetched or no content is available to download!");
                    else
                        switch (SelectedTools.SelectedAssetType)
                        {
                            case VRCAssetType.Unknown:
                                GUILayout.Label("Please select an Asset to Download");
                                GUILayout.BeginHorizontal();
                                if (GUILayout.Button("Avatar"))
                                    SelectedTools.SelectedAssetType = VRCAssetType.Avatar;
                                if (GUILayout.Button("World"))
                                    SelectedTools.SelectedAssetType = VRCAssetType.World;
                                GUILayout.EndHorizontal();
                                break;
                            default:
                                ShowList();
                                break;
                        }
                }
            }
            else
                GUILayout.Label("Please use the VRC SDK Control Panel to Login!");

            if (string.IsNullOrEmpty(SelectedTools.SelectedAvatarId) &&
                string.IsNullOrEmpty(SelectedTools.SelectedWorldId) && 
                SelectedTools.SelectedAssetType == VRCAssetType.Unknown)
            {
                if(!string.IsNullOrEmpty(SelectedTools.SelectedFile))
                    DrawManageFileMenu();
                else
                {
                    DrawManageAssetBundles();
                    if(!_ripperHandler.isPresent)
                        DrawRequestDownloadAssetRipper();
                }
            }
#else
            GUILayout.Label("Could not find the VRC_SDK_VRCSDK3 Scripting Definition! Is the VRCSDK present?");
#endif
        }

        public void Update()
        {
            if (!_ripperHandler.IsWorking && wasWorking)
            {
                wasWorking = false;
                string output = Path.Combine(wasPath, "ExportedProject");
                if (Directory.Exists(output))
                {
                    if (EditorUtility.DisplayDialog("dVRC", "Completed Operation!", "OK"))
                        Process.Start(output);
                }
                else
                {
                    Debug.LogWarning("Directory " + output + " does not exist post-export!");
                    EditorUtility.DisplayDialog("dVRC",
                        "Failed to complete operation! Check the Console for details.", "OK");
                }
            }
        }
    }
}