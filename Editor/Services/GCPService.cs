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

        [Serializable]
        public class GetAccessTokenResponse{
            public string access_token;
        }

        public async void SetSettings(){
            Debug.Log("Function: GetUploadDirectoryArguments");
            await GCPSpecificArgWindow.ShowWindowAsync();
            bucket = GCPSpecificArgWindow._bucket;
            target = GCPSpecificArgWindow._target;
        }

        public void AuthenticateMenu(){
            string service_account_file = EditorUtility.OpenFilePanel("Select Service Account JSON", "", "json");
            service_account_json = File.ReadAllText(service_account_file);
            Authenticate((res) => {
                Debug.Log("Success Auth!");
            });
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

    public class ServiceAccountJsonToToken
    {
        private struct JwtResponse{
            public string access_token;
            public int expires_in;
            public string token_type;
        }

        private struct JwtObject{
            public string iss;
            public string scope;
            public string aud;
            public long exp;
            public long iat;
        }

        [Serializable]
        private class ServiceAccount{
            public string type;
            public string project_id;
            public string private_key_id;
            public string private_key;
            public string client_email;
            public string client_id;
            public string auth_uri;
            public string token_uri;
            public string auth_provider_x509_cert_url;
            public string client_x509_cert_url;
        }

        public async static Task<string> GetServiceAccountAccessTokenAsync(string serviceAccountJson){   
            var serviceAccount = JsonUtility.FromJson<ServiceAccount>(serviceAccountJson);
            var email = serviceAccount.client_email;
            var privateKey = serviceAccount.private_key;
            string jwtHeader = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9";
            var time = DateTimeOffset.Now.ToUnixTimeSeconds();
            int expiresInSecond = 120;

            JwtObject payload = new JwtObject{
                iss = email,
                scope = "https://www.googleapis.com/auth/devstorage.read_write",
                aud = "https://www.googleapis.com/oauth2/v4/token",
                exp = time + expiresInSecond,
                iat = time,
            };

            var jsonString = JsonUtility.ToJson(payload);
            string jwtClaimSet = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
            var rsaParameters = DecodeRsaParameters(privateKey);

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaParameters);
            var signatureBytes = rsa.SignData(Encoding.UTF8.GetBytes($"{jwtHeader}.{jwtClaimSet}"), "SHA256");
            var jwtSignature = Convert.ToBase64String(signatureBytes);

            string completeJwt = $"{jwtHeader}.{jwtClaimSet}.{jwtSignature}";

            var req = UnityWebRequest.Post("https://www.googleapis.com/oauth2/v4/token", new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                ["assertion"] = completeJwt
            });
            req.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            var ao = req.SendWebRequest();
            while (ao.isDone == false){
                await Task.Yield();
            }

            var res = JsonUtility.FromJson<JwtResponse>(ao.webRequest.downloadHandler.text);
            if (ao.webRequest.isHttpError || ao.webRequest.isNetworkError){
                throw new Exception($"Getting service account access token error! {ao.webRequest.error} {ao.webRequest.downloadHandler.text}");
            }
            return res.access_token;
        }

        internal class Asn1
        {
            internal enum Tag
            {
                Integer = 2,
                OctetString = 4,
                Null = 5,
                ObjectIdentifier = 6,
                Sequence = 16,
            }

            internal class Decoder
            {
                public Decoder(byte[] bytes)
                {
                    _bytes = bytes;
                    _index = 0;
                }

                private byte[] _bytes;
                private int _index;

                public object Decode()
                {
                    Tag tag = ReadTag();
                    switch (tag)
                    {
                        case Tag.Integer:
                            return ReadInteger();
                        case Tag.OctetString:
                            return ReadOctetString();
                        case Tag.Null:
                            return ReadNull();
                        case Tag.ObjectIdentifier:
                            return ReadOid();
                        case Tag.Sequence:
                            return ReadSequence();
                        default:
                            throw new NotSupportedException($"Tag '{tag}' not supported.");
                    }
                }

                private byte NextByte() => _bytes[_index++];

                private byte[] ReadLengthPrefixedBytes()
                {
                    int length = ReadLength();
                    return ReadBytes(length);
                }

                private byte[] ReadInteger() => ReadLengthPrefixedBytes();

                private object ReadOctetString()
                {
                    byte[] bytes = ReadLengthPrefixedBytes();
                    return new Decoder(bytes).Decode();
                }

                private object ReadNull()
                {
                    int length = ReadLength();
                    if (length != 0)
                    {
                        throw new InvalidDataException("Invalid data, Null length must be 0.");
                    }
                    return null;
                }

                private int[] ReadOid()
                {
                    byte[] oidBytes = ReadLengthPrefixedBytes();
                    List<int> result = new List<int>();
                    bool first = true;
                    int index = 0;
                    while (index < oidBytes.Length)
                    {
                        int subId = 0;
                        byte b;
                        do
                        {
                            b = oidBytes[index++];
                            if ((subId & 0xff000000) != 0)
                            {
                                throw new NotSupportedException("Oid subId > 2^31 not supported.");
                            }
                            subId = (subId << 7) | (b & 0x7f);
                        } while ((b & 0x80) != 0);
                        if (first)
                        {
                            first = false;
                            result.Add(subId / 40);
                            result.Add(subId % 40);
                        }
                        else
                        {
                            result.Add(subId);
                        }
                    }
                    return result.ToArray();
                }

                private object[] ReadSequence()
                {
                    int length = ReadLength();
                    int endOffset = _index + length;
                    if (endOffset < 0 || endOffset > _bytes.Length)
                    {
                        throw new InvalidDataException("Invalid sequence, too long.");
                    }
                    List<object> sequence = new List<object>();
                    while (_index < endOffset)
                    {
                        sequence.Add(Decode());
                    }
                    return sequence.ToArray();
                }

                private byte[] ReadBytes(int length)
                {
                    if (length <= 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(length), "length must be positive.");
                    }
                    if (_bytes.Length - length < 0)
                    {
                        throw new ArgumentException("Cannot read past end of buffer.");
                    }
                    byte[] result = new byte[length];
                    Array.Copy(_bytes, _index, result, 0, length);
                    _index += length;
                    return result;
                }

                private Tag ReadTag()
                {
                    byte b = NextByte();
                    int tag = b & 0x1f;
                    if (tag == 0x1f)
                    {
                        // A tag value of 0x1f (31) indicates a tag value of >30 (spec section 8.1.2.4)
                        throw new NotSupportedException("Tags of value > 30 not supported.");
                    }
                    else
                    {
                        return (Tag)tag;
                    }
                }

                private int ReadLength()
                {
                    byte b0 = NextByte();
                    if ((b0 & 0x80) == 0)
                    {
                        return b0;
                    }
                    else
                    {
                        if (b0 == 0xff)
                        {
                            throw new InvalidDataException("Invalid length byte: 0xff");
                        }
                        int byteCount = b0 & 0x7f;
                        if (byteCount == 0)
                        {
                            throw new NotSupportedException("Lengths in Indefinite Form not supported.");
                        }
                        int result = 0;
                        for (int i = 0; i < byteCount; i++)
                        {
                            if ((result & 0xff800000) != 0)
                            {
                                throw new NotSupportedException("Lengths > 2^31 not supported.");
                            }
                            result = (result << 8) | NextByte();
                        }
                        return result;
                    }
                }

            }

            public static object Decode(byte[] bs) => new Decoder(bs).Decode();

        }

        public static RSAParameters DecodeRsaParameters(string pkcs8PrivateKey)
        {
            const string PrivateKeyPrefix = "-----BEGIN PRIVATE KEY-----";
            const string PrivateKeySuffix = "-----END PRIVATE KEY-----";

            pkcs8PrivateKey = pkcs8PrivateKey.Trim();
            if (!pkcs8PrivateKey.StartsWith(PrivateKeyPrefix) || !pkcs8PrivateKey.EndsWith(PrivateKeySuffix))
            {
                throw new ArgumentException(
                    $"PKCS8 data must be contained within '{PrivateKeyPrefix}' and '{PrivateKeySuffix}'.", nameof(pkcs8PrivateKey));
            }
            string base64PrivateKey =
                pkcs8PrivateKey.Substring(PrivateKeyPrefix.Length, pkcs8PrivateKey.Length - PrivateKeyPrefix.Length - PrivateKeySuffix.Length);
            // FromBase64String() ignores whitespace, so further Trim()ing isn't required.
            byte[] pkcs8Bytes = Convert.FromBase64String(base64PrivateKey);

            object ans1 = Asn1.Decode(pkcs8Bytes);
            object[] parameters = (object[])((object[])ans1)[2];

            var rsaParmeters = new RSAParameters
            {
                Modulus = TrimLeadingZeroes((byte[])parameters[1]),
                Exponent = TrimLeadingZeroes((byte[])parameters[2], alignTo8Bytes: false),
                D = TrimLeadingZeroes((byte[])parameters[3]),
                P = TrimLeadingZeroes((byte[])parameters[4]),
                Q = TrimLeadingZeroes((byte[])parameters[5]),
                DP = TrimLeadingZeroes((byte[])parameters[6]),
                DQ = TrimLeadingZeroes((byte[])parameters[7]),
                InverseQ = TrimLeadingZeroes((byte[])parameters[8]),
            };

            return rsaParmeters;
        }

        internal static byte[] TrimLeadingZeroes(byte[] bs, bool alignTo8Bytes = true)
        {
            int zeroCount = 0;
            while (zeroCount < bs.Length && bs[zeroCount] == 0) zeroCount += 1;

            int newLength = bs.Length - zeroCount;
            if (alignTo8Bytes)
            {
                int remainder = newLength & 0x07;
                if (remainder != 0)
                {
                    newLength += 8 - remainder;
                }
            }

            if (newLength == bs.Length)
            {
                return bs;
            }

            byte[] result = new byte[newLength];
            if (newLength < bs.Length)
            {
                Buffer.BlockCopy(bs, bs.Length - newLength, result, 0, newLength);
            }
            else
    {
                Buffer.BlockCopy(bs, 0, result, newLength - bs.Length, bs.Length);
            }
            return result;
        }

    }
 #endif