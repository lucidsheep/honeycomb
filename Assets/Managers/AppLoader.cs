using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.IO;
using UnityEngine.Networking;
using TMPro;
using System;

//todo - app update absolutely does not work on macos, not sure if there's an easy way to do it
public class AppLoader : MonoBehaviour
{
    public static int APP_VERSION = 87;

    public GameObject[] localBundles;
    public ProgressBar loadingBar;
    public TextMeshPro buildText;
    public bool useLocalBundle;
    public bool useCache;
    public bool clearCache = false;

    public static bool SKIP_SETUP = false;

    List<(float, bool)> bundleProgress = new List<(float, bool)>();
    List<GameObject> loadedBundles = new List<GameObject>();
    string currentJson;

    bool offlineMode = false;
    bool jsonLoaded = false;

    static string slash;

    // Start is called before the first frame update
    void Awake()
    {
        Application.targetFrameRate = 60;

        string platform = GetPlatform();
        slash = platform == "windows" ? "\\" : "/"; // <3 Windows
        var baseURL = "https://kq.style/honeycomb/";
        Debug.Log("version " + APP_VERSION);
        buildText.text = "version " + APP_VERSION;
        UnityEngine.QualitySettings.SetQualityLevel(0, true);
        CreateUpdateScript();
        CreateKQuityScript();
        if (clearCache)
            Caching.ClearCache();
        try
        { 
            StreamReader configReader = new StreamReader(Application.streamingAssetsPath + slash + "config.json");
            var json = configReader.ReadToEnd();
            configReader.Close();
            var config = JsonUtility.FromJson<ConfigData>(json);
            if (config.baseURL != "")
            {
                baseURL = config.baseURL;
                if (baseURL[baseURL.Length - 1] != '/') baseURL += '/'; //forgiveness on trailing slash
            }
            if (config.frameRate >= 0)
                Application.targetFrameRate = config.frameRate;
            if(config.resolution != "" || config.fullScreen)
            {
                bool fullScreen = config.fullScreen;
                int width = Screen.width;
                int height = Screen.height;
                string[] parts = config.resolution.Split("x");
                if(parts.Length > 1)
                {
                    int.TryParse(parts[0], out width);
                    int.TryParse(parts[1], out height);
                }
                Screen.SetResolution(width, height, fullScreen);
            }
            if(config.theme != "")
            {
                ViewModel.defaultTheme = config.theme;
            }
        } catch(Exception e)
        {
            //no config file found, proceed
            Debug.Log("no config file found, or config file formatted incorrectly");
            Debug.Log(e.Message);
        }

        //command line args
        var args = Environment.GetCommandLineArgs();
        if (args.Length > 0)
        {
            Debug.Log("command line args: " + args.Length);
            for (int i = 0; i < args.Length; i++)
            {
                bool nextArg = i + 1 < args.Length;
                var arg = args[i];
                Debug.Log(arg);
                switch(arg)
                {
                    case "setTheme":
                        if (!nextArg) break;
                        var next = args[i + 1];
                        ViewModel.defaultTheme = next;
                        PlayerPrefs.SetString("theme", next);
                        break;
                    case "skipSetup":
                        SKIP_SETUP = true;
                        break;
                }

            }
        }
        if (useLocalBundle)
        {
            foreach(var bundleObj in localBundles)
                Instantiate(bundleObj);
            FinishLoad();
        } else
        {
            StartCoroutine(LoadRemoteBundles(baseURL, platform));
        }
    }

    void CreateUpdateScript()
    {
        StreamWriter updateScript = new StreamWriter(Application.dataPath + slash + ".." + slash + "update.bat", false);
        updateScript.Write("timeout /t 5\nif exist update.zip tar -xf update.zip\nif exist update.zip del update.zip\nstart Honeycomb.exe");
        updateScript.Close();
    }

    void CreateKQuityScript()
    {
        StreamWriter updateScript = new StreamWriter(Application.dataPath + slash + ".." + slash + "kquity.bat", false);
        updateScript.Write("cd \"" + Application.dataPath + "\\..\"\nif exist kquity.zip tar -xf kquity.zip\nif exist kquity.zip del kquity.zip\ncd kquity\nstart /min kquity.exe");
        updateScript.Close();
    }

    [System.Serializable]
    public class BundleHash
    {
        public string bundle;
        public string hash;
    }

    [System.Serializable]
    public class ConfigData
    {
        public string baseURL = "";
        public string resolution = "";
        public bool fullScreen = false;
        public int frameRate = -1;
        public string theme = "";
    }

    [System.Serializable]
    public class BundleList
    {
        public BundleHash[] bundles;
        public string kquity;
        public int version;
    }

    IEnumerator LoadRemoteBundles(string baseURL, string platform)
    {
        string jsonURL = baseURL + platform + "/version.json";
        string assetURL = baseURL + platform + "/";
        using (var hashReq = UnityWebRequest.Get(jsonURL))
        {
            yield return hashReq.SendWebRequest();
            if (hashReq.result == UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<BundleList>(hashReq.downloadHandler.text);
                currentJson = hashReq.downloadHandler.text;
                if (APP_VERSION < result.version)
                {
                    Debug.Log("verion update required! starting download");
                    StartCoroutine(UpdateApp(baseURL, platform));
                    jsonLoaded = true;
                }
                else
                {
                    BeginLoadingBundles(result, assetURL);
                    jsonLoaded = true;
                }
            } else
            {
                StartOfflineMode(assetURL);
            }
        }
        
    }

    IEnumerator UpdateApp(string baseURL, string platform)
    {
        bundleProgress = new List<(float, bool)>();
        bundleProgress.Add((0f, false));
        Debug.Log("starting update...");
        using (var zipReq = UnityWebRequest.Get(baseURL + platform + "/update.zip"))
        {
            zipReq.downloadHandler = new DownloadHandlerFile(Application.dataPath + slash + ".." + slash + "update.zip");
            var request = zipReq.SendWebRequest();
            while (!request.isDone)
            {
                    bundleProgress[0] = (request.progress, false);
                yield return null;
            }
            if (zipReq.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("finished downloading zip, starting update script...");
                System.Diagnostics.Process.Start(Application.dataPath + slash + ".." + slash + "update.bat");
                Application.Quit();
            } else
            {
                Debug.Log("app update failed, falling back to offline mode");
                StartOfflineMode(baseURL + platform + "/");
            }
        }
    }
    void StartOfflineMode(string assetURL)
    {
        if (offlineMode) return;

        offlineMode = true;
        Debug.Log("A download failed, dropping back to cached packages");

        string savedJson = PlayerPrefs.GetString("cachedJson");
        if(savedJson == null || savedJson == "")
        {
            Debug.Log("No cached data found, failing out");
            return;
        }

        var result = JsonUtility.FromJson<BundleList>(savedJson);
        currentJson = savedJson;
        jsonLoaded = true;
        BeginLoadingBundles(result, assetURL);
    }

    void BeginLoadingBundles(BundleList bundleList, string baseURL)
    {
        int bundleIndex = 0;
        bundleProgress = new List<(float, bool)>();
        loadedBundles = new List<GameObject>();
        foreach (var bundle in bundleList.bundles)
        {
            StartCoroutine(LoadRemoteBundle(bundle, baseURL, bundleIndex, offlineMode));
            bundleIndex++;
        }
        if(bundleList.kquity != "")
        {
            StartCoroutine(LoadKQuity(bundleList.kquity, baseURL, bundleIndex, offlineMode));
            bundleIndex++;
        }
    }
    IEnumerator LoadKQuity(string hash, string baseURL, int bundleIndex, bool offline = false)
    {
        bundleProgress.Add((0f, false));

        var finalURL = baseURL + "kquity.zip";
        if(hash == PlayerPrefs.GetString("kquity"))
        {
            //already have latest version
            bundleProgress[bundleIndex] = (1f, true);
        } else
        {
            var handler = new DownloadHandlerFile(Application.dataPath + slash + ".." + slash + "kquity.zip");
            using (var webRequest = UnityWebRequest.Get(finalURL))
            {
                Debug.Log("started download of kquity");
                webRequest.downloadHandler = handler;
                var request = webRequest.SendWebRequest();
                while (!request.isDone)
                {
                    if (offline != offlineMode) // mode switched, cancel download
                        webRequest.Abort();
                    else
                        bundleProgress[bundleIndex] = (request.progress, false);
                    yield return null;
                }
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("finished getting kquity");
                    bundleProgress[bundleIndex] = (1f, true);
                    PlayerPrefs.SetString("kquity", hash);
                }
                else if (!offline)
                {
                    StartOfflineMode(baseURL);
                }
            }
        }
    }

    IEnumerator LoadRemoteBundle(BundleHash bundle, string baseURL, int bundleIndex, bool offline = false)
    {
        var hash = Hash128.Parse(bundle.hash);
        var finalURL = baseURL + bundle.bundle;
        bundleProgress.Add((0f, false));
        using (var webRequest = useCache ? UnityWebRequestAssetBundle.GetAssetBundle(finalURL, hash) : UnityWebRequestAssetBundle.GetAssetBundle(finalURL))
        {
            Debug.Log("starting download of " + bundle.bundle);
            var request = webRequest.SendWebRequest();
            while (!request.isDone)
            {
                if (offline != offlineMode) // mode switched, cancel download
                    webRequest.Abort();
                else
                    bundleProgress[bundleIndex] = (request.progress, false);
                yield return null;
            }
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("finished getting " + bundle.bundle + ", loading asset");
                bundleProgress[bundleIndex] = (1f, true);
                var bundleAsset = DownloadHandlerAssetBundle.GetContent(webRequest);
                var asset = bundleAsset.LoadAsset<GameObject>(bundle.bundle);
                loadedBundles.Add(asset);
            } else if(!offline)
            {
                StartOfflineMode(baseURL);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!jsonLoaded) return;

        float totalProgress = 0f;
        bool allDone = true;
        foreach (var prog in bundleProgress)
        {
            totalProgress += prog.Item1;
            if (!prog.Item2) allDone = false;
        }
        loadingBar.SetFill(totalProgress / (float)bundleProgress.Count);

        if (allDone)
            FinishLoad();
    }

    void FinishLoad()
    {
        Debug.Log("Packages loaded! Starting honeycomb v" + APP_VERSION);
        foreach (var asset in loadedBundles)
            Instantiate(asset);
        PlayerPrefs.SetString("cachedJson", currentJson);
        //self destruct!
        Destroy(this.gameObject);
    }
    string GetPlatform()
    {
        switch(Application.platform)
        {
            case RuntimePlatform.WindowsPlayer: case RuntimePlatform.WindowsEditor: return "windows";
            case RuntimePlatform.OSXEditor: case RuntimePlatform.OSXPlayer: return "macos";
        }
        return "unknown";
    }

    public static Sprite GetStreamingSprite(string assetName)
    {
        string path = Application.streamingAssetsPath + slash + "themes" + slash + ViewModel.currentTheme.themeName + slash + assetName + ".png";
        if (File.Exists(path))
        {
            Debug.Log("loading " + path);
            Texture2D ret = new Texture2D(8, 8);
            ret.LoadImage(File.ReadAllBytes(path), true);
            return Sprite.Create(ret, new Rect(0f, 0f, ret.width, ret.height), new Vector2(.5f, .5f));
        } else
        {
            Debug.Log("file not found at " + path);
        }
        return null;
    }
}
