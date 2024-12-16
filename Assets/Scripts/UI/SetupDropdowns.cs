using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class SetupDropdowns : MonoBehaviour
{
    TMP_Dropdown number_of_rounds_dropdown;
    TMP_Dropdown number_of_agents_dropdown;
    TMP_InputField experiment_name;

    private void Start()
    {

        string players_file_path = Application.streamingAssetsPath + "/Configs/players.json";
        string players_config = File.ReadAllText(players_file_path);



        number_of_rounds_dropdown = transform.Find("NumberOfRounds").Find("Dropdown").GetComponent<TMP_Dropdown>();
        number_of_agents_dropdown = transform.Find("NumberOfAgents").Find("Dropdown").GetComponent<TMP_Dropdown>();
        experiment_name = transform.Find("Name").GetComponentInChildren<TMP_InputField>();

        //TODO init Model options based on models in StreamingAssets folder
        List<string> agents = JsonConvert.DeserializeObject<PlayerConfigFile>(players_config).player_configs.Keys.ToList();
        List<string> rounds = new List<string>(Enumerable.Range(1, 150).Select(x => x.ToString()));

        number_of_agents_dropdown.ClearOptions();
        number_of_rounds_dropdown.ClearOptions();
        number_of_agents_dropdown.AddOptions(agents);
        number_of_rounds_dropdown.AddOptions(rounds);


        number_of_agents_dropdown.value = agents.IndexOf(ExperimentSettings.numberOfAgents.ToString());
        number_of_rounds_dropdown.value = rounds.IndexOf(ExperimentSettings.numberOfRounds.ToString());
    }

    private void Update()
    {
        ExperimentSettings.name = experiment_name.text;
    }

    public void SetNumberOfRounds()
    {
        ExperimentSettings.numberOfRounds = int.Parse(number_of_rounds_dropdown.captionText.text);
    }
    public void SetNumberOfAgents()
    {
        ExperimentSettings.numberOfAgents = int.Parse(number_of_agents_dropdown.captionText.text);
    }
}
