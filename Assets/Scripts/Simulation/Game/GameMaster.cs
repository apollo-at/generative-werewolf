using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Newtonsoft.Json;
using System.Linq;
using System.Data;
using TMPro;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.Reflection;

public class GameMaster : MonoBehaviour
{
    public Experiment experiment;
    public Game game;
    public GameTime time = new();
    public bool is_paused = false;

    public bool generative_rating = true;

    public Prompts prompts;
    public List<string> player_names;

    public TextArea text_area;
    public GameControls controls;
    public GameObject menu;

    public List<Vote> votes = new();
    public Observation react = null;
    public AgentInfo poisened = null;
    public bool saved = false;

    public int context_length = 2048;
    public double token_factor = 2.5f;

    int round_limit = 0;

    // Start is called before the first frame update
    void Start()
    {
        experiment = new();

        text_area = FindObjectOfType<TextArea>();
        menu = GameObject.Find("Canvas").transform.Find("MenuModal").gameObject;
        controls = FindObjectOfType<GameControls>();
        controls.Pause();

        //read prompts
        string config_file_path = Application.streamingAssetsPath + "/GameMaster/prompts.json";
        string config = File.ReadAllText(config_file_path);
        prompts = JsonConvert.DeserializeObject<PromptsFile>(config).prompts; // get prompts from prompts config

        string players_file_path = Application.streamingAssetsPath + "/Configs/players.json";
        string players_config = File.ReadAllText(players_file_path);
        player_names = JsonConvert.DeserializeObject<PlayerConfigFile>(players_config).player_names;
        token_factor = JsonConvert.DeserializeObject<PlayerConfigFile>(players_config).token_factor;
        context_length = JsonConvert.DeserializeObject<PlayerConfigFile>(players_config).context_length;

        StartCoroutine(SimulateGame());
    }

    /**
     * Simulate a round of Werewolf
     */
    IEnumerator SimulateGame()
    {
        InitGame(game != null ? game.data.game_id + 1 : 1);

        while (game.agents.Count < ExperimentSettings.numberOfAgents)
        {
            yield return null;
        }

        yield return StartCoroutine(Sleep(5));
        yield return StartCoroutine(WaitForUnpause());

        StartGame();

        //alternate between night and day routine
        while (!CheckGameOver())
        {
            if (time.day > round_limit)
            {
                game.data.winner = "Round Limit";
                break;
            }

            yield return StartCoroutine(SimulateNight()); 
            if (CheckGameOver())
            {
                break;
            }

            yield return StartCoroutine(AnalyzeNight());
            if (CheckGameOver())
            {
                break;
            }

            yield return StartCoroutine(SimulateDay()); 
            if (CheckGameOver())
            {
                break;
            }

            yield return StartCoroutine(AnalyzeDay());

        }

        FinishGame();
    }

    void InitGame(int game_id)
    {
        game = new Game(this, game_id);
    }

    void StartGame()
    {
        round_limit = game.GetGoodAgents().Count + game.GetWitches().Count - 1;

        text_area.ResetContent();
        time.ResetTime();
        Print("Welcome to Generative Werewolf!");
        game.LogEvent($"Game #{game.data.game_id} starts!");
    }

    IEnumerator SimulateDay()
    {
        time.PassTime();
        yield return StartCoroutine(WaitForUnpause());

        //plan
        yield return StartCoroutine(SimulatePlanning());

        Agent current_player = game.GetRandomAgent(); //start discussion with random player

        //TODO decide on how long discussion goes
        for (int i = 0; i < Math.Round(game.agents.Count * 1.5f) || react != null; i++) {
            Agent next_player = null;
            yield return StartCoroutine(current_player.Discussion(result => { next_player = result; }));

            if(next_player != null)
            {
                current_player = next_player;
            } 
            else
            {
                Agent random = current_player;
                while(current_player == random)
                {
                    random = game.GetRandomAgent();
                }
                current_player = random;
                react = null;
            }
        }
         
        //reflect on player roles for vote
        yield return StartCoroutine(SimulateReflectionOnPlayers());

        Print("It is dawn. Please vote now for one player to be eliminated!");

        yield return StartCoroutine(VoteDay()); 
        if (game.data.winner != null)
        {
            yield break;
        }

        Print("The vote has ended!");

        game.SaveGame();
    }
    IEnumerator AnalyzeNight()
    {
        Print("The village awakens!");
        AgentInfo winner = null;
        yield return StartCoroutine(CountVotes(result => { (winner, _) = result; }));


        if(winner == null && poisened == null)
        {
            Print($"This night no player was eliminated!");
        }

        //if distinct winner is found -> kill
        if (winner != null)
        {
            game.agents[winner.name].Kill();
            Print($"This night {winner.name} ({winner.role.ToString().FirstCharacterToUpper()}) was eliminated by the werewolves!");

            yield return StartCoroutine(BroadcastObservationToAgents(game.GetAgents(), $"{winner.name} is a {winner.role.ToString().FirstCharacterToUpper()}.", 10));
            yield return StartCoroutine(BroadcastObservationToAgents(game.GetAgents(), $"{winner.name} has been eliminated by the werewolves!", 10));
        }

        yield return StartCoroutine(AnalyzeWitch());

        game.SaveGame();
    }
    IEnumerator AnalyzeDay()
    {
        AgentInfo winner = null;
        string vote = null;
        yield return StartCoroutine(CountVotes(result => { (winner, vote) = result; }));
        

        //if distinct winner is found -> kill
        if (winner != null)
        {
            game.agents[winner.name].Kill();
            Print($"The village voted for {winner.name} ({winner.role.ToString().FirstCharacterToUpper()}) to be eliminated!");

            yield return StartCoroutine(BroadcastObservationToAgents(game.GetAgents(), $"{winner.name} is a {winner.role.ToString().FirstCharacterToUpper()}.", 10));
            yield return StartCoroutine(BroadcastObservationToAgents(game.GetAgents(), $"The village voted for {winner.name} to be eliminated! {vote}", 10));
        }
        else
        {
            Print($"Today no player was eliminated!");
        }

        game.SaveGame();
    }

    IEnumerator SimulateNight()
    {
        time.PassTime();
        yield return StartCoroutine(WaitForUnpause());

        Print("The village falls asleep!");

        Print("The werewolves wake up and choose their target!");
        foreach (var werewolf in game.GetWerewolves())
        {
            yield return StartCoroutine(werewolf.SimulateNight()); 
            if (game.data.winner != null)
            {
                yield break;
            }
        }
        Print("The werewolves fall asleep again!");

        if(game.GetWitches().Count > 0)
        {
            Print("The witch wakes up");
            foreach (var witch in game.GetWitches())
            {
                yield return StartCoroutine(witch.SimulateNight()); 
                if (game.data.winner != null)
                {
                    yield break;
                }
            }
            Print("The witch falls asleep again!");
        }

        if (game.GetSeers().Count > 0)
        {
            Print("The seer wakes up");
            foreach (var seer in game.GetSeers())
            {
                yield return StartCoroutine(seer.SimulateNight()); 
                if (game.data.winner != null)
                {
                    yield break;
                }
            }
            Print("The seer falls asleep again!");
        }

        game.SaveGame();
    }

    IEnumerator VoteDay()
    {
        yield return StartCoroutine(WaitForUnpause());
        foreach (var agent in game.GetAgents())
        {
            yield return StartCoroutine(agent.VoteDay());
            if(game.data.winner != null)
            {
                yield break;
            }
        }
    }

    IEnumerator SimulateReflection()
    {
        yield return StartCoroutine(WaitForUnpause());
        if (ExperimentSettings.reflection)
        {
            foreach (var agent in game.GetAgents())
            {
                yield return StartCoroutine(agent.Reflect());
            }
        }
    }
    IEnumerator SimulateReflectionOnPlayers()
    {
        yield return StartCoroutine(WaitForUnpause());
        if (ExperimentSettings.reflection)
        {
            foreach (var agent in game.GetAgents())
            {
                yield return StartCoroutine(agent.ReflectOnPlayers());
            }
        }
    }
    IEnumerator SimulatePlanning()
    {
        yield return StartCoroutine(WaitForUnpause());
        if (time.day > 1)
        {
            yield return StartCoroutine(SimulateReflection());
        }
        if (ExperimentSettings.planning)
        {
            foreach (var agent in game.GetAgents())
            {
                yield return StartCoroutine(agent.Plan());
            }
        }
    }

    public void FinishGame()
    {
        game.SaveGame();

        if (game.data.game_id < ExperimentSettings.numberOfRounds)
        {
            StartCoroutine(SimulateGame());
        } else
        {
            PauseGame();
        }
    }

    bool CheckGameOver()
    {
        if(game.data.winner != null)
        {
            return true;
        }
        List<Werewolf> werewolves = game.agents.Values.OfType<Werewolf>().ToList();
        if (werewolves.Count == 0)
        {
            Print("All werewolves were eliminated! The village wins!");
            game.LogEvent("Game is over! The village wins!");
            game.data.winner = "Village";
            return true;
        }

        if (game.GetAgents().Count - werewolves.Count == werewolves.Count)
        {
            Print("There are too few villagers left! The werewolves win!");
            game.LogEvent("Game is over! The werewolves win!");
            game.data.winner = "Werewolves";
            return true;
        }
        return false;
    }

    public void PauseGame()
    {
        is_paused = true;
        controls.Pause();
    }

    public void UnpauseGame()
    {
        is_paused = false;
        controls.Play();
    }

    public IEnumerator WaitForUnpause()
    {
        yield return StartCoroutine(Sleep(2));
        while (is_paused)
        {
            // Wait for one frame
            yield return null;
        }

    }

    public IEnumerator Sleep(int seconds)
    {
        while (is_paused)
        {
            // Wait for one frame
            yield return new WaitForSeconds(seconds);
        }

    }

    public IEnumerator CountVotes(Action<(AgentInfo, string)> onComplete)
    {
        string vote_string = PrintVotes();
        //group votes
        IEnumerable<IGrouping<AgentInfo, Vote>> grouped_votes = votes
             .GroupBy(vote => vote.vote)
             .OrderByDescending(group => group.Count());

        AgentInfo winner = grouped_votes.First().Key;

        if(saved == true)
        {
            game.LogPoll($"{winner.name} has been saved by the witch!", votes);
            Print($"This night {winner.name} was saved by the witch!");
            votes.Clear(); //reset votes 

            yield return StartCoroutine(BroadcastObservationToAgents(game.GetAgents(), $"{winner.name} has been saved by the witch!", 10));
            onComplete((null,null));
        }

        game.LogPoll($"{winner.name} has been eliminated!", votes);
        votes.Clear(); //reset votes 
        onComplete((winner, vote_string));
    }

    public IEnumerator AnalyzeWitch()
    {
        if(poisened != null)
        {
            game.LogPoll($"{poisened.name} has been poisoned by the witch!", votes);
            Print($"This night {poisened.name} has been poisoned by the witch");
            game.agents[poisened.name].Kill();

            yield return StartCoroutine(BroadcastObservationToAgents(game.GetAgents(), $"{poisened.name} is a {poisened.role}).", 10));
            yield return StartCoroutine(BroadcastObservationToAgents(game.GetAgents(), $"{poisened.name} has been poisoned by the witch!", 10));
            poisened = null;
        } 
        if (saved == true)
        {
            yield return StartCoroutine(BroadcastObservationToAgents(game.GetAgents(), $"The witch has saved the werewolves victim.", 8));
            saved = false;
        }
    }

    /// <summary>
    /// Write content to TextArea
    /// </summary>
    /// <param name="content"></param>
    /// <param name="game_flow"></param>
    public void Print(string content, bool game_flow = true)
    {
        text_area.WriteToArea(new GameMasterStatement(content, game_flow));
    }

    public IEnumerator BroadcastObservationToAgents(List<Agent> receivers, string message, int score = 0, AgentInfo agentinfo = null)
    {
        foreach (var agent in receivers)
        {
            yield return StartCoroutine(agent.SaveObservation(message, score, agentinfo));
        }
    }
    public string PrintVotes()
    {
        var content = "";
        foreach (var vote in votes)
        {
            content += $"{vote.voter.name} voted for {vote.vote.name}. ";
        }
        return content;
    }

    private void OnDestroy()
    {
        game.SaveGame();
        experiment.SaveJson();
    }
}
