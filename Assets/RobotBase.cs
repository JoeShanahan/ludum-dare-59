using UnityEngine;

public class RobotBase : MonoBehaviour
{
    [SerializeField]
    protected MergeObjectData _mergeData;

    protected HexGrid _grid;

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
