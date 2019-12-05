using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(OnLoad))]
[Serializable]
public class ObjectBuilder : Editor
{
    [SerializeField] public GameObject gizmo;
    [SerializeField] public GameObject box;
    [SerializeField] public int amount;
    [SerializeField] public float far;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        OnLoad scpt = (OnLoad)target;
        gizmo = (GameObject)EditorGUILayout.ObjectField("Gizmo", gizmo, typeof(GameObject) , false);
        box = (GameObject)EditorGUILayout.ObjectField("Cube", box, typeof(GameObject), false);
        amount = EditorGUILayout.IntField("Amount", amount);
        far = EditorGUILayout.FloatField("Distance", far);


        if (GUILayout.Button("Build"))
        {
            OnLoad.Build(amount, gizmo, box, far);
        }
    }
}
