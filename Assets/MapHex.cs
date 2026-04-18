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

    [SerializeField]
    private Transform _coverTransform;

    [SerializeField]
    private Transform _extrasTransform;

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

    public void SetStartingState(int fog)
    {
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
