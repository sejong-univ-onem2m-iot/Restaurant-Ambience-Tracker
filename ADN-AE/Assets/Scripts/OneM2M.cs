using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;
using System.Threading.Tasks;

namespace IoT
{
    public class OneM2M
    {
        private static string url = "https://192.168.0.8:3000";
        
        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {                 
            return true;
        }

        private static void ConfigureHttps()
        {            
            ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;            
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        public static async Task<string> PostDataAsync(string origin, int type, string body, string token = "")
        {
            ConfigureHttps();
            string endpoint = $"{url}/cse-in";
            Debug.Log(endpoint);
            System.Random rand = new System.Random();
            byte[] bodyRaw = Encoding.UTF8.GetBytes(body);

            using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", $"application/json;ty={type}");
                request.SetRequestHeader("X-M2M-Origin", origin);
                request.SetRequestHeader("X-M2M-RI", rand.Next().ToString());
                request.SetRequestHeader("X-M2M-RVI", "3");
                //http basic auth
                if (token != "")
                {                                       
                    request.SetRequestHeader("Authorization", $"Bearer {token}");
                }                
                request.certificateHandler = new BypassCertificateHandler();
                try
                {
                    await request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        string jsonResponse = request.downloadHandler.text;
                        Debug.Log($"Server response: {jsonResponse}");
                        return jsonResponse;
                    }
                    else
                    {
                        Debug.LogError($"Error: {request.error}");
                        Debug.LogError($"Response code: {request.responseCode}");
                        throw new Exception($"Request failed: {request.error}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Exception during web request: {e.Message}");
                    throw;
                }
            }
        }

        public static IEnumerator GetData(string origin)
        {
            ConfigureHttps();

            string endpoint = $"{url}/users";
            using (UnityWebRequest request = new UnityWebRequest(endpoint, "GET"))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                System.Random rand = new System.Random();
                request.SetRequestHeader("Accept", "application/json");
                request.SetRequestHeader("X-M2M-Origin", origin);
                request.SetRequestHeader("X-M2M-RI", rand.Next().ToString());
                request.SetRequestHeader("X-M2M-RVI", "3");

                // Configure certificate handling
                request.certificateHandler = new BypassCertificateHandler();

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"Server response: {jsonResponse}");
                }
                else
                {
                    Debug.LogError($"Error: {request.error}");
                    Debug.LogError($"Response code: {request.responseCode}");
                }
            }
        }
    }

    // Custom certificate handler for development
    public class BypassCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {           
            return true;
        }
    }
}