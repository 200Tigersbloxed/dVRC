#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VRC.Core;

namespace dVRC.Editor
{
    public class ReflectingTools
    {
        private static ApiWorld GetApiWorldFromCache(string id)
        {
            List<ApiWorld> apiWorlds = (List<ApiWorld>) Convert.ChangeType(
                typeof(VRCSdkControlPanel).GetField("uploadedWorlds", BindingFlags.Static | BindingFlags.NonPublic)?
                    .GetValue(null), typeof(List<ApiWorld>)) ?? new List<ApiWorld>();
            foreach (ApiWorld apiWorld in apiWorlds)
                if (apiWorld.id == id)
                    return apiWorld;
            return null;
        }

        private static ApiAvatar GetApiAvatarFromCache(string id)
        {
            List<ApiAvatar> apiAvatars = (List<ApiAvatar>) Convert.ChangeType(
                typeof(VRCSdkControlPanel).GetField("uploadedAvatars", BindingFlags.Static | BindingFlags.NonPublic)?
                    .GetValue(null), typeof(List<ApiAvatar>)) ?? new List<ApiAvatar>();
            foreach (ApiAvatar apiAvatar in apiAvatars)
                if (apiAvatar.id == id)
                    return apiAvatar;
            return null;
        }

        public static VRCAsset GetDynamicAsset(string id, VRCAssetType assetType)
        {
            switch (assetType)
            {
                case VRCAssetType.Avatar:
                    ApiAvatar avatar = GetApiAvatarFromCache(id);
                    VRCAsset vrcAsseta = new VRCAsset(avatar);
                    return vrcAsseta;
                case VRCAssetType.World:
                    ApiWorld world = GetApiWorldFromCache(id);
                    VRCAsset vrcAssetw = new VRCAsset(world);
                    return vrcAssetw;
            }
            return null;
        }

        public static Texture2D GetApiModelTextureFromCache(string id)
        {
            foreach (KeyValuePair<string,Texture2D> keyValuePair in VRCSdkControlPanel.ImageCache)
            {
                if (keyValuePair.Key == id)
                    return keyValuePair.Value;
            }
            return null;
        }

        public static bool DidFetchContent()
        {
            List<ApiWorld> apiWorlds = (List<ApiWorld>) Convert.ChangeType(
                typeof(VRCSdkControlPanel).GetField("uploadedWorlds", BindingFlags.Static | BindingFlags.NonPublic)?
                    .GetValue(null), typeof(List<ApiWorld>)) ?? new List<ApiWorld>();
            List<ApiAvatar> apiAvatars = (List<ApiAvatar>) Convert.ChangeType(
                typeof(VRCSdkControlPanel).GetField("uploadedAvatars", BindingFlags.Static | BindingFlags.NonPublic)?
                    .GetValue(null), typeof(List<ApiAvatar>)) ?? new List<ApiAvatar>();
            bool isAvatarEmpty = apiAvatars.Count <= 0;
            bool isWorldEmpty = apiWorlds.Count <= 0;
            return !isAvatarEmpty || !isWorldEmpty;
        }
    }
}
#endif