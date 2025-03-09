using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;

public class RecordStatistics: MonoBehaviour {
    
    // the url of your firebase
    private string url = "https://csci526playertrack-default-rtdb.firebaseio.com/";

    // set up a specific firebase with a variable and a function themselves
    private static RecordStatistics _instance;
    public static RecordStatistics Instance 
    {
        get 
        {
            // what happen if the instance does not exist
            if (_instance == null) {
                // step 1: create a new game object
                GameObject go = new GameObject("RecordStatistics");

                // step 2: add RecordStatistics component to the game object above
                _instance = go.AddComponent<RecordStatistics>();

                // step 3: tell Unity not to destroy it when loading a new scene
                DontDestroyOnLoad(go);
            }

            // step 4: loading new scenes
            return _instance;
        }
    }

    // What happen if you want to destroy the singleton above
    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        }
        else {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // (2) POST METHOD

    private string secretKey = "HBPq0z8oc1uettksEKQIhd1V50VlVCBL9IK2eJVX";
    
    public IEnumerator PostRequest(string endpoint, string jsonData, Action<bool> callback) {
       // step 1: call post request in UnityWebRequest above with the url, endpoint, and an authentication token(optional)
       using(UnityWebRequest request = new UnityWebRequest($"{url}{endpoint}.json?auth={secretKey}", "POST")) {
            // step 2: set up the request headers
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.disposeUploadHandlerOnDispose = true;
            request.disposeDownloadHandlerOnDispose = true;
            request.SetRequestHeader("Content-Type", "application/json");

            // step 3: send the request asynchronously and wait for the response
            yield return request.SendWebRequest();

            // step 4: Similar to what I have mentioned above, a total of 2 scenarios are required to take into account
            if (request.result == UnityWebRequest.Result.Success) {
                callback(true);
                Debug.Log($"Data received: {request.downloadHandler.text}");
            }
            else {
                callback(false);
                Debug.Log($"Error while Sending POST request: {request.error}");
            }
       }
    }
}

