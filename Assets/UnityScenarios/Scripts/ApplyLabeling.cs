using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;

public class ApplyLabeling : MonoBehaviour
{
    public GameObjectTags        m_Tags;
    public Camera                m_Camera;
    public LabelingConfiguration m_LabelingConfig;
    
    void Awake()
    {
        int id = 1, startValue = 10000;
        foreach (var tag in m_Tags.tags)
        {
            var gameobjects = GameObject.FindGameObjectsWithTag(tag);
            foreach (var go in gameobjects)
            {
                var labelingComponent = go.AddComponent<Labeling>();
                labelingComponent.labels = new List<string>() { tag };
            }
            m_LabelingConfig.LabelEntries.Add(new LabelEntry(id++, tag, startValue));
            startValue += 1000;
        }

        var perceptionCamera = m_Camera.GetComponent<PerceptionCamera>();
        if (perceptionCamera == null)
            m_Camera.gameObject.AddComponent<PerceptionCamera>();

        perceptionCamera.LabelingConfiguration = m_LabelingConfig;
        perceptionCamera.produceSegmentationImages = true;
    }
}
