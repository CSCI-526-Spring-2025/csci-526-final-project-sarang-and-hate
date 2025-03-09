using System.Collections.Generic;

[System.Serializable]
public class PlayerData {
    public string playerName; // the name of the player
    public int count; // how many times the player has played
    public List<GameStatus> scenarios; // the specific information of the list of scenarios they have already played

    public PlayerData(string playerName, int count, List<GameStatus> scenarios) {
        this.playerName = playerName;
        this.count = count;
        this.scenarios = scenarios;
    }
}

[System.Serializable]
public class GameStatus {
    public bool isWin; // check whether the player has won
    public string reason; // If the player has lost, tell me why it is lost
    public string time; // If the player wins the game, you may tell me the exact time the player has won the game

    public GameStatus(bool isWin, string reasonOrTime) {
        this.isWin = isWin;

        if (isWin) {
            this.time = reasonOrTime;
        }
        else {
            this.reason = reasonOrTime;
        }

    }
}