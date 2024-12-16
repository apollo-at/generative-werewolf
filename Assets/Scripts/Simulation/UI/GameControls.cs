using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControls : MonoBehaviour
{
    public GameObject play_button;
    public GameObject pause_button;

    // Start is called before the first frame update
    void Start()
    {
        play_button = transform.Find("Play").transform.Find("Background").gameObject;
        pause_button = transform.Find("Pause").transform.Find("Background").gameObject;
    }

    public void Play()
    {
        play_button.SetActive(true);
        pause_button.SetActive(false);
    }
    public void Pause()
    {
        play_button.SetActive(false);
        pause_button.SetActive(true);
    }
}
