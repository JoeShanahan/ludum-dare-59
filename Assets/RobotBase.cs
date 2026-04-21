using UnityEngine;


public abstract class RobotTask
{
    public virtual string Status => "?";

    public virtual void CleanUp(RobotBase robo)
    {
        
    }
}

public class FindSleepSpotTask : RobotTask
{
    public float SearchCooldown;
    public MapHex SleepHex;
    public bool IsLockedIn;
    public float SleepIn;

    public override string Status
    {
        get
        {
            if (IsLockedIn)
            {
                return "Powering down...";
            }

            if (SearchCooldown > 0 || SleepHex == null)
            {
                return $"Looking for empty spot in {Mathf.Ceil(SearchCooldown)}...";
            }

            return $"Heading to sleep";
        }   
    }
}

public class RechargeTask : RobotTask
{
    public float SearchCooldown;
    public float ChargeCooldown;
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

    public override void CleanUp(RobotBase robo)
    {
        if (Station != null && Station.Reservation == robo)
        {
            Station.Reservation = null;
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

    private Vector3 _sleepVector = new Vector3(0.34f, 0.00f, -0.94f);

    [SerializeField] protected float _turnSpeed = 1;
    [SerializeField] protected float _moveSpeed = 1;

    public void SetPower(int startingPower)
    {
        Power = startingPower;
    }

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
            _currentTask?.CleanUp(this);
            _currentTask = _directedTask;
        }
    }

    public void StartSleepTask()
    {
        if (_currentTask is FindSleepSpotTask)
            return;

        _directedTask = new FindSleepSpotTask();
        _currentTask?.CleanUp(this);
        _currentTask = _directedTask;
    }

    public virtual bool MoveTo(Vector3 position, float proximity)
    {
        Vector3 diffVec = position - transform.position;
        diffVec.y = 0;

        float signedAngle = Vector3.SignedAngle(transform.forward, diffVec, Vector3.up);
        signedAngle = Mathf.Clamp(signedAngle, -_turnSpeed * Time.deltaTime, _turnSpeed * Time.deltaTime);
        
        if (diffVec.magnitude > proximity * 1.05f)
        {
            if (Mathf.Abs(signedAngle) > 0.1f)
            {
                Rotate(signedAngle);
                return false;
            }
        }

        if (diffVec.magnitude > proximity)
        {
            float dist = Mathf.Min(_moveSpeed * Time.deltaTime, diffVec.magnitude);
            MoveForwards(dist);
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
        else if (_currentTask is FindSleepSpotTask st)
            DoSleepTask(st);
    }

    private void DoProducerTask(UseProducerTask proTask)
    {
        bool reachedDestination = MoveTo(proTask.Producer.transform.position, 1);

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
            _currentTask?.CleanUp(this);
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
            rTask.SearchCooldown = 6;
            return;
        }

        bool reachedDestination = MoveTo(rTask.Station.transform.position, 1);

        if (!reachedDestination)
        {
            return;
        }

        if (rTask.ChargeCooldown > 0)
        {
            rTask.ChargeCooldown -= Time.deltaTime;
            return;
        }

        float perMinute = rTask.Station.Data.PowerValues.PerMinute;
        float perSecond = perMinute / 60;
        Power ++;
        rTask.ChargeCooldown = 1f / perSecond;

        if (Power >= _mergeData.RobotValues.MaxCharge)
        {
            Power = _mergeData.RobotValues.MaxCharge;
            _currentTask.CleanUp(this);
            _currentTask = null;
        }
    }

    private void DoSleepTask(FindSleepSpotTask sTask)
    {
        if (sTask.IsLockedIn)
        {
            sTask.SleepIn -= Time.deltaTime;

            if (sTask.SleepIn <= 0)
            {
                if (sTask.SleepHex.ObjectOnTop != null)
                {
                    Debug.LogWarning("Something took my spot before I could sleep in it!");
                    WakeUp();
                    sTask.IsLockedIn = false;
                    sTask.SleepHex = null;
                }
                else
                {
                    GameObject newObj = GameObject.Instantiate(_mergeData.Prefab, transform.parent);
                    MergeObject sleepingObj = newObj.GetComponent<MergeObject>();
                    sleepingObj.SetCurrentHex(sTask.SleepHex);
                    sTask.SleepHex.SetObject(sleepingObj);
                    sleepingObj.InitFromRobot(this);

                    newObj.transform.position = sTask.SleepHex.transform.position;

                    if (RobotContextMenu.SINGLETON != null && RobotContextMenu.SINGLETON.AwakeObj == this)
                    {
                        RobotContextMenu.SINGLETON.InitAsleep(sleepingObj);
                    }

                    Destroy(gameObject);

                    return;            
                }
            }

            return;
        }

        if (sTask.SleepHex != null && sTask.SleepHex.ObjectOnTop != null)
        {
            sTask.SleepHex = null;
        }

        if (sTask.SearchCooldown > 0)
        {
            sTask.SearchCooldown -= Time.deltaTime;
            return;
        }
        
        if (sTask.SleepHex == null)
        {
            float closest = 999;
            MapHex best = null;

            foreach (MapHex hex in _grid.AllTiles)
            {
                if (hex.CurrentFog > 0 || hex.ObjectOnTop != null)
                    continue;

                if (hex.Data.IsWater)
                    continue;

                float d = Vector3.Distance(hex.transform.position, transform.position);

                if (d < closest)
                {
                    best = hex;
                    closest = d;
                }
            }


            if (best != null)
            {
                sTask.SleepHex = best;     
            }
        }

        if (sTask.SleepHex == null)
        {
            sTask.SearchCooldown = 6;
            return;
        }

        bool reachedDestination = MoveTo(sTask.SleepHex.transform.position, 0.01f);

        if (!reachedDestination)
        {
            return;
        }

        float signedAngle = Vector3.SignedAngle(transform.forward, _sleepVector, Vector3.up);
        signedAngle = Mathf.Clamp(signedAngle, -_turnSpeed * Time.deltaTime, _turnSpeed * Time.deltaTime);

        if (Mathf.Abs(signedAngle) > 1f)
        {
            Rotate(signedAngle);
            return;
        }

        sTask.IsLockedIn = true;
        sTask.SleepIn = 1f;
        GoToSleep();
    }

    private void StartRechargeTask()
    {
        if (_currentTask is RechargeTask)
            return;

        _currentTask?.CleanUp(this);
        _currentTask = new RechargeTask();
    }

    protected virtual void Rotate(float amount)
    {
        
    }

    protected virtual void MoveForwards(float amount)
    {
        
    }
}
