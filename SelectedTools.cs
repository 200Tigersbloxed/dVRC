using System;
using System.Linq;

namespace dVRC
{
    public static class SelectedTools
    {
        public static VRCAssetType SelectedAssetType { get; set; } = VRCAssetType.Unknown;
        public static string SelectedAvatarId { get; set; } = String.Empty;
        public static string SelectedWorldId { get; set; } = String.Empty;
        
        public static string SelectedFile { get; set; } = String.Empty;

        public static VRCAssetType GetAssetTypeFromId(string id)
        {
            string idtype = id.Split('_')[0];
            switch (idtype.ToLower())
            {
                case "avtr":
                    return VRCAssetType.Avatar;
                case "wrld":
                    return VRCAssetType.World;
            }
            return VRCAssetType.Unknown;
        }

        public static VRCAssetType GetAssetTypeFromFileType(string filename)
        {
            string filetype = filename.Split('.').Last();
            switch (filetype)
            {
                case "vrca":
                    return VRCAssetType.Avatar;
                case "vrcw":
                    return VRCAssetType.World;
            }
            return VRCAssetType.Unknown;
        }
    }

    public enum VRCAssetType
    {
        Unknown,
        Avatar,
        World
    }
}