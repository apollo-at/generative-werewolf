public class Vote
{
    public AgentInfo vote;
    public AgentInfo voter;

    public Vote(AgentInfo vote, AgentInfo voter)
    {
        this.vote = vote;
        this.voter = voter;
    }
}
