using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class GoogleSheetsDB : MonoBehaviour
{
    public bool isOnlineMode = true;
    public string googleSheetAddress = "";
    public bool saveRemoteData = true;
    public string FolderName = "GameData";
    public string DBPath;

    public List<string> sheetTabNames;
    public List<GoogleSheet> dataSheets;

    Dictionary<string, string> scenesToAPIKeys = new Dictionary<string, string>();

    private string _googleSheetsRequestAddressFormat = "https://script.google.com/macros/s/{0}/exec?sheetNameString={1}";
    private int _numDataSheetsAdded = 0;

    public event System.Action OnDownloadComplete;

    public static GoogleSheetsDB instance;

    public void Awake()
    {
        instance = this;
        dataSheets = new List<GoogleSheet>();

        DBPath = Application.persistentDataPath + "/" + FolderName + "/";
        if (!Directory.Exists(DBPath))
        {
            Directory.CreateDirectory(DBPath);
        }

        //hardcoded dictionary
        scenesToAPIKeys.Add("kqpdx", "AKfycbz8pURpJmFAY6WDan5b4A4NyoxXP4r5eoYfpaQReR7LMTyMaMce05rrtPWY4mjuKQBp");
        scenesToAPIKeys.Add("fpkql", "AKfycbxfJFDZLKCu8MgcpqLCljO5Rz5ZGWdjH21t7W1FlS4ByLYgQmzo1DS9e9BT29Rt1_fC_g");
        scenesToAPIKeys.Add("campkq", "AKfycbyNUmoYPQz3yitZ4wc0LyL_DQRgbWOak1PaTWLgvk5TFG6l2C9Y0oLCGZB8xr-9Tfcs");
    }

    public void Start()
    {
        //ImportData();
        
    }

    public void ImportData(string sceneName)
    {
        if(!scenesToAPIKeys.ContainsKey(sceneName))
        {
            return;
        }
        googleSheetAddress = scenesToAPIKeys[sceneName];

        if (sheetTabNames.Count == 0)
        {
            Debug.LogError("Add name of the sheet tabs to download from");
            return;
        }

        if (isOnlineMode)
        {
            StartCoroutine(ImportRemoteData());
        }
        else
        {
            ImportLocalData();
        }
    }


    public IEnumerator ImportRemoteData()
    {
        if (googleSheetAddress != "")
        {
            for (var i = 0; i < sheetTabNames.Count; i++)
            {
                StartCoroutine(RequestSheet(sheetTabNames[i]));
            }

            yield return new WaitUntil(() => dataSheets.Count == sheetTabNames.Count);

            OnDownloadComplete();
        }
        else
        {
            Debug.LogError("Check the GS Address or check the tab name");
        }
    }
    public void ImportLocalData()
    {
        //WIP
    }

    public IEnumerator RequestSheet(string sheetName)
    {
        var googleSheet = new GoogleSheet(_googleSheetsRequestAddressFormat, googleSheetAddress, sheetName, new UnityWebRequestProvider());
        googleSheet.OnlineUpdateSheetData((success) => {});

        yield return new WaitUntil(() => googleSheet.isDataUpdatingFinished == true);

        dataSheets.Add(googleSheet);

        if (isOnlineMode && saveRemoteData)
        {
            SaveSheet(googleSheet);
        }
    }

    public void SaveSheet(GoogleSheet googleSheet)
    {
        string fileName = DBPath + googleSheet.sheetName + ".json";
        var jsonStr = MiniJSON.Json.Serialize(googleSheet.sheetData);
        System.IO.File.WriteAllText(fileName, jsonStr);

        Debug.Log("Online dataSheets Saved: " + googleSheet.sheetName);
    }
    public void LoadSheet()
    {
        //WIP
    }
}
