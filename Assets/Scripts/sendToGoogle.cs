using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using System.Threading;
using Unity.VisualScripting;
using System.Timers;

public class sendToGoogle : MonoBehaviour
{
    // https://docs.google.com/forms/u/0/d/e/1FAIpQLSdflbbOIr8iLPxbDRSnuAtn4aubpkchM-q4bEKG5lo5c_sVnA/formResponse
    [SerializeField] private string URL;

    private long _sessionID;
    public string _map;
    public int _mapViewedAttempts;
    public string _wallRotation;
    public int _powerUps;
    public int _trapTiles;
    public int _magicTiles;
    public string _deadlocked;
    public string _timeUp;
    public string _levelCompleted;
    public string _currentLevel;

    private void Awake()
    {
        _sessionID = DateTime.Now.Ticks;
    }

    public void Send()
    {
        _map = PlayerController.hasPlayerOpenedMap ? "Yes" : "No";
        _mapViewedAttempts = PlayerController.mapViewedNum;
        _wallRotation = PlayerController.hasPlayerRotatedWalls ? "Yes" : "No";
        _powerUps = PlayerController.playerUsedPowerups;
        _trapTiles = GridManager.playerTrapped;
        _magicTiles = GridManager.playerMagicallyMoved;
        _deadlocked = GameTimer.isPlayerStuck ? "Yes" : "No";
        _timeUp = GameTimer.didPlayerRanOutOfTime ? "Yes" : "No";
        _levelCompleted = (GameTimer.isPlayerStuck == false && GameTimer.didPlayerRanOutOfTime == false) ? "Yes" : "No";
        _currentLevel = GameTimer.currentLevelPlayed;

        StartCoroutine(Post(_sessionID.ToString(), _mapViewedAttempts.ToString(), _wallRotation.ToString(), _powerUps.ToString(), _trapTiles.ToString(),
            _magicTiles.ToString(), _deadlocked.ToString(), _timeUp.ToString(), _levelCompleted.ToString(), _currentLevel.ToString()));
    }

    private IEnumerator Post(string sessionID, string mapViewedNum, string hasPlayerRotatedWalls, string powerUpsUsed, string trapTile,
        string magicTile, string deadLocked, string timesUp, string levelComplete, string levelName)
    {
        // Create the form and enter responses
        WWWForm form = new WWWForm();
        form.AddField("entry.1426993428", sessionID);
        form.AddField("entry.393964918", mapViewedNum);
        form.AddField("entry.1997040888", hasPlayerRotatedWalls);
        form.AddField("entry.2019652008", powerUpsUsed);
        form.AddField("entry.356531011", trapTile);
        form.AddField("entry.808463466", magicTile);
        form.AddField("entry.1926433817", deadLocked);
        form.AddField("entry.264644573", timesUp);
        form.AddField("entry.1597035899", levelComplete);
        form.AddField("entry.318650544", levelName);


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
