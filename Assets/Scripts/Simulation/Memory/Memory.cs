using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Unity.VisualScripting;
using System.Numerics;
using System.Linq;

public abstract class Memory
{
    public Guid id;
    public string content;
    public int score;
    public float normalized_score;
    public float recency_score;
    public DateTime time_stamp;
    public GameTime creation;
    public GameTime last_accessed;

    public Memory(string content, GameTime time,int score)
    {
        id = Guid.NewGuid();
        this.content = content;
        creation = time;
        last_accessed = time;
        this.score = score;
        normalized_score = score / 10;
        time_stamp = DateTime.Now;
    }

    /// <summary>
    /// Create string from Memory
    /// </summary>
    /// <returns></returns>
    public string Print()
    {
        if(this is Observation observation)
        {
            if(observation.actor != null)
            {
                return $"<color={ProjectColors.BlueHex}>{observation.actor.name} ({observation.actor.role.ToString().FirstCharacterToUpper()})</color>: {content}";
            }
        }
        if (this is Plan plan)
        {
            string[] steps = plan.steps.Select((s, i) => $"<color={ProjectColors.BlueHex}>Step{i + 1}: </color>{s}").ToArray();
            string step = string.Join("\n", steps);
            return $"<color={ProjectColors.VioletHex}>{content}</color>\n{step}";
        }
        return content;
    }

    /// <summary>
    /// Create string from Memory for prompt without agent role or coloring
    /// </summary>
    /// <returns></returns>
    public string PrintForPrompt()
    {
        if (this is Observation observation)
        {
            if (observation.actor != null)
            {
                return $" {content}: #{observation.actor.name}# ({creation.Print()})";
            }
            return $"{content} ({creation.Print()})";
        }
        if (this is Reflection reflection)
        {
            return $"{content} ({creation.Print()})";
        }
        if (this is Plan plan)
        {
            string[] steps = plan.steps.Select((s, i) => $"Step{i + 1}: {s}").ToArray();
            string step = string.Join("\n", steps);
            return $"{content}\n{step}";
        }
        return content;
    }
}

public class Observation : Memory
{
    public AgentInfo actor;
    public Observation(string content, GameTime time, int score, AgentInfo actor) : base(content, time, score)
    {
        if (actor != null) { this.actor = actor; }

    }
}

public class Reflection : Memory
{
    public Guid[] observations;
    public Reflection(string content, GameTime time, Guid[] observations, int score = 0) : base(content, time,score)
    {
        this.observations = observations;
    }
}

public class Plan : Memory
{
    public string[] steps;
    public Plan(string content, GameTime time, string[] steps, int score = 0) : base(content, time, score)
    {
        this.steps = steps;
    }
}
public class Experience : Memory
{
    public bool is_win;
    public Experience(string content, GameTime time, bool is_win, int score = 0) : base(content, time,  score)
    {
        this.is_win = is_win;
    }
}
public class ExperiencePool
{
    public List<Experience> experiences = new();
}
public class MemoryConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Memory);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jObject = JObject.Load(reader);

        // Determine the type of Memory based on JSON properties
        if (jObject["actor"] != null)
        {
            return jObject.ToObject<Observation>();
        }
        else if (jObject["is_win"] != null)
        {
            return jObject.ToObject<Experience>();
        }
        else if (jObject["observations"] != null)
        {
            return jObject.ToObject<Reflection>();
        }
        else if (jObject["memories"] == null)
        {
            return jObject.ToObject<Plan>();
        }
        else
        {
            throw new Exception("Invalid JSON for Memory type.");
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
