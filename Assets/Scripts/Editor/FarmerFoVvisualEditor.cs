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
        //Vector3 viewAngleA = fov.DirFromAngle(-fov.viewAngle / 2, false);
        //Vector3 viewAngleB = fov.DirFromAngle(fov.viewAngle / 2, false);
        //Handles.DrawWireArc(fov.transform.position, Vector3.up, viewAngleA, fov.viewAngle, fov.viewRadius);
        //Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius);
        //Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius);

        
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.viewRadius);
        Vector3 viewAngleA = fov.DirFromAngle(-fov.viewAngle / 2, false);
        Vector3 viewAngleB = fov.DirFromAngle(fov.viewAngle / 2, false);

        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius);
        
        foreach (Transform visibleTarget in fov.visibleTargets)
        {
            Handles.color = Color.red;
            Handles.DrawLine(fov.transform.position, visibleTarget.position);
        }
    }
}
