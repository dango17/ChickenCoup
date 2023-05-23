// Author: Harry Oldham
// Collaborator: N/A
// Created On: 14/02/23
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DO; 

public class CutOutFollowplayer : MonoBehaviour
{
    public static int PosID = Shader.PropertyToID("_Position");
    public static int SizeID = Shader.PropertyToID("_Size");

    //I would recommend adding more meterials if theyre needed, or store the cutout shader as a serializedFeild
    //so we can access "_Size" from several different materials rather than the 1 "roofMaterial". 
    public Material[] RoofMaterials;
    public Material RoofMaterial;

    public Camera Camera;
    public LayerMask Mask;

    PlayerController playerController; 

    public void Start()
    {
        playerController = FindObjectOfType<PlayerController>(); 
    }

    void Update()
    {
        var dir = Camera.transform.position - transform.position;
        var ray = new Ray(transform.position, dir.normalized);

        //Layering
        if(Physics.Raycast(ray, 3000, Mask))
        {
            //RoofMaterial.SetFloat(SizeID, 1);
        }
        else
        {
            //RoofMaterial.SetFloat(SizeID, 0);
        }

        var view = Camera.WorldToViewportPoint(transform.position);
        RoofMaterial.SetVector(PosID, view);

        //Disable cutout in FP
        if (playerController.isInFreeLook)
        {
            RoofMaterial.SetFloat("_Size", 0.0f);
        }
        else
        {
            //Returns back to set value 
        }
    }
}
