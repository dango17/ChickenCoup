// Author: Harry Oldham
// Collaborator: N/A
// Created On: 14/02
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutOutFollowplayer : MonoBehaviour
{
    public static int PosID = Shader.PropertyToID("_Position");
    public static int SizeID = Shader.PropertyToID("_Size");

    public Material RoofMaterial;
    public Camera Camera;
    public LayerMask Mask;
   
    
    void Update()
    {
        var dir = Camera.transform.position - transform.position;
        var ray = new Ray(transform.position, dir.normalized);

        if(Physics.Raycast(ray, 3000, Mask))
        {
            RoofMaterial.SetFloat(SizeID, 1);
        }
        else
        {
            RoofMaterial.SetFloat(SizeID, 0);
        }

        var view = Camera.WorldToViewportPoint(transform.position);
        RoofMaterial.SetVector(PosID, view);
    }
}
