using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ProducerObject : HexObject
{
    public ProducerData Data => _data;
    public override bool CanBeMoved => false;

    [SerializeField]
    private ProducerData _data;

    private HexGrid _grid;

    private void Start()
    {
        _grid = FindFirstObjectByType<HexGrid>();
    }

    public void ProduceItem()
    {
        MapHex freeHex = GetClosestFreeHex(CurrentHex);

        if (freeHex != null)
        {
            GameObject newObj = Instantiate(Data.Produces.Prefab, transform.parent);
            newObj.transform.position = CurrentHex.transform.position;

            HexObject newHexObj = newObj.GetComponent<HexObject>();
            newHexObj.SetCurrentHex(freeHex);
            freeHex.SetObject(newHexObj);
            newHexObj.GoToCurrentHex(0.0f);

            newObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            newObj.transform.DOScale(1, 0.25f).SetEase(Ease.OutExpo);
        }
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
}
