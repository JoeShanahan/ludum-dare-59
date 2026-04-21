using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;



public class RobotContextMenu : W2C
{
    public static RobotContextMenu SINGLETON;

    public MergeObject SleepingObj => _sleepingObj;
    public RobotBase AwakeObj => _awakeObj;

    [SerializeField] private Text _titleText;

    private RobotBase _awakeObj;

    [SerializeField]
    private MergeObject _sleepingObj;

    [SerializeField]
    private Text _statusText;

    [SerializeField]
    private RectTransform _chargeBar;

    [SerializeField]
    private RectTransform[] _asleepButtons;

    [SerializeField]
    private RectTransform[] _awakeButtons;


    private InputSystem_Actions _input;

    public void InitAsleep(MergeObject obj)
    {
        _sleepingObj = obj;
        _titleText.text = obj.Data.ObjectName;
        SetPosition(obj.transform);
        
        foreach (RectTransform t in _awakeButtons)
            t.gameObject.SetActive(false);
        
        foreach (RectTransform t in _asleepButtons)
            t.gameObject.SetActive(true);
    }

    public void InitAwake(RobotBase robot)
    {
        _awakeObj = robot;
        SetPosition(robot.transform);

        foreach (RectTransform t in _awakeButtons)
            t.gameObject.SetActive(true);
        
        foreach (RectTransform t in _asleepButtons)
            t.gameObject.SetActive(false);
    }

    public void ButtonPressPower()
    {
        if (_sleepingObj == null)
        {
            return;
        }

        _sleepingObj.CurrentHex.SetObject(null);
        GameObject newObj = GameObject.Instantiate(_sleepingObj.Data.RobotValues.RobotPrefab, _sleepingObj.transform.parent);
        _awakeObj = newObj.GetComponent<RobotBase>();
        _awakeObj.WakeUp();
        _awakeObj.SetPower(_sleepingObj.PowerStored);
        newObj.transform.position = _sleepingObj.transform.position;
        Destroy(_sleepingObj.gameObject);
        _sleepingObj = null;
        SetPosition(_awakeObj.transform);

        InitAwake(_awakeObj);

        foreach (RectTransform t in _awakeButtons)
            t.gameObject.SetActive(true);
        
        foreach (RectTransform t in _asleepButtons)
            t.gameObject.SetActive(false);
    }

    public void ButtonPressSleep()
    {
        if (_awakeObj == null)
        {
            return;
        }

        _awakeObj.StartSleepTask();
    }

    public void ButtonPressGo()
    {
        if (RobotBase.CURRENT_SELECTED == null)
        {
            _awakeObj.StartPickingTarget();
        }
        else
        {
            _awakeObj.CancelPickingTarget();
        }
    }

    public void ButtonPressClose()
    {
        Destroy(gameObject);
    }
    

    private void OnDestroy()
    {
        if (SINGLETON == this)
        {
            SINGLETON = null;
        }
        _input.Disable();
        _input.UI.Cancel.performed -= EscapePressed;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SINGLETON != null)
        {
            Destroy(SINGLETON.gameObject);
        }

        SINGLETON = this;
        _input = new();
        _input.Enable();
        _input.UI.Cancel.performed += EscapePressed;
    }

    private void EscapePressed(CallbackContext ctx)
    {
        if (ctx.control.IsPressed())
        {
            RobotBase.CURRENT_SELECTED = null;
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float chargePercent = 0;

        if (_awakeObj != null)
        {
            _titleText.text = _awakeObj.GetStatus();
            chargePercent = _awakeObj.ChargePercent / 100f;

        }
        else if (_sleepingObj != null)
        {
            _titleText.text = $"{_sleepingObj.Data.ObjectName} (asleep)";
            chargePercent = (float) _sleepingObj.PowerStored / _sleepingObj.Data.RobotValues.MaxCharge;
        }

        _chargeBar.localScale = new Vector3(chargePercent, 1, 1);
    }
}
