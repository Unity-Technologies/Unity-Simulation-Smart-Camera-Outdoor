using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;

public class ApplyLabeling : MonoBehaviour
{
    public GameObjectTags        m_Tags;
    public Camera                m_Camera;
    public SemanticSegmentationLabelConfig m_SemanticSegmentationLableConfig;

    private List<SemanticSegmentationLabelEntry> _labelEntries = new List<SemanticSegmentationLabelEntry>();
    
    void Awake()
    {
        int id = 1, startValue = 10000;

        for (int i = 0; i < m_Tags.tags.Length; i++)
        {
            var gameobjects = GameObject.FindGameObjectsWithTag(m_Tags.tags[i]);
            foreach (var go in gameobjects)
            {
                var labelingComponent = go.AddComponent<Labeling>();
                labelingComponent.labels = new List<string>() { m_Tags.tags[i] };
            }
            _labelEntries.Add(new SemanticSegmentationLabelEntry()
            {
                color = m_Tags.colors[i],
                label = m_Tags.tags[i]
            });
        }
        m_SemanticSegmentationLableConfig.Init(_labelEntries);

        var perceptionCamera = m_Camera.GetComponent<PerceptionCamera>();
        if (perceptionCamera == null)
            m_Camera.gameObject.AddComponent<PerceptionCamera>();

        perceptionCamera.AddLabeler(new SemanticSegmentationLabeler(m_SemanticSegmentationLableConfig));
    }
}
