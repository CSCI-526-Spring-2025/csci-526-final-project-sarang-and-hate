using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseManager : MonoBehaviour {
    private string url = "https://csci526playertrack-default-rtdb.firebaseio.com/";
    private string secretKey = "HBPq0z8oc1uettksEKQIhd1V50VlVCBL9IK2eJVX";
    
    private void Start() {
        StartCoroutine(CollectAndUploadData());
    }

    private IEnumerator CollectAndUploadData() {
        // 1. Retrieve or input player name
        string userName = PlayerPrefs.GetString("PlayerName", "");
        if (string.IsNullOrEmpty(userName)) {
            while (true) {
                Debug.Log("Please enter your username: ");
                userName = Console.ReadLine();
                if (!string.IsNullOrEmpty(userName.Trim())) {
                    PlayerPrefs.SetString("PlayerName", userName);
                    PlayerPrefs.Save();
                    break;
                } else {
                    Debug.LogError("Invalid username. Please try again.");
                }
            }
        }
        
        Debug.Log($"Current player: {userName}");

        // 2. Get the number of times the player has played
        int totalPlays = 0;
        while (true) {
            Debug.Log("Enter the number of times you have played (numeric value): ");
            string countTimesString = Console.ReadLine().Trim();
            if (int.TryParse(countTimesString, out totalPlays)) {
                break;
            } else {
                Debug.LogError("Please enter a valid number!");
            }
        }

        // 3. Record the results of each game session
        List<GameRecord> records = new List<GameRecord>();

        for (int i = 0; i < totalPlays; i++) {
            Debug.Log($"Enter the result for game {i + 1}:");

            bool isWin;
            while (true) {
                Debug.Log("Did you win? (yes/no): ");
                string winOrLoss = Console.ReadLine().Trim().ToLower();
                if (winOrLoss == "yes" || winOrLoss == "no") {
                    isWin = winOrLoss == "yes";
                    break;
                } else {
                    Debug.LogError("Please enter 'yes' or 'no'.");
                }
            }

            string timeOrReason = "";
            if (isWin) {
                while (true) {
                    Debug.Log("Enter the completion time (in seconds): ");
                    timeOrReason = Console.ReadLine().Trim();
                    if (!string.IsNullOrEmpty(timeOrReason)) {
                        break;
                    } else {
                        Debug.LogError("Please enter a valid time!");
                    }
                }
            } else {
                while (true) {
                    Debug.Log("Enter the reason for failure: ");
                    timeOrReason = Console.ReadLine().Trim();
                    if (!string.IsNullOrEmpty(timeOrReason)) {
                        break;
                    } else {
                        Debug.LogError("Please enter a valid reason!");
                    }
                }
            }

            records.Add(new GameRecord(isWin, timeOrReason));
        }

        // 4. Prepare JSON data
        PlayerData playerData = new PlayerData(userName, totalPlays, records);
        string jsonData = JsonUtility.ToJson(playerData);
        
        // 5. Upload data to Firebase
        string endpoint = $"players/{userName}";
        yield return StartCoroutine(PostRequest(endpoint, jsonData, (success) => {
            if (success) {
                Debug.Log("✅ Data uploaded successfully!");
            } else {
                Debug.LogError("❌ Data upload failed!");
            }
        }));
    }

    // HTTP request to upload data
    public IEnumerator PostRequest(string endpoint, string jsonData, Action<bool> callback) {
        using (UnityWebRequest request = new UnityWebRequest($"{url}{endpoint}.json?auth={secretKey}", "PUT")) {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                callback(true);
                Debug.Log($"Data received: {request.downloadHandler.text}");
            } else {
                callback(false);
                Debug.LogError($"Error while sending POST request: {request.error}");
            }
        }
    }
}

// **Data Structures**
[Serializable]
public class PlayerData {
    public string userName;
    public int totalPlays;
    public List<GameRecord> records;

    public PlayerData(string userName, int totalPlays, List<GameRecord> records) {
        this.userName = userName;
        this.totalPlays = totalPlays;
        this.records = records;
    }
}

[Serializable]
public class GameRecord {
    public bool win;
    public string timeOrReason;

    public GameRecord(bool win, string timeOrReason) {
        this.win = win;
        this.timeOrReason = timeOrReason;
    }
}