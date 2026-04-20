using System;
using UnityEngine;
using UnityEngine.UI;

public class SoundMuteButton : MonoBehaviour
{
    [SerializeField]
    private Image _image;

    [SerializeField]
    private Sprite _soundOn;

    [SerializeField]
    private Sprite _soundOff;

    [SerializeField]
    private AudioSource _src;

    private bool _isEnabled;
    private float _volume;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _volume = _src.volume;

        _isEnabled = PlayerPrefs.GetInt("SoundToggle", 1) == 1;
        _image.sprite = _isEnabled ? _soundOn : _soundOff;
        _src.volume = _isEnabled ? _volume : 0;

    }

    public void ToggleAudio()
    {
        _isEnabled = !_isEnabled;
        PlayerPrefs.SetInt("SoundToggle", _isEnabled ? 1 : 0);
        PlayerPrefs.Save();

        _image.sprite = _isEnabled ? _soundOn : _soundOff;
        _src.volume = _isEnabled ? _volume : 0;
    }
}
