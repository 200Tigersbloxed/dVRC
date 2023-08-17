#if VRC_SDK_VRCSDK3
using System;
using System.IO;
using BestHTTP.JSON;
using UnityEngine;
using VRC;
using VRC.Core;

namespace dVRC.Editor
{
    public class VRCAsset
    {
        public string Name { get; }
        public string Id { get; }
        public Texture2D Texture { get; }
        public string AssetURL { get; private set; }
        public int Version { get; }
        
        public string FileLocation { get; private set; }
        public byte[] FileBytes { get; private set; }

        private VRCAssetType _vrcAssetType;
        private ApiWorld _world;
        private ApiAvatar _avatar;

        private string _fileEnding
        {
            get
            {
                switch (_vrcAssetType)
                {
                    case VRCAssetType.World:
                        return "w";
                    case VRCAssetType.Avatar:
                        return "a";
                }
                return String.Empty;
            }
        }

        public VRCAsset(ApiWorld world)
        {
            _vrcAssetType = VRCAssetType.World;
            _world = world;
            Name = world.name;
            Id = world.id;
            Texture = ReflectingTools.GetApiModelTextureFromCache(world.id);
            Version = world.version;
        }
        
        public VRCAsset(ApiAvatar avatar)
        {
            _vrcAssetType = VRCAssetType.Avatar;
            _avatar = avatar;
            Name = avatar.name;
            Id = avatar.id;
            Texture = ReflectingTools.GetApiModelTextureFromCache(avatar.id);
            AssetURL = avatar.assetUrl;
            Version = avatar.version;
        }

        public void DownloadAsset(string path, Action<float> percentage = null, Action onDone = null)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string outputFile = Path.Combine(path, Id + ".vrc" + _fileEnding);
            if(string.IsNullOrEmpty(AssetURL) && _world != null)
                _world.Fetch(Tools.Platform, container =>
                {
                    Json.JObject j = (Json.JObject) container.Data;
                    Json.JArray up = j["unityPackages"].Array;
                    bool didOne = false;
                    foreach (Json.Token token in up)
                    {
                        Json.JObject l = token.Object;
                        if (l["platform"].StringInstance == Tools.Platform)
                        {
                            AssetURL = l["assetUrl"].StringInstance;
                            didOne = true;
                        }
                    }
                    if (!didOne && up.Count > 0)
                    {
                        Json.JObject l = up[0].Object;
                        AssetURL = l["assetUrl"].StringInstance;
                    }
                    if(!string.IsNullOrEmpty(AssetURL))
                        download2(outputFile, percentage, onDone);
                }, container =>
                {
                    Debug.LogError("uh oh");
                });
            else if(!string.IsNullOrEmpty(AssetURL))
                download2(outputFile, percentage, onDone);
        }

        private void download2(string outputFile, Action<float> percentage = null, Action onDone = null)
        {
            ApiFile.DownloadFile(AssetURL, bytes =>
            {
                using (FileStream fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush();
                }
                FileLocation = outputFile;
                FileBytes = bytes;
                if(onDone != null)
                    onDone.Invoke();
            }, e =>
            {
                Debug.LogError(e);
                if(onDone != null)
                    onDone.Invoke();
            }, (l, l1) =>
            {
                float percent =  l / l1 * 100f;
                if(percentage != null)
                    percentage.Invoke(percent);
            });
        }

        public override string ToString()
        {
            string g = Name + "\n" +
                       Id + "\n";
            return g;
        }
    }
}
#endif