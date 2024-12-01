using UnityEngine;
using IoT;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System;
using Unity.VisualScripting;
public class Sensors : MonoBehaviour
{
    public float timeSpan = 5;
    private float time = 0;
    public float temp = 24;
    public float lux = 200;
    public float db = 70;

    public string ReadFileWithEncoding(string filePath, Encoding encoding)
    {
        try
        {
            using (StreamReader reader = new StreamReader(filePath, encoding))
            {
                string content = reader.ReadToEnd();                
                return content;
            }
        }
        catch (Exception ex)
        {            
            return string.Empty;            
        }
    }

    public async void Start()
    {
        await CreateAE();
    }

    public async Task<string> CreateAE()
    {
        string requestBody = @"{
            ""m2m:ae"": {
                ""rn"": """ + "Sensor" + @""",
                ""api"": ""N_Sensor_AE"",
                ""rr"": true,
                ""srv"": [""3""]
            }
        }";
        string res = await OneM2M.PostDataAsync("Sensors", 2, requestBody, "osori");
        return res;
    }

    public async Task<string> CreateContentInstance(string containerName, string content)
    {
        string requestBody = @"{
            ""m2m:cin"": {
                ""con"": """ + content + @"""
            }
        }";        
        string res = await OneM2M.PostDataAsync(containerName, 4, requestBody, "osori");
        return res;
    }

}
