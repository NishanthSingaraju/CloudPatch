using System;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class EditAddressableRequest : MonoBehaviour{
    private IAuthHeader _authHeader;

    private void Awake(){
        DontDestroyOnLoad(gameObject);
        try{
            string filePath = PatchSettings.FILE_PATH;
            if (!File.Exists(filePath)){
                throw new FileNotFoundException($"File not found at {filePath}");
            }
            string jsonString = File.ReadAllText(filePath);
            SettingsData settings = SettingsData.FromJson(jsonString);
            ServiceType selectedServiceType = settings.service_type;
            switch (selectedServiceType){
                case ServiceType.GCPService:
                    string serviceAccountJsonPath = Application.streamingAssetsPath + (string)settings.parameters["service_account_json_path"];
                    Debug.Log(serviceAccountJsonPath);
                    if (!File.Exists(serviceAccountJsonPath)){
                        throw new FileNotFoundException($"File not found at {serviceAccountJsonPath}");
                    }
                    string service_account_json = File.ReadAllText(serviceAccountJsonPath);
                    _authHeader = new GCPAuthHeader(service_account_json);
                    _authHeader.SetAccessToken();
                    break;
                default:
                    Debug.LogError("Invalid service type selected");
                    break;
            }
        }
        catch (Exception e){
            Debug.LogError("Error reading file: " + e.Message);
        }
        Addressables.WebRequestOverride = (UnityWebRequest request) => {
            Debug.Log("WebRequestOverride called");
            _authHeader.AddHeader(request);
        };
    }
}