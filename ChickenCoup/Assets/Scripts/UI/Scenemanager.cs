// Author: Harry Oldham
// Collaborator: N/A
// Created On: 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 
/// </summary>
public class Scenemanager : MonoBehaviour
{
    public GameObject playBtn;
    public GameObject optBtn;
    public GameObject extBtn;
    public GameObject backBtn;

    // Start is called before the first frame update
    void Start()
    {
        //default active menu layout on game start
        playBtn.SetActive(true);
        optBtn.SetActive(true);
        extBtn.SetActive(true);
        backBtn.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Options()
    {
        playBtn.SetActive(false);
        optBtn.SetActive(false);
        extBtn.SetActive(false);
        backBtn.SetActive(true);
    }

    public void backButton()
    {
        playBtn.SetActive(true);
        optBtn.SetActive(true);
        extBtn.SetActive(true);
        backBtn.SetActive(false);
    }
}
