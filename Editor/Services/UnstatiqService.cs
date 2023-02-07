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

    public class UnstatiqService: IBackendService{

        public string game { get; set; }
        public string target { get; set; }
        public string RemoteBuildPath { get; private set; }

        public async Task SetSettings(){
            await UnstatiqSpecificArgWindow.ShowWindowAsync();
            game = UnstatiqSpecificArgWindow._game;
            target = UnstatiqSpecificArgWindow._target;
        }

        public Dictionary<string, string> GetUploadDirectoryArguments(string build_path){
            if (game == null || target == null){
                throw new System.Exception("Need to Set Google Settings.");
            }
            var arguments = new Dictionary<string, string>{
            { "game", game },
            { "target", target },
            { "local_dir", build_path }
            };
            return arguments;
        }

        public void AuthenticateMenu(){}

        public async void Authenticate(){}

        public void UploadDirectoryMenu(){}

        public async Task UploadDirectory(Dictionary<string, string> arguments){}

    }
 #endif