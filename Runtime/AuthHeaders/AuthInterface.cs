using UnityEngine.Networking;
using System.Threading.Tasks;

public abstract class IAuthHeader{
    public abstract void AddHeader(UnityWebRequest request);
    public abstract void SetAccessToken();
}