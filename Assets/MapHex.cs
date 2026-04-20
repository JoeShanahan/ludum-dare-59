using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MapHex : MonoBehaviour
{
    public event Action OnTileFlip;

    public string TypeName => _hexType.TileName;
    public HexType Data => _hexType;

    [SerializeField]
    private HexType _hexType;

    [SerializeField]
    private int _fogCost;

    [SerializeField]
    private HexObject _currentObject;

    [SerializeField]
    private Transform _coverTransform;

    [SerializeField]
    private Transform _extrasTransform;

    [SerializeField]
    private Material[] _lockedFogMats;

    [SerializeField]
    private Material[] _unlockedFogMats;

    public HexObject ObjectOnTop => _currentObject;

    public IEnumerable<MapHex> Neighbours => _neighbours;

    private List<MapHex> _neighbours;

    public int CurrentFog => _fogCost;

    public bool CanBeDefogged { get; private set; }

    public void SetObject(HexObject mergeObject)
    {
        _currentObject = mergeObject;
    }

    private Transform _ogParent;

    public void SetObjectInit(MergeObject mergeObject)
    {
        _currentObject = mergeObject;

        if (_fogCost > 0)
        {
            _ogParent = mergeObject.transform.parent;
            transform.localEulerAngles = new Vector3(-90, 0, 0);
            mergeObject.transform.SetParent(transform);
            transform.localEulerAngles = new Vector3(90, 0, 0);
        }
    }

    public void FindNeighbours(IEnumerable<MapHex> hexList)
    {
        _neighbours = new();

        foreach (MapHex other in hexList)
        {
            if (other == this)
                continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);

            if (dist < 2.1f)
            {
                _neighbours.Add(other);
            }
        }
    }

    public void RemoveFog(int amount)
    {
        if (_fogCost <= 0)
            return;

        _fogCost -= amount;
        _fogCost = Mathf.Max(_fogCost, 0);
        DetermineFogStatus();

        if (_fogCost == 0)
        {
            transform.DOLocalMoveY(transform.localPosition.y + 1.5f, 0.2f).SetEase(Ease.OutSine);
            transform.DOLocalMoveY(transform.localPosition.y, 0.2f).SetDelay(0.2f).SetEase(Ease.InSine);
            transform.DOLocalRotate(new Vector3(-90, 0, 0), 0.45f).SetEase(Ease.OutSine).OnComplete(() =>
            {
                if (_currentObject != null)
                {
                    _currentObject.transform.parent = _ogParent;
                }
                Destroy(_coverTransform.gameObject);
            });


            OnTileFlip?.Invoke();
        }
    }

    public void DetermineFogStatus()
    {
        CanBeDefogged = false;

        if (_fogCost <= 0)
            return;

        foreach (MapHex hex in _neighbours)
        {
            if (hex.Data.IsWater == false && hex.CurrentFog <= 0)
            {
                CanBeDefogged = true;
                break;
            }
        }

        _coverTransform.GetComponent<MeshRenderer>().sharedMaterials = CanBeDefogged ? _unlockedFogMats : _lockedFogMats;
    }

    public void SetStartingState(int fog)
    {
        if (Data.IsWater)
            fog = 0;

        _fogCost = fog;

        if (fog == 0)
        {
            transform.localEulerAngles = new Vector3(-90, 0, 0);
            Destroy(_coverTransform.gameObject);
        }
        else
        {
            transform.localEulerAngles = new Vector3(90, 0, 0);
        }
    }
}
