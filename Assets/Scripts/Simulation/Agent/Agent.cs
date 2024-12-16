using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using OpenAI;
using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System.Reflection;
using UnityEngine.SocialPlatforms.Impl;


public abstract class Agent : MonoBehaviour
{
    const double MAX_RESPONSE_LENGTH = 384;

    public OpenAIApi openai = new("this mustn't be empty");
    public GameMaster gm;
    public TMP_Text text;
    protected string file_path;
    public AgentData data = new();
    public GameObject modal;

    bool active = false;

    private void Update()
    {
        if(active)
        {
            Pulsate();
        }
    }
    public IEnumerator Discussion(Action<Agent> onComplete)
    {
        yield return StartCoroutine(gm.WaitForUnpause());
        SetActive();

        //construct prompt
        string prompt = null;

        if (gm.react != null && gm.react.content != "")
        {
            prompt = string.Format(gm.prompts.react_to, gm.react.actor.name, gm.react.content);
            gm.react = null;
            onComplete(this);
        } 
        else
        {
            prompt = string.Format(gm.prompts.discussion);
        }

        string query_result = null;
        yield return StartCoroutine(PromptLlm(prompt, result => { query_result = result; }));

        Agent next_player =  Checkname(query_result);

        if (next_player != null)
        {
            onComplete(next_player);
            gm.react = new Observation(query_result, gm.time.Copy(), 0, data.info);
        } 
        else
        {
            onComplete(null);
        }

        yield return StartCoroutine(gm.BroadcastObservationToAgents(gm.game.GetAgents(), query_result, 0, data.info));

        PrintAndLog(query_result);

        SetUnactive();
    }

    public IEnumerator VoteDay()
    {
        yield return StartCoroutine(gm.WaitForUnpause());
        SetActive();

        string actions = string.Join(" ",gm.game.GetAgents().Select(a => $"I vote for {a.GetName()}."));

        string query_result = null;
        string prompt = string.Format(gm.prompts.vote_day, actions);
        yield return StartCoroutine(PromptLlm(prompt, result => { query_result = result; }));

        Agent winner = ExtractName(query_result);

        if (winner == this)
        {
            gm.game.LogMistake(query_result, this, "VoteDay");
            winner = null;
        }
        if(winner != null)
        {
            Print(query_result);
            gm.votes.Add(new Vote(winner.data.info, data.info));
            yield return StartCoroutine(gm.BroadcastObservationToAgents(gm.game.GetAgents(), query_result, 0, data.info));
        }

        SetUnactive();
    }

    public IEnumerator Reflect()
    {
        yield return StartCoroutine(gm.WaitForUnpause());
        SetActive();

        string query_result = null;
        string prompt = gm.prompts.reflection;
        yield return StartCoroutine(PromptLlm(prompt, result => { query_result = result; }, 512));

        yield return StartCoroutine(ProcessReflection(query_result));
        SetUnactive();
    }
    public IEnumerator Plan()
    {
        yield return StartCoroutine(gm.WaitForUnpause());
        SetActive();

        string query_result = null;
        string prompt = gm.prompts.planning;
        yield return StartCoroutine(PromptLlm(prompt, result => { query_result = result; }, 512));

        ProcessPlanning(query_result);
        SetUnactive();
    }

    public IEnumerator ReflectOnPlayers()
    {
        yield return StartCoroutine(gm.WaitForUnpause());
        SetActive();

        string players = string.Join(" ,", gm.game.GetAgents().Where(a => a != this).Select(a => a.GetName()));

        string query_result = null;
        string prompt = string.Format(gm.prompts.reflection_roles, players);
        yield return StartCoroutine(PromptLlm(prompt, result => { query_result = result; }, 256));

        yield return StartCoroutine(ProcessReflectionOnPlayers(query_result));
        SetUnactive();
    }

    public void SaveJSON()
    {
        string json = JsonConvert.SerializeObject(data);
        File.WriteAllText(file_path, json);
    }

    public void LoadJSON()
    {
        if (File.Exists(file_path))
        {
            string json = File.ReadAllText(file_path);
            JsonUtility.FromJsonOverwrite(json, data);
        }
    }

    public string ConstructPrompt(string prompt)
    {
        string werewolves_string = "";
        if(this is Werewolf werewolf && gm.game.GetWerewolves().Count > 1)
        {
            string werewolves = string.Join(", ",gm.game.GetWerewolves().Where(w => w != this).Select(w => w.GetName()));
            werewolves_string = string.Format(" You are a werewolf. Your team members are {0}. Don't eliminate them!. ", string.Join(", ", werewolves));
        }
        string other_players = string.Join(", ", GetOtherAgents().Select(a => a.GetName()));

        string temp_promp = string.Format(gm.prompts.game_rules, gm.game.GetVillagers().Count.ToString(), gm.game.GetWerewolves().Count.ToString(), gm.game.GetSeers().Count.ToString(), gm.game.GetWitches().Count.ToString(), GetName(), GetRole(), "", data.ConstructPlan(), gm.time.Print(), other_players, werewolves_string, prompt);

        double temp_token_count = Math.Round( temp_promp.Length / gm.token_factor);

        return string.Format(gm.prompts.game_rules, gm.game.GetVillagers().Count.ToString(), gm.game.GetWerewolves().Count.ToString(), gm.game.GetSeers().Count.ToString(), gm.game.GetWitches().Count.ToString(), GetName(), GetRole(), FilterMemories(temp_token_count), data.ConstructPlan(), gm.time.Print(), other_players, werewolves_string, prompt);
    }

    public void SetActive()
    {
        active = true;
    }

    public void SetUnactive()
    {
        transform.localScale = new Vector3(1f, 1f, 1f);
        SaveJSON();
        active = false;
    }
    public void Kill()
    {
        data.eliminated = true;
        gm.game.agents.Remove(data.info.name);
        SetEliminated();

        SaveJSON();
    }

    public void SetEliminated()
    {

        Image image = transform.Find("Image").GetComponent<Image>();
        image.color = Color.Lerp(Color.black, image.color, 0.5f);
        TMP_Text text = transform.Find("Name").GetComponent<TMP_Text>();
        text.color = Color.Lerp(Color.black, text.color, 0.5f);
    }

    public IEnumerator SaveObservation(string content, int score, AgentInfo actor = null)
    {
        if(!ExperimentSettings.memory)
        {
            yield break;
        }
        if(score == 0 && gm.generative_rating)
        {
            string actor_string = "";
            if(actor != null)
            {
                actor_string = $" #{actor.name}#";
            }
            string query_result = null;
            string prompt = string.Format(gm.prompts.get_score, content + actor_string);
            yield return StartCoroutine(PromptLlm(prompt, result => { query_result = result; })); 
            Match match = Regex.Match(query_result, @"\b(?:10|[0-9])\b");

            if (match.Success)
            {
                score = int.Parse(match.Value);
            }
        }
        data.memory_stream.Add(new Observation(content, gm.time.Copy(), score, actor));
        SaveJSON();
    }

    public IEnumerator SaveReflection(string content, Guid[] parents, int score = 0)
    {
        if (!ExperimentSettings.reflection)
        {
            yield break;
        }
        if (score == 0 && gm.generative_rating)
        {
            string query_result = null;
            string prompt = string.Format(gm.prompts.get_score, content);
            yield return StartCoroutine(PromptLlm(prompt, result => { query_result = result; }));
            Match match = Regex.Match(query_result, @"\b(?:10|[0-9])\b");

            if (match.Success)
            {
                score = int.Parse(match.Value);
            }
        }
        data.memory_stream.Add(new Reflection(content, gm.time.Copy(), parents, score));
        SaveJSON();
    }

    public void SavePlan(string content, string[] steps, int score = 0)
    {
        if (!ExperimentSettings.planning)
        {
            return;
        }
        data.memory_stream.Add(new Plan(content, gm.time.Copy(), steps, score));
        SaveJSON();
    }

    public void SaveExperience(string content, bool is_win, int score = 0)
    {
        data.memory_stream.Add(new Experience(content, gm.time.Copy(), is_win, score));
        SaveJSON();
    }

    /// <summary>
    /// Write content to TextArea
    /// </summary>
    /// <param name="content"></param>
    public void Print(string content)
    {
        gm.text_area.WriteToArea(new AgentStatement(content, this));
    }

    /// <summary>
    /// Write content to TextArea and save it to the game log
    /// </summary>
    /// <param name="content"></param>
    public void PrintAndLog(string content)
    {
        gm.text_area.WriteToArea(new AgentStatement(content, this));
        gm.game.LogStatement(content, this);
    }

    /// <summary>
    /// Init agent json file, name and id.
    /// </summary>
    /// <returns></returns>
    public IEnumerator InitAgent()
    {
        gm = FindObjectOfType<GameMaster>();
        if (gm == null)
        {
            yield break;
        }

        string players_file_path = Application.streamingAssetsPath + "/Configs/players.json";
        string players_config = File.ReadAllText(players_file_path);

        data.id = gm.game.agents.Count; //set id of agent
        data.info.name = JsonConvert.DeserializeObject<PlayerConfigFile>(players_config).player_names[data.id]; //set name of agent
        gm.game.agents.Add(data.info.name, this);

        file_path = gm.game.game_folder + "/" + data.info.name + ".json";
        transform.Find("Name").transform.GetComponent<TMP_Text>().text = data.info.name;
        SaveJSON();
    }

    public IEnumerator PromptLlm(string prompt, Action<string> onComplete, int max_tokens = 128, PromptType type = PromptType.simulation)
    {
        string constructed_prompt = ConstructPrompt(prompt);

        if (gm.experiment.data.model == "ChatGPT-3.5 Turbo")
        {
            yield return new WaitForSeconds((float)(constructed_prompt.Length / gm.token_factor / 1000));
        }
        List<ChatMessage> messages = new()
        {
            new ChatMessage() { Role = "user", Content = constructed_prompt }
        };
        var req = new CreateChatCompletionRequest()
        {
            Model = "",
            Messages = messages,
            MaxTokens = max_tokens, 
        };

        var query = openai.CreateChatCompletion(req);

        yield return new WaitUntil(() => query.IsCompleted);

        if (gm.time.CalculatePhases() == 1)
        {
            gm.experiment.data.model = query.Result.Model;
            gm.experiment.SaveJson();
        }

        string response = query.Result.Choices[0].Message.Content;
        yield return StartCoroutine(CheckForError(response));
        response = FormatResponse(response);

        data.prompt_list.Add(new GPTPrompt(gm.time.Copy(), constructed_prompt, response, type));
        onComplete(response);
    }
    /// <summary>
    /// Extract the first name from the response of an agent
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public Agent ExtractName(string result)
    {
        if (result == null || gm.game.agents == null || result == "")
        {
            return null;
        }

        string[] sentences = result.Split(new char[] { '.','!' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string sentence in sentences)
        {
            string[] words = sentence.Split(new char[] { ' ',',','[',']' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in words)
            {
                if (gm.game.agents.ContainsKey(word))
                {
                    return gm.game.agents[word];
                }
            }
        }

        return null; // No matching name found in gm.game.agents
    }

    public Agent Checkname(string result)
    {
        if (result == null || gm.game.agents == null || result == "")
        {
            return null;
        }

        string name = result.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0];
        if(name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList().Count > 1)
        {
            return null;
        }

        if (name != GetName() && gm.game.agents.ContainsKey(name))
        {
            return gm.game.agents[name];
        }
            

        return null; // No matching name found in gm.game.agents
    }
    public string FormatResponse(string result)
    {
        if (result == null || gm.game.agents == null || result == "")
        {
            return null;
        }

        string statement = result.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries)[0];

        statement = statement.Replace("*", "");
        statement = statement.Trim();
        return statement;
    }
    IEnumerator CheckForError(string result)
    {
        if(result.Contains("ERROR"))
        {
            gm.game.data.winner = "Prompt error";
            gm.Print("Game was cancelled due to prompt error");
            gm.game.LogEvent($"Game was cancelled due to prompt error!");
            SetUnactive();
            gm.FinishGame();
            yield break;
        }
    }
    public IEnumerator ProcessReflection(string result)
    {
        if (result == null || gm.game.agents == null || result == "")
        {
            yield break;
        }

        string[] statements = result.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string statement in statements)
        {
            string[] words = statement.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            if (words[0] == null)
            {
                Debug.LogError("Reflection response was faulty");
                continue;
            }

            if (words.Length < 2)
            {
                Debug.LogError("Ids missing in reflection response");
                yield return StartCoroutine(SaveReflection(words[0], new Guid[0]));
                continue;
            }

            string[] ids = words[1].Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<Guid> guids = new();
            foreach (string id in ids)
            {
                try
                {
                    Guid guid = new(id);
                    guids.Add(guid);
                    Console.WriteLine("Parsed GUID for {0}: {1}", id, guid);
                }
                catch (FormatException)
                {
                    Console.WriteLine("Error: {0} is not in the correct format for a GUID", id);
                }
            }
            yield return StartCoroutine(SaveReflection(words[0].Trim(), guids.ToArray()));
            continue;
        }
    }
    public void ProcessPlanning(string result)
    {
        if (result == null || gm.game.agents == null || result == "")
        {
            return;
        }

        List<string> statements = result.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        if (statements[0] == null)
        {
            Debug.LogError("Faulty Plan response!");
            return;
        }

        string goal = statements[0].Trim();
        statements.RemoveAt(0);
        statements = statements.Select(s => s.Trim()).ToList();
        SavePlan(goal, statements.ToArray());
    }
    public IEnumerator ProcessReflectionOnPlayers(string result)
    {
        if (result == null || gm.game.agents == null || result == "")
        {
            yield break;
        }

        string[] statements = result.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string statement in statements)
        {
            yield return StartCoroutine(SaveReflection(statement, new Guid[0]));

            string[] words = statement.Split(new char[] { ' ',',','[',']' }, StringSplitOptions.RemoveEmptyEntries);
            //TODO store voting
            string agent = null;
            Role role;
            foreach (string word in words)
            {
                if (word != GetName() && gm.game.agents.ContainsKey(word))
                {
                    agent = word;
                    continue;
                }
                if (Enum.IsDefined(typeof(Role), word))
                {
                    role = (Role)Enum.Parse(typeof(Role), word);
                }
            }
        }
    }

    public void OpenModal()
    {
        GameObject new_modal = Instantiate(modal, FindObjectOfType<Canvas>().transform);
        AgentModal agent_modal = new_modal.GetComponentInChildren<AgentModal>();
        if (gm != null)
        {
            agent_modal.is_paused = gm.is_paused;
            gm.PauseGame();
        }
        agent_modal.SetAgent(this);
    }

    public string GetName()
    {
        return data.info.name;
    }
    public Role GetRole()
    {
        return data.info.role;
    }

    void Pulsate()
    {
        float pulseSpeed = 1f;
        float minScale = 1f; 
        float maxScale = 1.20f; 
        float scale = Mathf.Lerp(minScale, maxScale, Mathf.PingPong(Time.time * pulseSpeed, 1f));
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    List<Agent> GetOtherAgents()
    {
        return gm.game.GetAgents().Where(a => a != this).ToList();
    }
    public string FilterMemories(double temp_token_count)
    {
        foreach (var memory in data.memory_stream)
        {
            memory.recency_score = memory.creation.CalculatePhases() / gm.time.CalculatePhases();
        }
        List<Memory> mems = data.memory_stream.Where((m) => m is Observation o || m is Reflection r).ToList();
        if (mems.Count == 0)
        {
            return "";
        }

        string temp_mem_string = "";
        int mem_count = 0;

        while((temp_token_count + Math.Round(temp_mem_string.Length / gm.token_factor)) < (gm.context_length - MAX_RESPONSE_LENGTH) && mem_count <= mems.Count)
        {
            var temp_top = mems.OrderByDescending(m => m.recency_score + m.normalized_score).ThenByDescending(m => m.time_stamp).Take(mem_count).ToList();
            string temp_memories = string.Join("\n", temp_top.Select((m) => $"{m.id} -> {m.PrintForPrompt()}"));
            temp_mem_string = string.Format("These are the most relevant memories of yours, numbered and in chronological order (if a memory is a statement made by a player, it is followed by the player's name surround by # like this #<name>#):\n{0}\n\n", temp_memories);
            mem_count++;
        }

        if((temp_token_count + Math.Round(temp_mem_string.Length / gm.token_factor)) < (gm.context_length - MAX_RESPONSE_LENGTH))
        {
            mem_count--;
        }

        var top = mems.OrderByDescending(m => m.recency_score + m.normalized_score).ThenByDescending(m => m.time_stamp).Take(mem_count).ToList();
        top = top.OrderBy(m => m.time_stamp).ToList();
        string memories = string.Join("\n", top.Select((m) => $"{m.id} -> {m.PrintForPrompt()}"));

        return string.Format("These are the most relevant memories of yours, numbered and in chronological order (if a memory is a statement made by a player, it is followed by the player's name surround by # like this #<name>#):\n{0}\n\n", memories);

    }
}

