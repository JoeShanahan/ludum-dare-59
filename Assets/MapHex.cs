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

    public void SetObject(MergeObject mergeObject)
    {
        _currentObject = mergeObject;
    }

    public void SyncVisuals()
    {
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
