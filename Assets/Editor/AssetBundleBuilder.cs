using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class AssetBundleBuilder : MonoBehaviour
{
    [MenuItem("Tools/Build AssetBundles")]
    public static void BuildAllAssetBundles()
    {
        string bundleDir = "AssetBundles";
        if(!Directory.Exists(bundleDir))
        {
            Directory.CreateDirectory(bundleDir);
        }

        BuildPipeline.BuildAssetBundles(bundleDir, BuildAssetBundleOptions.None, BuildTarget.Android);
    }
}
