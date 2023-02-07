using System;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Threading;

public class GCPAuthHeader : IAuthHeader
{
    private string _accessToken;
    private string _serviceAccountJson;

    public GCPAuthHeader(string serviceAccountJson){   
       _serviceAccountJson = serviceAccountJson;
    }

    public override void SetAccessToken(){
        _accessToken = ServiceAccountJsonToToken.GetServiceAccountAccessToken(_serviceAccountJson);
    }

    public override void AddHeader(UnityWebRequest request){
        request.SetRequestHeader("Authorization", $"Bearer {_accessToken}");
    }

}



