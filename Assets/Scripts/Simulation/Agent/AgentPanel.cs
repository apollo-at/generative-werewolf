using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class AgentPanel : MonoBehaviour
{
    public GameObject VillagerPrefab;
    public GameObject WerewolfPrefab;
    public GameObject SeerPrefab;
    public GameObject WitchPrefab;

    public GameMaster gm;
    private void Start()
    {
        gm = FindObjectOfType<GameMaster>();
    }

    public void SpawnUIAgents()
    {

        string config_file_path = Application.streamingAssetsPath + "/Configs/players.json";
        string config_content = System.IO.File.ReadAllText(config_file_path);
        PlayerConfig game_config = JsonConvert.DeserializeObject<PlayerConfigFile>(config_content).player_configs[ExperimentSettings.numberOfAgents.ToString()];



        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < ExperimentSettings.numberOfAgents; i++)
        {
            int random_idx = Random.Range(0, game_config.roles.Count);
            switch (game_config.roles[random_idx])
            {
                case Role.villager:
                    Instantiate(VillagerPrefab, transform);
                    break;
                case Role.werewolf:
                    Instantiate(WerewolfPrefab, transform);
                    break;
                case Role.seer:
                    Instantiate(SeerPrefab, transform);
                    break;
                case Role.witch:
                    Instantiate(WitchPrefab, transform);
                    break;
            }
            game_config.roles.RemoveAt(random_idx);

        }
    }    

    public void DestroyAgents()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
