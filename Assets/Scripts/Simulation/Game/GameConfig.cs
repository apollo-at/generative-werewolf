using System.Collections.Generic;

public class PlayerConfigFile
{
    public Dictionary<string, PlayerConfig> player_configs;
    public List<string> player_names;
    public int context_length;
    public double token_factor;

}

public class PlayerConfig
{
    public List<Role> roles;
}
