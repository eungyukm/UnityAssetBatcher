using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using UnityEngine.Events;

public class MapWebRequest : MonoBehaviour
{
    private string host = "https://obliy.azurewebsites.net/";
    // private string host = "http://127.0.0.1:8000/";

    private string mapCreateUri = "api/map_create";

    private void Start()
    {
        
    }


    public void MapCreate(string subject, string text, int writer_idx, int info_idx, UnityAction<int> OnCreateMap)
    {
        var mapData = new MapDataReq(subject, text, writer_idx, info_idx);
        StartCoroutine(MapCreateRoutine(mapCreateUri, mapData, OnCreateMap));
    }
    

    private IEnumerator MapCreateRoutine(string uri, MapDataReq info, UnityAction<int> OnCreateMap)
    {
        var json = JsonConvert.SerializeObject(info);
        var url = string.Format("{0}{1}", this.host, uri);
        var request = new UnityWebRequest(url, "POST");
        var bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        if(request.isNetworkError || request.isHttpError)
        {
            Debug.Log("www error");
        }
        else
        {
            Debug.LogFormat("request.responseCode: {0}", request.responseCode);
            Debug.LogFormat("data.responseCode : {0}", request.downloadHandler.text);
            string resCode = Regex.Replace(request.downloadHandler.text, "\"", "", RegexOptions.Singleline);
            int code = int.Parse(resCode);
            Debug.Log("code : " + code);
            if (code == 200)
            {
                Debug.Log("Map Create");
                OnCreateMap?.Invoke(code);
            }
            else
            {
                Debug.Log("Login Failed");
            }
        }
    }
    
    public class MapDataReq
    {
        public string map_data_subject;
        public string map_data_text;
        public int map_data_writer_idx;
        public int map_info_idx;

        public MapDataReq(string subject, string text, int writer_idx, int info_idx)
        {
            map_data_subject = subject;
            map_data_text = text;
            map_data_writer_idx = writer_idx;
            map_info_idx = info_idx;
        }
    }
}
