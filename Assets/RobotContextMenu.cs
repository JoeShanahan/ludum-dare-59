using UnityEngine;
using UnityEngine.UI;

public class RobotContextMenu : W2C
{
    [SerializeField] private Text _titleText;

    private RobotBase _robot;
    [SerializeField]
    private MergeObject _obj;
    private MergeObjectData _data;

    private bool _isAwake;

    public void InitAsleep(MergeObject obj)
    {
        _isAwake = false;
        _obj = obj;
        _data = obj.Data;
        _titleText.text = obj.Data.ObjectName;
        SetPosition(obj.transform);
    }

    public void InitAwake(RobotBase robot)
    {
        _isAwake = false;
        _robot = robot;
        _data = robot.Data;
        _titleText.text = "Robot (awake)";
        SetPosition(robot.transform);
    }

    public void ButtonPress()
    {
        if (_isAwake)
        {
            
        }
        else
        {
            _obj.CurrentHex.SetObject(null);
            GameObject newObj = GameObject.Instantiate(_obj.Data.RobotValues.RobotPrefab, _obj.transform.parent);
            newObj.GetComponent<RobotBase>().WakeUp();
            newObj.transform.position = _obj.transform.position;
            Destroy(_obj.gameObject);
        }

        Destroy(gameObject);
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
