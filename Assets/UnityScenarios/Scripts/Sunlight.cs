using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Simulation;

public class Sunlight : MonoBehaviour
{
    [Header("Options: morning/afternoon/evening/night")]
    public string m_Daytime = "morning";

    [Range(0.5f, 2.0f)]
    public float m_Intensity = 1.0f;

    Dictionary<string, float> _lightIntenses = new Dictionary<string, float> {
        {"morning", 0.5f}, 
        {"afternoon", 1.0f}, 
        {"evening", 0.3f}, 
        {"night", 0.1f}
    };

    Dictionary<string, Vector3> _lightDirs = new Dictionary<string, Vector3> {
        {"morning", new Vector3(37, 240, 0)}, 
        {"afternoon", new Vector3(80, 240, 0)}, 
        {"evening", new Vector3(17, 240, 0)}, 
        {"night", new Vector3(180, 240, 0)},
    };

    Light _light;

    // Start is called before the first frame update
    void Start()
    {
        _light = GetComponent<Light>();
        if (_light == null) {
            Debug.LogError("Missing Light Component");
            return;
        }
        if (Configuration.Instance.IsSimulationRunningInCloud()) {
            m_Intensity = SimulationOptions.LightIntensity;
            m_Daytime = SimulationOptions.Daytime;
        } 
        if (!_lightIntenses.ContainsKey(m_Daytime)) {
            Debug.LogWarning(m_Daytime + " is not an option!");
            m_Daytime = "morning";
        }
        _light.intensity = m_Intensity * _lightIntenses[m_Daytime]; // set light intensity
        transform.rotation = Quaternion.Euler(_lightDirs[m_Daytime]);  // set light rotation
    }
}
