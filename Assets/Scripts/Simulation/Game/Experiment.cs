
using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class Statistics
{
    public double win_rate_ww;
    public int wins_ww;
    public double win_rate_vl;
    public int wins_vl;
    public double avg_rounds_oa;
    public double avg_rounds_ww;
    public double avg_rounds_vl;
    public int mistakes;
    public double avg_mistakes;

}

public class ExperimentInfo
{
    public Guid experiment_id;
    public string experiment_name;
    public string model;
    public DateTime timestamp;
    public int numberOfAgents;
    public int numberOfRounds;
    public bool memory;
    public bool reflection;
    public bool planning;
    public Statistics stats;

    public ExperimentInfo()
    {
        experiment_id = Guid.NewGuid();
        experiment_name = ExperimentSettings.name;
        timestamp = DateTime.Now;
        numberOfAgents = ExperimentSettings.numberOfAgents;
        numberOfRounds = ExperimentSettings.numberOfRounds;
        memory = ExperimentSettings.memory;
        reflection = ExperimentSettings.reflection;
        planning = ExperimentSettings.planning;

    }
}
public class Experiment
{
    public string experiment_folder;
    public string experiment_file;
    public ExperimentInfo data = new();

    public Experiment()
    {
        experiment_folder = Application.persistentDataPath + "/Experiment-" + data.experiment_id;
        experiment_file = $"{experiment_folder}/experiment_settings.json";
        Directory.CreateDirectory(experiment_folder);
        SaveJson();
    }

    public void SaveJson()
    {
        string json = JsonConvert.SerializeObject(data);
        File.WriteAllText(experiment_file, json);
    }
}
