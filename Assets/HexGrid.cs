using UnityEngine;

public class HexGrid : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("SYNC ALL")]
    public void SyncAllTiles()
    {
        foreach (Transform t in transform)
        {
            if(t.TryGetComponent(out MapHex hex))
            {
                hex.SyncVisuals();
            }
        }
    }
}
