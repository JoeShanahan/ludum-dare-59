using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum ChargeResult
{
    Error,
    Charging,
    Produced,
    MapFull
}

public class ProducerObject : HexObject
{
    public ProducerData Data => _data;
    public override bool CanBeMoved => false;

    [SerializeField]
    private ProducerData _data;

    private HexGrid _grid;

    [SerializeField]
    private int _currentCharge;

    private void Start()
    {
        _grid = FindFirstObjectByType<HexGrid>();
    }

    public ChargeResult AddCharge(int amount)
    {
        _currentCharge += amount;
        
        if (_data.PowerRequirement == 0)
        {
            Debug.LogError($"Shouldn't have zero as power requirement for {_data.name}");
            return ChargeResult.Error;
        }

        if (_currentCharge < _data.PowerRequirement)
            return ChargeResult.Charging;

        bool didFail = false;

        while (_currentCharge >= _data.PowerRequirement)
        {
            _currentCharge -= _data.PowerRequirement;

            if (ProduceItem() == false)
                didFail = true;
        }

        return didFail ? ChargeResult.MapFull : ChargeResult.Produced;
    }

    private bool ProduceItem()
    {
        MapHex freeHex = GetClosestFreeHex(CurrentHex);

        if (freeHex == null)
            return false;
        
        GameObject newObj = Instantiate(Data.Produces.Prefab, transform.parent);
        newObj.transform.position = CurrentHex.transform.position;

        HexObject newHexObj = newObj.GetComponent<HexObject>();
        newHexObj.SetCurrentHex(freeHex);
        freeHex.SetObject(newHexObj);
        newHexObj.GoToCurrentHex(0.0f);

        newObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        newObj.transform.DOScale(1, 0.25f).SetEase(Ease.OutExpo);
        return true;
    }

    private MapHex GetClosestFreeHex(MapHex startHex)
    {
        float dist = 9999;
        MapHex bestHex = null;

        foreach (MapHex hex in _grid.GetAllFreeTiles())
        {
            float d = Vector3.Distance(hex.transform.position, startHex.transform.position);
            
            if (d < dist)
            {
                dist = d;
                bestHex = hex;
            }
        }

        return bestHex;
    }

    public override IEnumerable<(string, string)> GetInfo()
    {
        yield return ("Power", $"{_currentCharge} / {Data.PowerRequirement}");
        yield return ("Produces", Data.Produces.ObjectName);
    }
}
