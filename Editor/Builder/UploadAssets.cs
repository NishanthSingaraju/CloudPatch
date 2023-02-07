#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEngine;
    using UnityEngine.Networking;
    using System.Collections;
    using System.Collections.Generic;

    public class UploadAssets{

        private static AddressableAssetSettings settings;
        public static string settings_asset
                = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
        
        static void getSettingsObject(string settingsAsset) {
            settings
                = AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsAsset)
                    as AddressableAssetSettings;

            if (settings == null)
                Debug.LogError($"{settingsAsset} couldn't be found or isn't " +
                                $"a settings object.");
        }

        public static string getAddressablePath(){
            getSettingsObject(settings_asset);
            var build_path = settings.RemoteCatalogBuildPath.GetValue(settings);
            return build_path;
        }

        public static void SetAdddressablePath(string path){
            getSettingsObject(settings_asset);
            foreach(var x in settings.profileSettings.GetVariableNames()) {
                Debug.Log(x.ToString());
            }
            foreach(var x in settings.profileSettings.GetAllProfileNames()) {
                Debug.Log(x.ToString());
            }
            settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath", path);
        }
    }
#endif