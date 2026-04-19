using UnityEngine;
using UnityEngine.UI;

public class HexDataCostUI : W2C
{
    [SerializeField]
    private Text _text;

    private MapHex _hex;
    private int _lastValue;

    public void SetHex(MapHex hex)
    {
        SetPosition(hex.transform);

        _hex = hex;
        _text.text = hex.CurrentFog.ToString();
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (_lastValue != _hex.CurrentFog)
        {
            _lastValue = _hex.CurrentFog;
            _text.text = _hex.CurrentFog.ToString();
        }

        if (_hex.CurrentFog <= 0 || _hex.CanBeDefogged == false)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 12);
        }
    }
}
