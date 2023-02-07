#if UNITY_EDITOR
    using System;
    using System.IO;
    using UnityEngine.Networking;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using UnityEngine;
    using UnityEditor;

    public class GCPService: IBackendService
    {   
        private static string _accessToken;
        private static string service_account_json;
        public string bucket { get; set; }
        public string target { get; set; }
        public string RemoteBuildPath { get; private set; }
        public string runtimeServiceAccountPath = Application.dataPath + "/Resources/service_account.json";

        [Serializable]
        public class GetAccessTokenResponse{
            public string access_token;
        }

        public async Task SetSettings(){
            Debug.Log("Function: GetUploadDirectoryArguments");
            await GCPSpecificArgWindow.ShowWindowAsync();
            bucket = GCPSpecificArgWindow._bucket;
            target = GCPSpecificArgWindow._target;
            RemoteBuildPath = "https://storage.googleapis.com/" + bucket + "/" + target;
        }

        public void AuthenticateMenu(){
            string service_account_file = EditorUtility.OpenFilePanel("Select Service Account JSON", "", "json");
            service_account_json = File.ReadAllText(service_account_file);
            string directoryPath = Path.GetDirectoryName(runtimeServiceAccountPath);
            if (!Directory.Exists(directoryPath)){
                Directory.CreateDirectory(directoryPath);
            }
            File.WriteAllText(runtimeServiceAccountPath, service_account_json);
            Authenticate((res) => {
                Debug.Log("Success Auth!");
            });
            UpdateSettingsWithServiceAccount();
        }

        public void UploadDirectoryMenu(){
            UploadDirectoryWindow window = (UploadDirectoryWindow)EditorWindow.GetWindow(typeof(UploadDirectoryWindow));
            window.Show();
        }
        
        public async void Authenticate(Action<string> onSuccess){
            var form = new WWWForm();
            _accessToken = await ServiceAccountJsonToToken.GetServiceAccountAccessTokenAsync(service_account_json);
            onSuccess(null);
        }
        
        public async Task UploadDirectory(Dictionary<string, string> arguments){
            string bucket_name = arguments["bucket"];
            string local_dir = arguments["local_dir"];
            string target = arguments["target"];

            if (_accessToken == null){
                throw new System.Exception("Please call Authenticate method first to get access token.");
            }
            var dirInfo = new DirectoryInfo(local_dir);
            foreach (var file in dirInfo.GetFiles()){
                Debug.Log(file);
                await UploadFile(target, file, _accessToken, bucket_name);
        }
    }

        private static async Task UploadFile (string target, FileInfo fileInfo, string token, string bucket){
            var headers = new Dictionary<string, string> ();
            headers.Add ("Authorization", "Bearer " + token);
            headers.Add ("Content-Type", "application/octet-stream");

            var filename = target + "/" + fileInfo.Name;
            var url =  $"https://www.googleapis.com/upload/storage/v1/b/{bucket}/o?uploadType=media&name={filename}";
            byte[] fileBytes = File.ReadAllBytes(fileInfo.FullName);
            var uploadHandler = new UploadHandlerRaw (fileBytes);
            await PostRequest(url, null, uploadHandler, headers, response => {
                Debug.Log("Upload success: " + response);
            });
        }

        public Dictionary<string, string> GetUploadDirectoryArguments(string build_path){
            if (bucket == null || target == null){
                throw new System.Exception("Need to Set Google Settings.");
            }
            var arguments = new Dictionary<string, string>{
            { "bucket", bucket },
            { "target", target },
            { "local_dir", build_path }
            };
            return arguments;
        }

        public void UpdateSettingsWithServiceAccount(){
            // Load the existing settings.json file
            SettingsData settings = PatchSettings.GetSettingsData();
            // Update the settings data with the new service_account_json_path
            settings.service_type = ServiceType.GCPService;
            settings.parameters = new Dictionary<string, object>();
            settings.parameters["service_account_json_path"] = runtimeServiceAccountPath;
            // Save the updated settings data to the settings.json file
            PatchSettings.WriteSettingsData(settings);
        }

        public static async Task<bool> PostRequest(string url, WWWForm form, UploadHandler uploadHandler, Dictionary<string, string> headers, Action<string> onSuccess){
            using (UnityWebRequest r = UnityWebRequest.Post(url, form))
            {
                if (uploadHandler != null){
                    r.uploadHandler = uploadHandler;
                }
                if (headers != null){
                    foreach (var header in headers){
                        r.SetRequestHeader(header.Key, header.Value);
                    }
                }

                r.SendWebRequest();
                while (!r.isDone){
                    await Task.Yield();
                }

                if (r.isNetworkError || r.isHttpError){
                    Debug.Log(r.error);
                    return false;
                }
                else{
                    onSuccess(r.downloadHandler.text);
                    return true;
                }
            }
        }
    }
 #endif