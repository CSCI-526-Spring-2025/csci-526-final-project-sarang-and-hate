using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;

public class sendToGoogle : MonoBehaviour
{
    // https://docs.google.com/forms/u/0/d/e/1FAIpQLSdflbbOIr8iLPxbDRSnuAtn4aubpkchM-q4bEKG5lo5c_sVnA/formResponse
    [SerializeField] private string URL;

    private long _sessionID;
    public bool _map;
    public bool _wallRotation;

    private void Awake()
    {
        _sessionID = DateTime.Now.Ticks;
    }

    public void Send()
    {
        _map = PlayerController.hasPlayerOpenedMap;
        _wallRotation = PlayerController.hasPlayerRotatedWalls;

        StartCoroutine(Post(_sessionID.ToString(), _map.ToString(), _wallRotation.ToString()));
    }

    private IEnumerator Post(string sessionID, string hasPlayerOpenedMap, string hasPlayerRotatedWalls)
    {
        // Create the form and enter responses
        WWWForm form = new WWWForm();
        form.AddField("entry.1426993428", sessionID);
        form.AddField("entry.393964918", hasPlayerOpenedMap);
        form.AddField("entry.1997040888", hasPlayerRotatedWalls);

        // Send responses and verify result
        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }
    }

}
