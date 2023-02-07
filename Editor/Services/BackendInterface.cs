using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.Threading.Tasks;

public interface IBackendService{
    string RemoteBuildPath { get; }
    Task SetSettings();
    void AuthenticateMenu();
    Task UploadDirectory(Dictionary<string, string> arguments);
    void UploadDirectoryMenu();
    Dictionary<string, string> GetUploadDirectoryArguments(string build_path);
}