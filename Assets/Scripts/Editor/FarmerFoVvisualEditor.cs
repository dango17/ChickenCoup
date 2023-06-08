// Author: Harry Oldham
// Collaborator: Liam Bansal
// Created On: 09/05/23

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
        //Vector3 viewAngleA = fov.DirFromAngle(-fov.viewAngle / 2, false);
        //Vector3 viewAngleB = fov.DirFromAngle(fov.viewAngle / 2, false);

        //Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius);
        //Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius);

        const float half = 0.5f;
        // Gets the desired angle to relative to target's y axis.
        float rotationAmount = half * fov.viewAngle - fov.transform.rotation.eulerAngles.y;
        Vector3[] fieldOfViewExtents = new Vector3[2];
        // Gets the forward direction of the quaternion to rotate by.
        fieldOfViewExtents[0] = Quaternion.Euler(0, -rotationAmount, 0) * Vector3.forward;
        // Finds the direction, relative to the target position, where the FOV extents will be drawn along.
        // View radius controls how far out the extents will be drawn.
        Vector3 fieldOfViewExtentPosition = fov.transform.position + fieldOfViewExtents[0] * fov.viewRadius;
        Debug.DrawLine(fov.transform.position, fieldOfViewExtentPosition);
        rotationAmount = half * fov.viewAngle + fov.transform.rotation.eulerAngles.y;
        fieldOfViewExtents[1] = Quaternion.Euler(0, rotationAmount, 0) * Vector3.forward;
        fieldOfViewExtentPosition = fov.transform.position + fieldOfViewExtents[1] * fov.viewRadius;
        Debug.DrawLine(fov.transform.position, fieldOfViewExtentPosition);

        foreach (Transform visibleTarget in fov.visibleTargets)
        {
            Handles.color = Color.red;
            Handles.DrawLine(fov.transform.position, visibleTarget.position);
        }
    }
}
