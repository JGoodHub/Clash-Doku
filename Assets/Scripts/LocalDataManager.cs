using System;
using System.Collections.Generic;
using GoodHub.Core.Runtime;
using Newtonsoft.Json;
using UnityEngine;

public class LocalDataManager : GlobalSingleton<LocalDataManager>
{

    public const string PREFS_KEY = "local_data_stroe";

    private LocalData _data;

    public static LocalData Data => Instance._data;

    protected override void Awake()
    {
        base.Awake();

        LoadFromPrefs();
    }

    public static void Save()
    {
        string serialisedData = JsonConvert.SerializeObject(Data, Formatting.Indented);
        PlayerPrefs.SetString(PREFS_KEY, serialisedData);
    }

    private void LoadFromPrefs()
    {
        string serialisedData = PlayerPrefs.HasKey(PREFS_KEY) ? PlayerPrefs.GetString(PREFS_KEY) : string.Empty;
        _data = new LocalData();

        if (string.IsNullOrWhiteSpace(serialisedData) == false)
            JsonConvert.PopulateObject(serialisedData, _data);
    }

}

[Serializable]
public class LocalData
{

    public List<MatchReport> MatchReports = new List<MatchReport>();

}