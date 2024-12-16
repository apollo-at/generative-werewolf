using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using TMPro;
using UnityEngine;
using System.Reflection;

public class ExperimentModal : MonoBehaviour
{
    public delegate void OnDestroyActionHandler();
    public event OnDestroyActionHandler OnDestroyAction;

    public ExperimentInfo info;
    public Dictionary<GameData, List<AgentData>> games = new();
    public GameObject game_list;
    public GameObject experiment_info;
    public string path;

    /// <summary>
    /// Set initial values of ExperimentModal
    /// </summary>
    /// <param name="info"></param>
    /// <param name="games"></param>
    public void SetExperiment(ExperimentInfo info, Dictionary<GameData, List<AgentData>> games, string path)
    {
        this.info = info;
        this.games = games;
        game_list.transform.GetComponent<GameList>().SetGames(games);
        experiment_info.transform.GetComponent<ExperimentInfoPanel>().SetExperiment(info);
        TMP_Text title = transform.Find("Header").transform.Find("ModalTitle").transform.GetComponent<TMP_Text>();
        title.text = $"{info.experiment_id}";
        this.path = path;
        ComputeStatistics();
    }

    public void ComputeStatistics()
    {

        IEnumerable<GameData> validGames = games.Keys.Where(game => game.winner != "Prompt error" && game.winner != "Not finished" && game.winner != null);
        Debug.Log(validGames.Count());
        info.stats = new Statistics();

        //analyse mistakes
        info.stats.mistakes = validGames.Sum(game => game.game_log.OfType<Mistake>().Count());
        info.stats.avg_mistakes = (double) info.stats.mistakes / (double) validGames.Count();

        info.stats.avg_rounds_oa = validGames.Sum(game => game.end_time.CalculatePhases()) / validGames.Count();
        info.stats.avg_rounds_ww = validGames.Where(game => game.winner == "Werewolves").Average(game => game.end_time.CalculatePhases());
        info.stats.avg_rounds_vl = validGames.Where(game => game.winner == "Village").Average(game => game.end_time.CalculatePhases());

        info.stats.wins_ww = validGames.GroupBy(game => game.winner).ToDictionary(group => group.Key, group => group.Count())["Werewolves"];
        info.stats.wins_vl = validGames.GroupBy(game => game.winner).ToDictionary(group => group.Key, group => group.Count())["Village"];

        info.stats.win_rate_ww = (double)info.stats.wins_ww / (double)validGames.Count();
        info.stats.win_rate_vl = (double)info.stats.wins_vl / (double)validGames.Count();

        string stats_file = $"{path}/statistics.json";

        string json = JsonConvert.SerializeObject(info.stats);
        File.WriteAllText(stats_file, json);
    }

    /// <summary>
    /// Delete an experiment folder
    /// </summary>
    public void DeleteExperiment()
    {
        if (info.experiment_id == Guid.Empty)
        {
            Debug.LogWarning("No Guid!");
            return;
        }
        string saveFolder_path = Application.persistentDataPath;
        string experiment_path = Path.Combine(saveFolder_path, $"Experiment-{info.experiment_id}");
        if (Directory.Exists(experiment_path))
        {
            Directory.Delete(experiment_path, true);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("Directory not found!");
        }
    }

    void OnDestroy()
    {
        OnDestroyAction?.Invoke();
    }
}
