using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tutorialtrigger : MonoBehaviour
{
    public GameObject dialoguetext;

    void Start()
    {
        dialoguetext.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        dialoguetext.SetActive(true);
    }

    
}
