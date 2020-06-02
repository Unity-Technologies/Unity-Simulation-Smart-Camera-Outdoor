using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

public class RenderPathBlockTest : MonoBehaviour {


    Dictionary<string, Color> colorMap = new Dictionary<string, Color>();
    Color32 car = new Color32(255, 0, 0, 255); // Potentially 1 instead of 255?]

    public Shader shader;

    // Use this for initialization
    void Start () {
        Camera cam = this.GetComponent<Camera>();
        cam.SetReplacementShader(shader, "RenderType");

      
        var cb = new CommandBuffer();
        //cb.SetGlobalFloat("_OutputMode", 1);
        cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cb);
        cam.AddCommandBuffer(CameraEvent.BeforeFinalPass, cb);
        //cam.SetReplacementShader(shader, "");
        cam.clearFlags = CameraClearFlags.SolidColor;
        //cam.renderingPath = RenderingPath.Forward;


        //var renderers = Object.FindObjectsOfType<Renderer>();
        //var mpb = new MaterialPropertyBlock();
        //foreach (var r in renderers)
        //{
        //    var obj_tag = r.gameObject.tag;
        //    Debug.Log(obj_tag);

        //    mpb.SetColor("_Color", Color.red);
        //    r.SetPropertyBlock(mpb);
        //}
    }

}
