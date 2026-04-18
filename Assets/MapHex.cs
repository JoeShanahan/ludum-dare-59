using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapHex : MonoBehaviour
{
    public string TypeName => _hexType.TileName;

    [SerializeField]
    private HexType _hexType;

    [SerializeField]
    private int _fogCost;

    [SerializeField]
    private MergeObject _currentObject;

    public MergeObject ObjectOnTop => _currentObject;

    public IEnumerable<MapHex> Neighbours => _neighbours;

    private List<MapHex> _neighbours;

    public void SetObject(MergeObject mergeObject)
    {
        _currentObject = mergeObject;
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

    public void SyncVisuals()
    {
        // TODO check for duplicate positions
        GetComponent<MeshRenderer>().sharedMaterial = _hexType.Material;

        int row = Mathf.RoundToInt(transform.localPosition.z / 1.5f);
        bool isOdd = Mathf.Abs(row) % 2 == 1;

        if (isOdd)
        {
            int column = Mathf.RoundToInt((transform.localPosition.x + 0.5f) / 1.5f);
            gameObject.name = $"Hex({column},{row})";
        }
        else
        {
            int column = Mathf.RoundToInt(transform.localPosition.x / 1.5f);
            gameObject.name = $"Hex({column},{row})";
        }


    }
}
