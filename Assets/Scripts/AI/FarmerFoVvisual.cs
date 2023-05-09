using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmerFoVvisual : MonoBehaviour
{
    public float viewRadius;
    [Range(0,360)]

    public float viewAngle;

    public Vector3 DirFromAngle(float angleInDegrees, bool angleisGlobal)
    {
        if (!angleisGlobal)
        {

        }
        return new Vector3(Mathf.Sin(angleInDegrees = Mathf.Deg2Rad),0,Mathf.Cos(angleInDegrees = Mathf.Deg2Rad));
    }

   

    
}
