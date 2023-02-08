#if UNITY_EDITOR
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    using UnityEditor;
    using UnityEngine.SceneManagement;
    using UnityEditor.SceneManagement;


    public class BackendService{   
        public static ServiceType serviceType;
        private static IBackendService _currentService;

        [MenuItem("Cloud Patch/Set Service Type")]
        static void SetServiceTypeMenu(){
            SetServiceTypeWindow window = (SetServiceTypeWindow)EditorWindow.GetWindow(typeof(SetServiceTypeWindow));
            window.Show();
        }

        public static void SetServiceType(ServiceType type) {
            switch (type) {
                case ServiceType.GCPService:
                    _currentService = new GCPService();
                    break;
                default:
                    Debug.LogError("Invalid service type selected");
                    break;
            }
             SetUpAfterServiceSelected();
        }

        [MenuItem("Cloud Patch/Set Settings")]
        static async void SetSettings(){
            if (_currentService == null)
            {
                Debug.LogError("Please select a service type first");
                return;
            }
            
            await _currentService.SetSettings();
            UpdateRemoteBundlePath();
        }


        [MenuItem("Cloud Patch/Authenticate")]
        static void AuthenticateMenu(){
            if (_currentService == null)
            {
                Debug.LogError("Please select a service type first");
                return;
            }
            _currentService.AuthenticateMenu();
        }


        [MenuItem("Cloud Patch/Upload Directory")]
        static async void UploadAddressables(){
            if (_currentService == null){
                Debug.LogError("Please select a service type first");
                return;
            }
            string build_path = UploadAssets.getAddressablePath();
            Dictionary<string, string> arguments = _currentService.GetUploadDirectoryArguments(build_path);
            await _currentService.UploadDirectory(arguments);
        }

        [MenuItem("Cloud Patch/Patch")]
        static async void Patch(){
          BuildAssets.UpdateAddressables();
          UploadAddressables();
        }

        static void UpdateRemoteBundlePath() {
            Debug.Log(_currentService.RemoteBuildPath);
            if (_currentService != null) {
                UploadAssets.SetAdddressablePath(_currentService.RemoteBuildPath);
            }
        }

        static void SetUpAfterServiceSelected(){
            SettingsData settings = PatchSettings.GetSettingsData();
            settings.service_type = serviceType;
            PatchSettings.WriteSettingsData(settings);
            Scene firstScene = EditorSceneManager.GetSceneAt(0);
            if (!firstScene.isLoaded) {
                EditorSceneManager.OpenScene(firstScene.path);
            }
            EditAddressableRequest[] requests = Resources.FindObjectsOfTypeAll<EditAddressableRequest>();
            if (requests.Length == 0) {
                GameObject obj = new GameObject("EditAddressableRequest");
                obj.AddComponent<EditAddressableRequest>();
                EditorSceneManager.MarkSceneDirty(firstScene);
            }
        }
    }
 #endif