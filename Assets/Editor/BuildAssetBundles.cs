using UnityEditor;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System;

public class BuildAssetBundles
{

    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleMainDirectory = "Assets/AssetBundles/";
        string assetBundleDirectoryWindows = "Assets/AssetBundles/windows";
        string assetBundleDirectoryMac = "Assets/AssetBundles/macos";
        string kquityBundleWindows = "Assets/AssetBundles/windows/kquity.zip";
        string kquityBundleMac = "Assets/AssetBundles/macos/kquity.zip";
        if (!Directory.Exists(assetBundleMainDirectory))
        {
            Directory.CreateDirectory(assetBundleMainDirectory);
        }
        if (!Directory.Exists(assetBundleDirectoryWindows))
        {
            Directory.CreateDirectory(assetBundleDirectoryWindows);
        }
        if (!Directory.Exists(assetBundleDirectoryMac))
        {
            Directory.CreateDirectory(assetBundleDirectoryMac);
        }

        //windows
        var winManifest = BuildPipeline.BuildAssetBundles(assetBundleDirectoryWindows,
                                        BuildAssetBundleOptions.None,
                                        BuildTarget.StandaloneWindows64);
        string json = "{ \"version\": " + AppLoader.APP_VERSION + ", \"bundles\": [ ";
        for (int i = 0; i < winManifest.GetAllAssetBundles().Length; i++)
        {
            var bundle = winManifest.GetAllAssetBundles()[i];
            json += "{ \"bundle\": \"" + bundle + "\", \"hash\": \"" + winManifest.GetAssetBundleHash(bundle) + "\" }";
            if (i + 1 < winManifest.GetAllAssetBundles().Length)
                json += ", ";
        }
        json += "]";
        using (var md5 = MD5.Create())
        {
            using (var file = File.OpenRead(kquityBundleWindows))
            {
                json += ", \"kquity\": \"" + BitConverter.ToString(md5.ComputeHash(file)).Replace("-", "").ToLowerInvariant() + "\" ";
            }
        }
            
        json += "}";
        StreamWriter writer = new StreamWriter(assetBundleDirectoryWindows + "/version.json", false);
        writer.WriteLine(json);
        writer.Close();

        //mac
        var macManifest = BuildPipeline.BuildAssetBundles(assetBundleDirectoryMac,
                                BuildAssetBundleOptions.None,
                                BuildTarget.StandaloneOSX);
        json = json = "{ \"version\": " + AppLoader.APP_VERSION + ", \"bundles\": [ ";
        for (int i = 0; i <  macManifest.GetAllAssetBundles().Length; i++)
        {
            var bundle = macManifest.GetAllAssetBundles()[i];
            json += "{ \"bundle\": \"" + bundle + "\", \"hash\": \"" + macManifest.GetAssetBundleHash(bundle) + "\" }";
            if (i + 1 < macManifest.GetAllAssetBundles().Length)
                json += ", ";
        }
        json += "]";
        using (var md5 = MD5.Create())
        {
            using (var file = File.OpenRead(kquityBundleMac))
            {
                json += ", \"kquity\": \"" + BitConverter.ToString(md5.ComputeHash(file)).Replace("-", "").ToLowerInvariant() + "\" ";
            }
        }

        json += "}";
        writer = new StreamWriter(assetBundleDirectoryMac + "/version.json", false);
        writer.WriteLine(json);
        writer.Close();
    }
}