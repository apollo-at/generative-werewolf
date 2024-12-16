using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PromptType {
    simulation,
    manual
}

public class GPTPrompt
{
    public Guid id;
    public DateTime time_stamp;
    public GameTime time;
    public string prompt;
    public string response;
    public PromptType type;

    public GPTPrompt(GameTime time, string prompt, string response, PromptType type)
    {
        id = Guid.NewGuid();
        this.time = time;
        time_stamp = DateTime.Now;
        this.prompt = prompt;
        this.response = response;
        this.type = type;
    }
}
