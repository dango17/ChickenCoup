using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using TMPro;


public class BloomSettings : MonoBehaviour
{
    private PostProcessVolume _postProcessVolume;
    private Bloom _bloom;

    [Header("UI Values")]
    [SerializeField] private TextMeshProUGUI bloom_intensityValueText;
    [SerializeField] private TextMeshProUGUI bloom_thresholdValueText;
    [SerializeField] private TextMeshProUGUI bloom_softkneeValueText;
    [SerializeField] private TextMeshProUGUI bloom_diffusionValueText;
    [SerializeField] private TextMeshProUGUI bloom_anamorphicValueText;

    private void Start()
    {
        _postProcessVolume = GetComponent<PostProcessVolume>();
        _postProcessVolume.profile.TryGetSettings(out _bloom);
    }

    public void BloomOnOff(bool on)
    {
        if(on)
        {
            _bloom.active = true;
        }
        else
        {
            _bloom.active = false;
        }
    }

    public void BLIntensityValue(float sliderValue)
    {
        _bloom.intensity.value = sliderValue;
        bloom_intensityValueText.text = sliderValue.ToString("0");
    }

    public void BLThresholdValue(float sliderValue)
    {
        _bloom.threshold.value= sliderValue;
        bloom_thresholdValueText.text = sliderValue.ToString("0");
    }

    public void BLSoftKneeValue(float sliderValue)
    {
        _bloom.softKnee.value = sliderValue;
        bloom_softkneeValueText.text = sliderValue.ToString("0");
    }

    public void BLDiffusionValue(float sliderValue)
    {
        _bloom.diffusion.value = sliderValue;
        bloom_diffusionValueText.text = sliderValue.ToString("0");
    }

    public void BLAnamorphicValue(float sliderValue)
    {
        _bloom.anamorphicRatio.value = sliderValue;
        bloom_diffusionValueText.text = sliderValue.ToString("0");
    }

    public void SetBloomColour(int index)
    {
        switch (index)
        {
            case 0: _bloom.color.value = Color.black; break;
            case 1: _bloom.color.value = Color.green; break;
            case 2: _bloom.color.value = Color.red; break;
            case 3: _bloom.color.value = Color.blue; break;
        }
    }

    public void FastMode(bool on)
    {
        if (on)
        {
            _bloom.fastMode.value = true;
           
        }
        else
        {
            _bloom.fastMode.value = false;
        }
    }


}
