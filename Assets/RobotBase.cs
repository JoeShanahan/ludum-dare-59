using UnityEngine;


public abstract class RobotTask
{
    public virtual string Status => "?";
}

public class FindSleepSpotTask : RobotTask
{
    public override string Status => "Looking for sleep spot";

    public MapHex CurrentHex;
}

public class RechargeTask : RobotTask
{
    public float SearchCooldown;
    public MergeObject Station;

    public override string Status
    {
        get
        {
            if (SearchCooldown > 0)
            {
                return $"Looking for recharge in {Mathf.Ceil(SearchCooldown)}...";
            }
            if (Station == null)
            {
                return "Looking for recharge";
            }

            return $"Recharging at {Station.Data.ObjectName}";
        }   
    }
}

public class UseProducerTask : RobotTask
{
    // public override string Status => $"Getting {Producer.Data.Produces.ObjectName} from {Producer.Data.name}";
    public override string Status => Producer.Data.TaskString;

    public ProducerObject Producer;
    public float Cooldown;
}

public class DoResearchTask : RobotTask
{
    public MergeObject Target;
}

public class RobotBase : MonoBehaviour
{
    public static RobotBase CURRENT_SELECTED;

    [SerializeField]
    protected MergeObjectData _mergeData;

    protected HexGrid _grid;

    public MergeObjectData Data => _mergeData;

    public float ChargePercent => (Power * 100f) / _mergeData.RobotValues.MaxCharge;
    
    public int Power { get; private set; }

    private RobotTask _directedTask;
    private RobotTask _currentTask;

    [SerializeField] protected float _turnSpeed = 1;
    [SerializeField] protected float _moveSpeed = 1;

    public string GetStatus()
    {
        if (CURRENT_SELECTED == this)
            return "Please select destination";

        if (_currentTask == null)
            return "No Task";

        return _currentTask.Status;
    }

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

    public void StartPickingTarget()
    {
        CURRENT_SELECTED = this;
    }

    public void CancelPickingTarget()
    {
        CURRENT_SELECTED = null;
    }

    public void PickTarget(HexObject hexObj)
    {
        CURRENT_SELECTED = null;

        if (hexObj is ProducerObject proObj)
        {
            StartProducerTask(proObj);
        }
    }

    public void StartProducerTask(ProducerObject obj)
    {
        bool doingWhatTold = _currentTask == _directedTask;

        _directedTask = new UseProducerTask() { Producer = obj };
    
        if (doingWhatTold)
        {
            _currentTask = _directedTask;
        }
    }

    public virtual bool MoveTo(Vector3 position)
    {
        Vector3 diffVec = position - transform.position;
        diffVec.y = 0;

        float signedAngle = Vector3.SignedAngle(transform.forward, diffVec, Vector3.up);
        signedAngle = Mathf.Clamp(signedAngle, -_turnSpeed * Time.deltaTime, _turnSpeed * Time.deltaTime);

        if (Mathf.Abs(signedAngle) > 0.1f)
        {
            Rotate(signedAngle);
            return false;
        }

        if (diffVec.magnitude > 1)
        {
            MoveForwards(_moveSpeed * Time.deltaTime);
            return false;
        }

        return true;
    }

    private void Update()
    {
        if (Power <= 0)
            StartRechargeTask();

        if (_currentTask == null && _directedTask == null)
            return;

        _currentTask ??= _directedTask;

        if (_currentTask is UseProducerTask pt)
            DoProducerTask(pt);
        else if (_currentTask is RechargeTask rt)
            DoRechargeTask(rt);
    }

    private void DoProducerTask(UseProducerTask proTask)
    {
        bool reachedDestination = MoveTo(proTask.Producer.transform.position);

        if (!reachedDestination)
        {
            return;
        }

        if (proTask.Cooldown > 0)
        {
            proTask.Cooldown -= Time.deltaTime;
            return;
        }

        proTask.Cooldown = 1f / _mergeData.RobotValues.DischargeRate;
        ChargeResult result = proTask.Producer.AddCharge(1);
        Power --;

        if (result == ChargeResult.MapFull || result == ChargeResult.Empty)
        {
            _directedTask = null;
            _currentTask = null;
        }
    }

    private void DoRechargeTask(RechargeTask rTask)
    {
        if (rTask.SearchCooldown > 0)
        {
            rTask.SearchCooldown -= Time.deltaTime;
            return;
        }
        
        if (rTask.Station == null)
        {
            float closest = 999;
            MergeObject best = null;

            foreach (MapHex hex in _grid.AllTiles)
            {
                if (hex.CurrentFog > 0 || hex.ObjectOnTop is not MergeObject mo)
                    continue;

                if (mo.Data.PowerValues.IsEnabled == false)
                    continue;

                if (mo.Reservation != null && mo.Reservation != this)
                    continue;

                float d = Vector3.Distance(mo.transform.position, transform.position);

                if (best == null)
                {
                    best = mo;
                    closest = d;
                    continue;
                }

                if (best.Data.PowerValues.PerMinute < mo.Data.PowerValues.PerMinute)
                    continue;

                if (d < closest)
                {
                    best = mo;
                    closest = d;
                }
            }


            if (best != null)
            {
                rTask.Station = best;
                best.Reservation = this;            
            }
        }

        if (rTask.Station == null)
        {
            rTask.SearchCooldown = 3;
            return;
        }

        bool reachedDestination = MoveTo(rTask.Station.transform.position);

        if (!reachedDestination)
        {
            return;
        }
    }

    private void StartRechargeTask()
    {
        if (_currentTask is RechargeTask)
            return;

        _currentTask = new RechargeTask();
    }

    protected virtual void Rotate(float amount)
    {
        
    }

    protected virtual void MoveForwards(float amount)
    {
        
    }
}
