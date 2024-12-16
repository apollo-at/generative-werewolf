using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class GameData
{
    public int game_id;
    public string winner;
    public DateTime time_stamp = DateTime.Now;
    public List<Log> game_log = new();
    public GameTime end_time;
}
public class Game
{
    public GameMaster gm;
    public string game_folder;
    public GameData data = new();
    public string file_path;
    public Dictionary<string, Agent> agents = new();
    public AgentPanel agent_panel;

    public Game(GameMaster gm, int game_id)
    {
        this.gm = gm;
        data.game_id = game_id;
        game_folder = gm.experiment.experiment_folder + "/Game-" + game_id;
        file_path = $"{game_folder}/game_log.json";
        Directory.CreateDirectory(game_folder);

        agent_panel = GameObject.Find("AgentPanel").GetComponent<AgentPanel>();
        agent_panel.SpawnUIAgents();
        SaveGame();
    }
    public void LogMistake(string response, Agent speaker, string stage)
    {
        data.game_log.Add(new Mistake(response, speaker.data.info, stage, gm.time.Copy()));
    }
    public void LogEvent(string action)
    {
        data.game_log.Add(new Event(action, gm.time.Copy()));
    }

    public void LogStatement(string statement, Agent speaker)
    {
        data.game_log.Add(new StatementLog(statement, speaker.data.info, gm.time.Copy()));
    }

    public void LogPoll(string result, List<Vote> votes)
    {
        List<Vote> votes_copy = new(votes);
        data.game_log.Add(new Poll(result, gm.time.Copy(), votes_copy));
    }
    /// <summary>
    /// Save GameData object in json file
    /// </summary>
    public void SaveGame()
    {
        data.end_time = gm.time.Copy();
        string json = JsonConvert.SerializeObject(data);
        File.WriteAllText(file_path, json);
    }

    public List<Werewolf> GetWerewolves()
    {
        return agents.Values.OfType<Werewolf>().ToList();
    }
    public List<Villager> GetVillagers()
    {
        return agents.Values.OfType<Villager>().ToList();
    }
    public List<Seer> GetSeers()
    {
        return agents.Values.OfType<Seer>().ToList();
    }
    public List<Witch> GetWitches()
    {
        return agents.Values.OfType<Witch>().ToList();
    }
    public List<Agent> GetGoodAgents()
    {
        List<Agent> villagers = GetVillagers().Cast<Agent>().ToList();
        List<Agent> seer = GetSeers().Cast<Agent>().ToList();
        List<Agent> witch = GetWitches().Cast<Agent>().ToList();
        return villagers.Concat(seer).Concat(witch).ToList();
    }
    public List<Agent> GetAgents()
    {
        return agents.Values.ToList();
    }

    public Agent GetRandomAgent()
    {
        return agents[agents.Keys.ToList()[UnityEngine.Random.Range(0, agents.Keys.ToList().Count)]];
    }
}
