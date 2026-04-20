using UnityEngine;
using UnityEngine.UI;



public class RobotContextMenu : W2C
{
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
        newObj.transform.position = _sleepingObj.transform.position;
        Destroy(_sleepingObj.gameObject);
        _sleepingObj = null;
        SetPosition(_awakeObj.transform);

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_awakeObj != null)
        {
            _titleText.text = _awakeObj.GetStatus();
        }
        else if (_sleepingObj != null)
        {
            _titleText.text = $"{_sleepingObj.Data.ObjectName} (asleep)";
        }
    }
}
