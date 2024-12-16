using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statement
{
    public string text;
    protected Statement(string text_)
    {
        text = text_;
    }
}

public class AgentStatement: Statement
{
    public Agent agent;
    public AgentStatement(string text_, Agent agent): base(text_) 
    {
        text = text_;
        this.agent = agent;
    }
}

public class GameMasterStatement : Statement
{
    public bool game_flow;

    public GameMasterStatement(string text_, bool game_flow_) : base(text_)
    {
        text = text_;
        game_flow = game_flow_;
    }
}
