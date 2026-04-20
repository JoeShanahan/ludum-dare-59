using UnityEngine;

public class RobotBase : MonoBehaviour
{
    [SerializeField]
    protected MergeObjectData _mergeData;

    protected HexGrid _grid;

    public MergeObjectData Data => _mergeData;

    public float ChargePercent => 66f;

    public virtual void Start()
    {
        _grid = FindFirstObjectByType<HexGrid>();
    }

    public virtual void WakeUp()
    {
        
    }

    public virtual void GoToSleep()
    {
        
    }

    public virtual void MoveTo(MapHex hex)
    {
        
    }
}
