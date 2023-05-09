using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FarmerFoVvisual))]
public class FarmerFoVvisualEditor : Editor
{
    private void OnSceneGUI()
    {
        FarmerFoVvisual fov = (FarmerFoVvisual)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.viewRadius);
    }
}
