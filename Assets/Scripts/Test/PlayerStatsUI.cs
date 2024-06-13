using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerStatsUI : MonoBehaviour
{
    public GameObject playerStatPrefab; // Assign the PlayerStatEntry prefab in the Inspector
    public Transform contentPanel; // Assign the Content panel in the Inspector

    [System.Serializable]
    public struct PlayerStat
    {
        public string playerName;
        public int kills;
        public int deaths;
    }

    private List<PlayerStat> playerStats = new List<PlayerStat>
    {
        new PlayerStat { playerName = "Player1" },
        new PlayerStat { playerName = "Player2" },
        new PlayerStat { playerName = "Player3" },
        new PlayerStat { playerName = "Player4" },
        new PlayerStat { playerName = "Player5" }
    };

    void Start()
    {
        PopulatePlayerStats();
    }

    void PopulatePlayerStats()
    {
        System.Random rand = new System.Random();
        for (int i = 0; i < playerStats.Count; i++)
        {
            PlayerStat stat = playerStats[i];
            stat.kills = rand.Next(0, 100); // Random kills between 0 and 100
            stat.deaths = rand.Next(0, 100); // Random deaths between 0 and 100
            playerStats[i] = stat; // Update the list with the modified struct

            GameObject newEntry = Instantiate(playerStatPrefab, contentPanel);
            Text statText = newEntry.GetComponent<Text>();
            statText.text = $"{stat.playerName}: Kills: {stat.kills}, Deaths: {stat.deaths}";
        }
    }
}
