// Author: Harry Oldham
// Collaborator: N/A
// Created On: 13/02

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
/// <summary>
/// 
/// </summary>
public class Scenemanager : MonoBehaviour
{
    public GameObject playBtn;
    public GameObject optBtn;
    public GameObject extBtn;
    public GameObject backBtn;
    public GameObject ResMenu;
    public GameObject selectfirstBtn, optionsfirstBtn, optionsclosedBtn;
    public TextMeshProUGUI valueText;

    // Start is called before the first frame update
    void Start()
    {
        //default active menu layout on game start
        playBtn.SetActive(true);
        optBtn.SetActive(true);
        extBtn.SetActive(true);
        backBtn.SetActive(false);
        ResMenu.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);

        EventSystem.current.SetSelectedGameObject(selectfirstBtn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play()
    {
        SceneManager.LoadScene("Barn");
    }

    public void Options()
    {
        playBtn.SetActive(false);
        optBtn.SetActive(false);
        extBtn.SetActive(false);
        backBtn.SetActive(true);
        ResMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(optionsfirstBtn);
    }

    public void backButton()
    {
        playBtn.SetActive(true);
        optBtn.SetActive(true);
        extBtn.SetActive(true);
        backBtn.SetActive(false);
        ResMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(optionsclosedBtn);
    }

    public void OnSliderChanged(float value)
    {
        valueText.text = value.ToString();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
