using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace dVRC
{
    public class AssetLoader
    {
        public static void LoadAssetBundle(string file)
        {
            AssetBundle.UnloadAllAssetBundles(false);
            AssetBundle ab = AssetBundle.LoadFromFile(file);
            if (ab.isStreamedSceneAssetBundle)
            {
                List<string> scenePaths = ab.GetAllScenePaths().ToList();
                if (scenePaths.Count > 0)
                {
                    SceneManager.LoadScene(Path.GetFileNameWithoutExtension(scenePaths[0]));
                }
            }
            else
            {
                List<Object> loadedassets = ab.LoadAllAssets().ToList();
                foreach (Object loadedasset in loadedassets)
                {
                    if (loadedasset is GameObject)
                    {
                        Object.Instantiate((GameObject) loadedasset);
                    }
                }
            }
        }

        public static void DeleteAsset(string file) => File.Delete(file);
    }
}