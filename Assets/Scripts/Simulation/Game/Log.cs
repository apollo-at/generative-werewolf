using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public abstract class Log
{
    public Guid id;
    public DateTime time_stamp;
    public GameTime time;

    public Log( GameTime time)
    {
        id = Guid.NewGuid();
        this.time = time;
        time_stamp = DateTime.Now;
    }
}


public class Mistake: Log
{
    public AgentInfo speaker;
    public string stage;
    public string response;

    public Mistake(string response, AgentInfo speaker, string stage, GameTime time): base(time)
    {
        this.speaker = speaker;
        this.stage = stage;
        this.response = response;
    }
    public string Print()
    {
        return $"<color={ProjectColors.BlueHex}>{speaker.name}</color> during '{stage}': {response}";
    }
}

public class Poll : Log
{
    public List<Vote> votes;
    public string result;

    public Poll(string result, GameTime time, List<Vote> votes) : base(time)
    {
        this.votes = votes;
        this.result = result;
    }

    public string Print()
    {
        var content = "";
        foreach (var vote in votes)
        {
            content += $"<color={ProjectColors.BlueHex}>{vote.voter.name}</color> voted for <color={ProjectColors.RedHex}>{vote.vote.name}</color>\n";
        }
        return content += result;
    }
}
public class StatementLog : Log
{
    public AgentInfo speaker;
    public string statement;

    public StatementLog(string statement, AgentInfo speaker, GameTime time) : base(time)
    {  
        this.speaker = speaker;
        this.statement = statement;
    }

}

public class Event : Log
{
    public string action;

    public Event(string action, GameTime time) : base(time)
    {
        this.action = action;
    }

}

public class LogConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Log);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jObject = JObject.Load(reader);

        if (jObject["votes"] != null)
        {
            return jObject.ToObject<Poll>();
        }
        else if (jObject["response"] != null)
        {
            return jObject.ToObject<Mistake>();
        }
        else if (jObject["speaker"] != null)
        {
            return jObject.ToObject<StatementLog>();
        }
        else if (jObject["action"] != null)
        {
            return jObject.ToObject<Event>();
        }
        else
        {
            throw new Exception("Invalid JSON for Log type.");
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}