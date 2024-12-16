using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentList : MonoBehaviour
{
    public delegate void ReloadEventHandler();
    //public event ReloadEventHandler OnReload;

    public GameObject list_item;
    public GameObject experiment_modal;

    Dictionary<ExperimentInfo, Dictionary<GameData, List<AgentData>>> experiments = new();

    private void OnEnable()
    {
        Reload();
    }

    public void Reload()
    {
        string saveFolderPath = Application.persistentDataPath;
        string[] subDirectories = Directory.GetDirectories(saveFolderPath)
                                             .OrderByDescending(d => Directory.GetCreationTime(d))
                                             .ToArray();

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        foreach (string subDirPath in subDirectories)
        {
            string settings_file_path = Path.Combine(subDirPath, "experiment_settings.json");
            if (File.Exists(settings_file_path))
            {
                // Read the JSON file
                string json = File.ReadAllText(settings_file_path);
                ExperimentInfo info = JsonConvert.DeserializeObject<ExperimentInfo>(json);
                LoadGames(info, subDirPath);

                GameObject item = Instantiate(list_item, transform);
                item.transform.GetComponent<Button>().onClick.AddListener(() => OpenExperimentModal(info, subDirPath));

                TMP_Text experiment_id = item.transform.Find("ExperimentId").GetComponent<TMP_Text>();
                if(info.experiment_name != null && info.experiment_name.Length > 0)
                {
                    experiment_id.text = $"{info.experiment_name}";
                } else
                {
                    experiment_id.text = $"{info.experiment_id}";
                }
                TMP_Text model = item.transform.Find("Model").GetComponent<TMP_Text>();
                model.text = $"{info.model}";
                TMP_Text number_of_rounds = item.transform.Find("NumberOfRounds").GetComponent<TMP_Text>();
                number_of_rounds.text = $"{info.numberOfRounds} {(info.numberOfRounds == 1 ? "Round" : "Rounds")}";
                TMP_Text number_of_agents = item.transform.Find("NumberOfAgents").GetComponent<TMP_Text>();
                number_of_agents.text = $"{info.numberOfAgents} Agents";
                TMP_Text timestamp = item.transform.Find("Timestamp").GetComponent<TMP_Text>();
                timestamp.text = $"{info.timestamp:dd.MM.yyyy HH:mm}";

                Transform toggles = item.transform.Find("Toggles");
                toggles.Find("Memory").Find("Check").gameObject.SetActive(info.memory);
                toggles.Find("Reflection").Find("Check").gameObject.SetActive(info.reflection);
                toggles.Find("Planning").Find("Check").gameObject.SetActive(info.planning);
            }
            else
            {
                Debug.LogWarning("experiment_settings.json not found in directory: " + subDirPath);
            }
        }
    }

    void OpenExperimentModal(ExperimentInfo info, string path)
    {
        if (info == null)
        {
            Debug.LogWarning("No Experiment!");
            return;
        }
        GameObject new_modal = Instantiate(experiment_modal, FindObjectOfType<Canvas>().transform);
        new_modal.transform.Find("Frame").transform.GetComponent<ExperimentModal>().OnDestroyAction += Reload;
        new_modal.transform.Find("Frame").GetComponent<ExperimentModal>().SetExperiment(info, experiments[info], path);
    }

    void LoadGames(ExperimentInfo info, string path)
    {
        Dictionary<GameData, List<AgentData>> games = new();
        string saveFolderPath = Application.persistentDataPath;
        
        string[] game_directories = Directory.GetDirectories(path)
                                             .OrderByDescending(d => Directory.GetCreationTime(d))
                                             .ToArray();

        foreach (string game_directory_path in game_directories)
        {
            string game_data_path = Path.Combine(game_directory_path, "game_log.json");
            if (File.Exists(game_data_path))
            {
                // Read the JSON file
                string json = File.ReadAllText(game_data_path);
                GameData game_data = JsonConvert.DeserializeObject<GameData>(json, new LogConverter());

                string[] json_files = Directory.GetFiles(game_directory_path, "*.json");

                List<AgentData> agents = new();
                foreach (string json_file in json_files)
                {
                    string file_name = Path.GetFileName(json_file);
                    if (file_name == "game_log.json")
                    {
                        continue;
                    }
                    string agent_json = File.ReadAllText(json_file);
                    AgentData agent_data = JsonConvert.DeserializeObject<AgentData>(agent_json, new MemoryConverter());
                    agents.Add(agent_data);
                }

                games.Add(game_data, agents);
            }
            else
            {
                Debug.LogWarning("game_log.json not found in directory: " + game_directory_path);
            }
        }
        experiments.Add(info, games);
    }
}
