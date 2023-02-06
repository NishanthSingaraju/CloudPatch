#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public class UploadDirectoryWindow : EditorWindow
    {
        public string bucketName = "";
        public string localDir = "";
        public string target = "";
        private bool isUploading = false;

        private async void UploadFiles(){
            try{
                var arguments = new Dictionary<string, string>();
                arguments["bucketname"] = bucketName;
                arguments["local_dir"] = localDir;
                arguments["target"] = target;
                GCPService service = new GCPService();
                await service.UploadDirectory(arguments);
            }
            finally{
                isUploading = false;
                Close();
            }
        }

        private void OnGUI(){
            bucketName = EditorGUILayout.TextField("Bucket Name: ", bucketName).Trim();
            target = EditorGUILayout.TextField("Target: ", target).Trim();

            if (GUILayout.Button("Select Local Directory!"))
            {
                localDir = EditorUtility.OpenFolderPanel("Select Local Directory", "", "");
            }

            GUILayout.Space(10);

            if (!isUploading && GUILayout.Button("Upload"))
            {   
                Debug.Log("Uploading files");
                isUploading = true;
                UploadFiles();
            }
        }
    }
 #endif