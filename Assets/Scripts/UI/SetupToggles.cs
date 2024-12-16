using UnityEngine;
using UnityEngine.UI;

public class SetupToggles : MonoBehaviour
{
    Toggle memory_toggle;
    Toggle reflection_toggle;
    Toggle planning_toggle;

    private void Start()
    {
        memory_toggle = transform.Find("Memory").transform.GetComponent<Toggle>();
        reflection_toggle = transform.Find("Reflection").transform.GetComponent<Toggle>();
        planning_toggle = transform.Find("Planning").transform.GetComponent<Toggle>();
        memory_toggle.onValueChanged.AddListener(delegate
        {
            ExperimentSettings.memory = memory_toggle.isOn;
        });
        reflection_toggle.onValueChanged.AddListener(delegate
        {
            ExperimentSettings.reflection = reflection_toggle.isOn;
        });
        planning_toggle.onValueChanged.AddListener(delegate
        {
            ExperimentSettings.planning = planning_toggle.isOn;
        });

        memory_toggle.isOn = ExperimentSettings.memory = true;
        reflection_toggle.isOn = ExperimentSettings.reflection = false;
        planning_toggle.isOn = ExperimentSettings.planning = false;
    }
    private void Update()
    {
        if (!memory_toggle.isOn)
        {
            ExperimentSettings.reflection = reflection_toggle.isOn = false;
            ExperimentSettings.planning = planning_toggle.isOn = false;
        }
    }

}
