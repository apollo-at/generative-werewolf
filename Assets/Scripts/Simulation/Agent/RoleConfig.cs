using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Role
{
    villager,
    werewolf,
    seer,
    witch,
}
public class RoleConfigFile
{
    public Dictionary<Role, RoleConfig> role_configs;

}


public class RoleConfig
{
    public string description;
    public string init_memory;
}
