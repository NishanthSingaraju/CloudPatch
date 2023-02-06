#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.AddressableAssets.Build;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEditor.AddressableAssets;
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    public class BuildAssets
    {
        public static string build_script
            = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";
        public static string settings_asset
            = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
        public static string profile_name = "Default";
        private static AddressableAssetSettings settings;

        static void getSettingsObject(string settingsAsset) {
            // This step is optional, you can also use the default settings:
            //settings = AddressableAssetSettingsDefaultObject.Settings;

            settings
                = AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsAsset)
                    as AddressableAssetSettings;

            if (settings == null)
                Debug.LogError($"{settingsAsset} couldn't be found or isn't " +
                               $"a settings object.");
        }

        static void setProfile(string profile) {
            string profileId = settings.profileSettings.GetProfileId(profile);
            if (String.IsNullOrEmpty(profileId))
                Debug.LogWarning($"Couldn't find a profile named, {profile}, " +
                                 $"using current profile instead.");
            else
                settings.activeProfileId = profileId;
        }

        static void setBuilder(IDataBuilder builder) {
            int index = settings.DataBuilders.IndexOf((ScriptableObject)builder);

            if (index > 0)
                settings.ActivePlayerDataBuilderIndex = index;
            else
                Debug.LogWarning($"{builder} must be added to the " +
                                 $"DataBuilders list before it can be made " +
                                 $"active. Using last run builder instead.");
        }

        static bool buildAddressableContent() {
            AddressableAssetSettings
                .BuildPlayerContent(out AddressablesPlayerBuildResult result);
            bool success = string.IsNullOrEmpty(result.Error);

            if (!success) {
                Debug.LogError("Addressables build error encountered: " + result.Error);
            }
            return success;
        }
        
        [MenuItem("Cloud Patch/Build Addressables")]
        public static bool BuildAddressables() {
            getSettingsObject(settings_asset);
            setProfile(profile_name);
            IDataBuilder builderScript
              = AssetDatabase.LoadAssetAtPath<ScriptableObject>(build_script) as IDataBuilder;

            if (builderScript == null) {
                Debug.LogError(build_script + " couldn't be found or isn't a build script.");
                return false;
            }

            setBuilder(builderScript);

            return buildAddressableContent();
        }

        public static void UpdateAddressables(string groupName = ""){
            List<AddressableAssetGroup> groups;
            if (!string.IsNullOrEmpty(groupName)){
                var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup(groupName);
                if (group == null){
                    Debug.LogError($"No group found with name: {groupName}");
                    return;
                }
                groups = new List<AddressableAssetGroup> { group };
            }
            else{
                groups = AddressableAssetSettingsDefaultObject.Settings.groups;
            }

            foreach (var g in groups){
                var path = ContentUpdateScript.GetContentStateDataPath(false);
                var result = ContentUpdateScript.BuildContentUpdate(g.Settings, path);
                if (!string.IsNullOrEmpty(result.Error)){
                    Debug.LogError(result.Error);
                }
            }
        }
    };
#endif