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

    private List<PlayerStat> playerStats = new List<PlayerStat>();

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Fetch data from PlayerStatsManager and populate playerStats list
        foreach (var kvp in PlayerStatsManager.kills)
        {
            string playerName = kvp.Key;
            int kills = kvp.Value;
            int deaths = PlayerStatsManager.deaths.ContainsKey(playerName) ? PlayerStatsManager.deaths[playerName] : 0;

            playerStats.Add(new PlayerStat
            {
                playerName = playerName,
                kills = kills,
                deaths = deaths
            });
        }

        PopulatePlayerStats();
    }

    void PopulatePlayerStats()
    {
        foreach (PlayerStat stat in playerStats)
        {
            GameObject newEntry = Instantiate(playerStatPrefab, contentPanel);
            Text statText = newEntry.GetComponent<Text>();
            statText.text = $"{stat.playerName}: Kills: {stat.kills}, Deaths: {stat.deaths}";
        }
    }
}
