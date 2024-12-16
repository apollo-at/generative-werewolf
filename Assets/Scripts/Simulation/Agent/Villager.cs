using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class Villager : Agent
{
    // Start is called before the first frame update
    public void Start()
    {
        data.info.role = Role.villager;
        StartCoroutine(InitAgent());
    }
}
