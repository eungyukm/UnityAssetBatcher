using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

public class LoginWebRequest : MonoBehaviour
{
    private string host = "https://obliy.azurewebsites.net/";

    private string loginUri = "api/account/login_result";
    
    public void LoginAction(string loginMemberName, string loginPassword, Action OnLogin)
    {
        Debug.LogFormat("{0} {1}",loginMemberName, loginPassword);
        var info = new member_req(loginMemberName, loginPassword);
        StartCoroutine(this.LoginRoutine(loginUri, info, OnLogin));
    }
    
    private IEnumerator LoginRoutine(string uri, member_req info, Action OnLogin)
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
                Debug.Log("Login");
                OnLogin?.Invoke();
            }
        }
    }
    
    public class member_req
    {
        public string user_id;
        public string user_pw;
        public member_req(string user_id, string user_pw)
        {
            this.user_id = user_id;
            this.user_pw = user_pw;
        }
    }
}
