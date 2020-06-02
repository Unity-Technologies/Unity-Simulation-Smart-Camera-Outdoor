using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectMovementManager))]
public class ObjectMovementManagerEditor : Editor
{
    bool showPositions = false;
    public override void OnInspectorGUI()
    {


        ObjectMovementManager movementManager = (ObjectMovementManager)target;
        movementManager.raycastPosition = EditorGUILayout.Toggle("Raycst position", movementManager.raycastPosition);
        movementManager.resolution = EditorGUILayout.FloatField("Spline Resolution", movementManager.resolution);


        showPositions = EditorGUILayout.Foldout(showPositions, "Positions:");
        if (showPositions)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < movementManager.controlPoints.Count; i++)
            {



                GUILayout.Label("Point: " + i.ToString(), EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();
                movementManager.controlPoints[i] = EditorGUILayout.Vector3Field("", movementManager.controlPoints[i]);

                if (GUILayout.Button(new GUIContent("A", "Add point after this point"), GUILayout.MaxWidth(20)))
                {
                    movementManager.AddPointAfter(i);
                }
                if (GUILayout.Button(new GUIContent("R", "Remove this Point"), GUILayout.MaxWidth(20)))
                {
                    movementManager.RemovePoint(i);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

            }
            EditorGUI.indentLevel--;
        }

        if (GUILayout.Button("Add point at end"))
        {
            int i = movementManager.controlPoints.Count - 1;
            Vector3 position = Vector3.zero;

            if (i < movementManager.controlPoints.Count - 1 && movementManager.controlPoints.Count > i + 1)
            {
                position = movementManager.controlPoints[i];
                Vector3 positionSecond = movementManager.controlPoints[i + 1];
                if (Vector3.Distance((Vector3)positionSecond, (Vector3)position) > 0)
                    position = (position + positionSecond) * 0.5f;
                else
                    position.x += 1;
            }
            else if (movementManager.controlPoints.Count > 1 && i == movementManager.controlPoints.Count - 1)
            {
                position = movementManager.controlPoints[i];
                Vector3 positionSecond = movementManager.controlPoints[i - 1];
                if (Vector3.Distance((Vector3)positionSecond, (Vector3)position) > 0)
                    position = position + (position - positionSecond);
                else
                    position.x += 1;
            }
            else if (movementManager.controlPoints.Count > 0)
            {
                position = movementManager.controlPoints[i];
                position.x += 1;
            }

            movementManager.AddPoint(position);


        }

        if (GUILayout.Button("Clear points"))
        {
            movementManager.RemovePoints();


        }

        if (GUILayout.Button("Raycast points"))
        {
            for (int i = 0; i < movementManager.controlPoints.Count; i++)
            {
                Ray ray = new Ray(movementManager.controlPoints[i] + Vector3.up * 100, Vector3.down);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                    movementManager.controlPoints[i] = hit.point;
            }
        }
    }


    void OnSceneGUI()
    {
        Color baseColor = Handles.color;
        int controlId = GUIUtility.GetControlID(FocusType.Passive);

        ObjectMovementManager movementManager = (ObjectMovementManager)target;
        for (int i = 0; i < movementManager.controlPoints.Count; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 handlePos = movementManager.controlPoints[i];


            if (Tools.current == Tool.Move)
            {

                float size = 0.6f;
                size = HandleUtility.GetHandleSize(handlePos) * size;

                Handles.Label(handlePos + Vector3.up, "Point: " + i);

                Handles.color = Handles.xAxisColor;
                handlePos = Handles.Slider(handlePos, Vector3.right, size, Handles.ArrowHandleCap, 0.01f);
                Handles.color = Handles.yAxisColor;
                handlePos = Handles.Slider(handlePos, Vector3.up, size, Handles.ArrowHandleCap, 0.01f);
                Handles.color = Handles.zAxisColor;
                handlePos = Handles.Slider(handlePos, Vector3.forward, size, Handles.ArrowHandleCap, 0.01f);

                Vector3 halfPos = (Vector3.right + Vector3.forward) * size * 0.3f;
                Handles.color = Handles.yAxisColor;
                handlePos = Handles.Slider2D(handlePos + halfPos, Vector3.up, Vector3.right, Vector3.forward, size * 0.3f, Handles.RectangleHandleCap, 0.01f) - halfPos;
                halfPos = (Vector3.right + Vector3.up) * size * 0.3f;
                Handles.color = Handles.zAxisColor;
                handlePos = Handles.Slider2D(handlePos + halfPos, Vector3.forward, Vector3.right, Vector3.up, size * 0.3f, Handles.RectangleHandleCap, 0.01f) - halfPos;
                halfPos = (Vector3.up + Vector3.forward) * size * 0.3f;
                Handles.color = Handles.xAxisColor;
                handlePos = Handles.Slider2D(handlePos + halfPos, Vector3.right, Vector3.up, Vector3.forward, size * 0.3f, Handles.RectangleHandleCap, 0.01f) - halfPos;

                movementManager.controlPoints[i] = handlePos;


            }

            if (EditorGUI.EndChangeCheck())
            {

                Undo.RecordObject(movementManager, "Change Position");

            }
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.control)
        {


            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Undo.RecordObject(movementManager, "Add point");

                Vector4 position = hit.point;
                movementManager.AddPoint(position);


                GUIUtility.hotControl = controlId;
                Event.current.Use();
                HandleUtility.Repaint();
            }
        }
        if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && Event.current.control)
        {
            GUIUtility.hotControl = 0;

        }
    }

}
