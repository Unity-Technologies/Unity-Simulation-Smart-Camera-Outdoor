using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ApplySemanticSegmentationShader : MonoBehaviour
{

    public Shader m_SemanticSegmentationShader;
    public Camera m_SemanticSegmentationCamera;
    public GameObjectTags m_Tags;

    Dictionary<string, Color32> _segmentDict = new Dictionary<string, Color32>();

    void Start()
    {
        Debug.Assert(m_SemanticSegmentationCamera!=null, "Segmentation Camera is not provided");
        //Debug.Log(Application.persistentDataPath + "/" + Configuration.Instance.GetAttemptId());
        Debug.Assert(m_Tags != null && m_Tags.tags.Length > 0, "No tags provided");

        for (int i = 0; i < m_Tags.tags.Length; i++)
        {
            _segmentDict.Add(m_Tags.tags[i], m_Tags.colors[i]);
        }

        ApplySemanticSegmentation();
    }

    /// <summary>
    /// Apply Semantic Segmentation replacement shader based on tags generated during Start.
    /// </summary>
    public void ApplySemanticSegmentation()
    {
        // Find all GameObjects with Mesh Renderer and add a color variable to be
        // used by the shader in it's MaterialPropertyBlock
        var renderers = FindObjectsOfType<MeshRenderer>();
        var mpb = new MaterialPropertyBlock();
        foreach (var r in renderers)
        {

            if (_segmentDict.TryGetValue(r.transform.tag, out Color32 outColor))
            {
                mpb.SetColor("_SegmentColor", outColor);
                r.SetPropertyBlock(mpb);
            }
        }

        // Finally set the Segment shader as replacement shader
        m_SemanticSegmentationCamera.SetReplacementShader(m_SemanticSegmentationShader, "RenderType");
    }

    public void ApplySemanticSegmentationForObject(GameObject go)
    {
        var renderer = (Renderer) go.transform.GetComponentInChildren(typeof(Renderer));
        var mpb = new MaterialPropertyBlock();
        if (_segmentDict.TryGetValue(go.transform.tag, out Color32 outColor))
        {
            mpb.SetColor("_SegmentColor", outColor);
            renderer.SetPropertyBlock(mpb);
        }
    }
}