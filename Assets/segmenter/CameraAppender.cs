using System.Collections.Generic;
using UnityEngine;
using System;
//using Unity.AI.Simulation;

public class Label
{
    public string label;
    public int labelId;
    public string category;
    public int categoryId;
    public Color32 color;

    public Label(string label, int labelId, string category, int categoryId, Color32 color)
    {
        this.label = label;
        this.labelId = labelId;
        this.category = category;
        this.categoryId = categoryId;
        this.color = color;
    }
}

/*
    NOTE: This scrip MUST be attached to the same GameObject as the ESim SDK CameraGrab script.
*/
public class CameraAppender : MonoBehaviour
{

    public Shader segmentShader;
    public Shader instanceShader;

    public bool segment_color;
    public bool segment_label_id = true;

    Dictionary<string, Color32> tagSegmentMap = new Dictionary<string, Color32>();
    Dictionary<string, Color32> tagLabelIdMap = new Dictionary<string, Color32>();

    // Default pink for unknown tags
    Color32 unknownTag = new Color32(244, 66, 238, 255);

    List<Label> labelList = new List<Label>(new Label[] {
        new Label("unlabeled"           ,  0,"void"        , 0, new Color32(  0,  0,  0, 255)),
        new Label("ego vehicle"         ,  1,"void"        , 0, new Color32(  0,  0,  0, 255)),
        new Label("rectification border",  2,"void"        , 0, new Color32(  0,  0,  0, 255)),
        new Label("out of roi"          ,  3,"void"        , 0, new Color32(  0,  0,  0, 255)),
        new Label("static"              ,  4,"void"        , 0, new Color32(  0,  0,  0, 255)),
        new Label("dynamic"             ,  5,"void"        , 0, new Color32(111, 74,  0, 255)),
        new Label("ground"              ,  6,"void"        , 0, new Color32( 81,  0, 81, 255)),
        new Label("road"                ,  7,"flat"        , 1, new Color32(128, 64,128, 255)),
        new Label("sidewalk"            ,  8,"flat"        , 1, new Color32(244, 35,232, 255)),
        new Label("parking"             ,  9,"flat"        , 1, new Color32(250,170,160, 255)),
        new Label("rail track"          , 10,"flat"        , 1, new Color32(230,150,140, 255)),
        new Label("building"            , 11,"construction", 2, new Color32( 70, 70, 70, 255)),
        new Label("wall"                , 12,"construction", 2, new Color32(102,102,156, 255)),
        new Label("fence"               , 13,"construction", 2, new Color32(190,153,153, 255)),
        new Label("guard rail"          , 14,"construction", 2, new Color32(180,165,180, 255)),
        new Label("bridge"              , 15,"construction", 2, new Color32(150,100,100, 255)),
        new Label("tunnel"              , 16,"construction", 2, new Color32(150,120, 90, 255)),
        new Label("pole"                , 17,"object"      , 3, new Color32(153,153,153, 255)),
        // NOTE: changed from polegroup because retagging takes too many clicks
        new Label("pole group"           , 18,"object"      , 3, new Color32(153,153,153, 255)),
        new Label("traffic light"       , 19,"object"      , 3, new Color32(250,170, 30, 255)),
        new Label("traffic sign"        , 20,"object"      , 3, new Color32(220,220,  0, 255)),
        new Label("vegetation"          , 21,"nature"      , 4, new Color32(107,142, 35, 255)),
        new Label("terrain"             , 22,"nature"      , 4, new Color32(152,251,152, 255)),
        new Label("sky"                 , 23,"sky"         , 5, new Color32( 70,130,180, 255)),
        new Label("person"              , 24,"human"       , 6, new Color32(220, 20, 60, 255)),
        new Label("rider"               , 25,"human"       , 6, new Color32(255,  0,  0, 255)),
        new Label("car"                 , 26,"vehicle"     , 7, new Color32(  0,  0,142, 255)),
        new Label("truck"               , 27,"vehicle"     , 7, new Color32(  0,  0, 70, 255)),
        new Label("bus"                 , 28,"vehicle"     , 7, new Color32(  0, 60,100, 255)),
        new Label("caravan"             , 29,"vehicle"     , 7, new Color32(  0,  0, 90, 255)),
        new Label("trailer"             , 30,"vehicle"     , 7, new Color32(  0,  0,110, 255)),
        new Label("train"               , 31,"vehicle"     , 7, new Color32(  0, 80,100, 255)),
        new Label("motorcycle"          , 32,"vehicle"     , 7, new Color32(  0,  0,230, 255)),
        new Label("bicycle"             , 33,"vehicle"     , 7, new Color32(119, 11, 32, 255)),
        new Label("license plate"       , -1,"vehicle"     , 7, new Color32(  0,  0,142, 255))
    });

    void SetUpColorTagDicts()
    {
        foreach (Label entry in labelList)
        {
            // Colorful view
            tagSegmentMap.Add(entry.label, entry.color);

            // Monochrome view
            Color32 labelCol = new Color32((Byte)entry.labelId, (Byte)entry.labelId, (Byte)entry.labelId, 255);
            tagLabelIdMap.Add(entry.label, labelCol);
        }

    }

    Color32 GetTagColor(string tagName)
    {
        // If tag exists in dict return color
        if (tagSegmentMap.TryGetValue(tagName, out Color32 outColor))
            return outColor;

        // If tag is unknown return unknownTag color
        return unknownTag;
    }

    Color32 GetInstanceColor(string tagName)
    {
        // If tag exists in dict return color
        if (tagLabelIdMap.TryGetValue(tagName, out Color32 outColor))
            return outColor;

        // If tag is unknown return unknownTag color
        return unknownTag;
    }



    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log(Application.persistentDataPath);
        // Is this script attached to game object with ESim SDK CameraGrab script?
//        if (this.GetComponent<CameraGrab>()._cameraSources == null)
//        {
//            Debug.Log("Could not located ESimSDK game object");
//            Application.Quit();
//        }

        SetUpColorTagDicts();

        List<Camera> camerasToAdd = new List<Camera>();
        GameObject[] cameraObjects = GameObject.FindGameObjectsWithTag("CamerasObject");

        if (cameraObjects.Length <= 0)
        {
            Debug.Log("Could not located CameraObject game objects");
            Application.Quit();
        }

        // Find all renderable objects and add variables for segment shader
        var renderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
        var mpb = new MaterialPropertyBlock();
        foreach (var r in renderers)
        {
            mpb.SetColor("_SegmentColor", GetTagColor(r.transform.tag));
            mpb.SetColor("_InstanceColor", GetInstanceColor(r.transform.tag));

            r.SetPropertyBlock(mpb);

        }

        /*
         * For each CameraObject append to CamerasToAdd list and add replacement
         * shader for cameras tagged SegmentCam.        
         */

        for (int gameObjNum = 0; gameObjNum < cameraObjects.Length; gameObjNum++)
        {
            for (int camNum = 0; camNum < cameraObjects[gameObjNum].transform.childCount; camNum++)
            {
                Transform camTransform = cameraObjects[gameObjNum].transform.GetChild(camNum);
                camTransform.name = gameObjNum.ToString() + camTransform.name;
                Camera cam = camTransform.GetComponent<Camera>();

                if (cam == null)
                    continue;

                // If is segmentation camera, set replacement shader
                if (camTransform.CompareTag("SegmentCam") && segment_color)
                {
                    cam.SetReplacementShader(segmentShader, "RenderType");
                    cam.renderingPath = RenderingPath.Forward;
                    cam.depthTextureMode = DepthTextureMode.Depth;
                    camerasToAdd.Add(cam);
                }

                // TODO: Update name from Instance to Segment Labels
                if (camTransform.CompareTag("InstanceCam") && segment_label_id)
                {
                    cam.SetReplacementShader(instanceShader, "RenderType");
                    cam.renderingPath = RenderingPath.Forward;
                    cam.depthTextureMode = DepthTextureMode.Depth;
                    camerasToAdd.Add(cam);
                }

                if (camTransform.CompareTag("MainCam"))
                    camerasToAdd.Add(cam);
            }
        }

        // Set the ESim SDK camera sources
        //this.GetComponent<CameraGrab>()._cameraSources = camerasToAdd.ToArray();
    }

}
