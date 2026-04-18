using UnityEngine;

public class MergeObject : MonoBehaviour
{
    public MergeObjectData Data;

    [SerializeField]
    private MapHex _currentHex;

    public MapHex CurrentHex => _currentHex;

    public void SetCurrentHex(MapHex hex)
    {
        _currentHex = hex;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
