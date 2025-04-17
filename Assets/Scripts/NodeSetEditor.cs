using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(Board))]
public class NodeSetEditor : Editor
{
    SerializedProperty nodeSetListProperty;

    void OnEnable()
    {
        nodeSetListProperty = serializedObject.FindProperty("nodeSetList");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Board monopolyBoard = (Board)target;
        EditorGUILayout.PropertyField(nodeSetListProperty, true);

        if(GUILayout.Button("Change Image Colors"))
        {
            Undo.RecordObject(monopolyBoard, "Change Image Colors");
            for (int i = 0; i < monopolyBoard.nodeSetList.Count; i++)
            {
                Board.NodeSet nodeSet = monopolyBoard.nodeSetList[i];

                for (int j = 0; j < nodeSet.nodesInSetList.Count; j++)
                {
                    MonopolyNode node = nodeSet.nodesInSetList[j];
                    Image image = node.propertyColorField;
                    if (image != null)
                    {
                        Undo.RecordObject(image, "Change Image Color");
                        image.color = nodeSet.setColor;
                    }
                }
            }
        }


        serializedObject.ApplyModifiedProperties();
    }
}
