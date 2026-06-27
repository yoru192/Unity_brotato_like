using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BerserkPixel.Tilemap_Generator
{
    public static class DirectoryHelpers
    {

    #if UNITY_EDITOR

        private static string SetupDirectory(string savePath, string prefabName)
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
                
            savePath += prefabName;
            return savePath;
        }

        public static void SaveMap(string saveName, GameObject grid, Action<string> onError, Action<string> onSuccess)
        {
            if (grid)
            {
                var savePath = SetupDirectory(
                    "Assets/Prefabs/Maps/Generated/",
                    saveName + ".prefab"
                );

                if (PrefabUtility.SaveAsPrefabAsset(grid, savePath))
                {
                    onSuccess($"Your Tilemap was saved under {savePath}" +
                              $"\n\nMake sure that you place it as a child of a Grid GameObject");
                }
                else
                {
                    onError("An ERROR occured while trying to saveTilemap under" + savePath);
                }
            }
            else
            {
                onError("There must be a GameObject with a Grid component attached!");
            }
        }

    #endif

    }
}