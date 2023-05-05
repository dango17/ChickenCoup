using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

public class PostProcessingValues : MonoBehaviour
{
    private PostProcessVolume _postProcessVolume;
    private AmbientOcclusion _ao;
    

    private float intensityVal;
    private float radVal;

    //UI values
    [Header(" AO UI Values")]
    [SerializeField] private TextMeshProUGUI ao_intensityValueText;
    [SerializeField] private TextMeshProUGUI ao_radiusValueText;


    private void Start()
    {
        _postProcessVolume = GetComponent<PostProcessVolume>();
        _postProcessVolume.profile.TryGetSettings(out _ao);       
    }

    public void AmbientOcclusionOnOff(bool on)
    {
        if(on)
        {
            _ao.active = true;
        }
        else
        {
            _ao.active = false;
        }
    }

    public void AmbientMode(int index)
    {
        switch(index)
        {
            case 0: _ao.mode.value = AmbientOcclusionMode.ScalableAmbientObscurance; break;
            case 1: _ao.mode.value = AmbientOcclusionMode.MultiScaleVolumetricObscurance; break;
        }
    }

    public void AOIntensityValue(float sliderValue)
    {
        _ao.intensity.value = sliderValue;
        ao_intensityValueText.text = sliderValue.ToString("0");
    }

    public void AORadiusValue(float sliderValue)
    {
        _ao.radius.value = sliderValue;
        ao_radiusValueText.text = sliderValue.ToString("0");
    }

    public void SetAmbientQuality(int index)
    {
        switch (index)
        {
            case 0: _ao.quality.value = AmbientOcclusionQuality.Lowest; break;
            case 1: _ao.quality.value = AmbientOcclusionQuality.Low; break;
            case 2: _ao.quality.value = AmbientOcclusionQuality.Medium; break;
            case 3: _ao.quality.value = AmbientOcclusionQuality.High; break;
            case 4: _ao.quality.value = AmbientOcclusionQuality.Ultra; break;
        }
    }

    public void SetAmbientColor(int index)
    {
        switch (index)
        {
            case 0: _ao.color.value = Color.black; break;
            case 1: _ao.color.value = Color.green; break;
            case 2: _ao.color.value = Color.red; break;
            case 3: _ao.color.value = Color.blue; break;
        }
    }

    public void AmbientOnly(bool on)
    {
        if (on)
        {
            _ao.ambientOnly.value = true;
        }
        else
        {
            _ao.ambientOnly.value = false;
        }
    }

    public void GeneralSettings()
    {
        _ao.mode.value = AmbientOcclusionMode.MultiScaleVolumetricObscurance;
        _ao.intensity.value = intensityVal;
        _ao.radius.value = radVal;
        _ao.quality.value = AmbientOcclusionQuality.Medium;
        _ao.color.value = Color.blue;
        _ao.ambientOnly.value = true;
    }

    
}
