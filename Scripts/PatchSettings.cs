using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

public static class PatchSettings {

    public static readonly string FILE_PATH = Application.dataPath + "/Settings/patch_settings.json";

    public static SettingsData GetSettingsData() {
        SettingsData settings = new SettingsData();
        if (File.Exists(FILE_PATH)){
            string json = File.ReadAllText(FILE_PATH);
            settings = SettingsData.FromJson(json);
        }
        return settings;
    }

    public static void WriteSettingsData(SettingsData settings) {
        if (settings == null) {
            throw new NullReferenceException("SettingsData object is null.");
        }
        string directoryPath = Path.GetDirectoryName(FILE_PATH);
        if (!Directory.Exists(directoryPath)){
            Directory.CreateDirectory(directoryPath);
        }
        string newJson = SettingsData.ToJson(settings);
        Debug.Log(newJson);
        File.WriteAllText(FILE_PATH, newJson);
    }
}
