#if UNITY_EDITOR
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public enum ServiceType { GCPService };

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

        [MenuItem("Cloud Patch/Set Settings")]
        static void SetSettings(){
            if (_currentService == null)
            {
                Debug.LogError("Please select a service type first");
                return;
            }
            
            _currentService.SetSettings();
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
    }
    }
 #endif


