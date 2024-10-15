using UnityEngine;
using System.Collections;
using System.Text;
using System.Net;
using System.IO;
using System;
using System.Collections.Generic;

public class WatsonAssistantRequest : MonoBehaviour
{
    public string apiKey = "bmzIfX4oG-anniBhZF-CJJV3S03bKIF7B3txfIYD9gBU";
    public string assistantUrl = "https://api.au-syd.assistant.watson.cloud.ibm.com/instances/47da9a59-9ce6-4805-9b74-57b6de692956/v2/assistants/2c1ab89c-be33-4777-9fc8-b8c4205ae9ae/message?version=2021-11-27";

    void Start()
    {
        string inputText = "Hello";
        string responseText = SendRequest(inputText);
        Debug.Log(responseText);
    }

    string SendRequest(string inputText)
    {
        string url = assistantUrl;
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("apikey:" + apiKey));
        request.ContentType = "application/json";
        request.Method = "POST";

        using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
        {
            string json = "{\"input\": {\"text\": \"" + inputText + "\"}}";
            streamWriter.Write(json);
        }

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
        {
            return ExtractText(streamReader.ReadToEnd());
        }
    }

    string ExtractText(string jsonResponse)
    {
        WatsonResponse response = JsonUtility.FromJson<WatsonResponse>(jsonResponse);
        if (response.output.generic != null && response.output.generic.Length > 0)
        {
            return response.output.generic[0].text;
        }
        return null;
    }

    [Serializable]
    public class WatsonResponse
    {
        public Output output;
    }

    [Serializable]
    public class Output
    {
        public Generic[] generic;
    }

    [Serializable]
    public class Generic
    {
        public string text;
    }
}
