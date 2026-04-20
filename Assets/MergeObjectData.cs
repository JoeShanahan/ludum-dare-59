using System;
using System.Collections.Generic;
using MergeObjectSubData;
using UnityEngine;

[CreateAssetMenu]
public class MergeObjectData : ScriptableObject
{
    public string ObjectName;
    public int Level;
    public MergeObjectData Next;
    public MergeFamilyData Family;
    public GameObject Prefab;
    public bool CanDispelFog => DataVals.IsEnabled && DataVals.DataValue > 0;

    [TextArea(1,5)]
    public string Description;

    [Space(16)]
    public DataValues DataVals;
    public RobotValues RobotValues;

    public IEnumerable<(string, string)> InfoForUI()
    {
        if (DataVals.IsEnabled)
        {
            foreach (var s in DataVals.InfoForUI())
                yield return s;
        }

        if (RobotValues.IsEnabled)
        {
            foreach (var s in RobotValues.InfoForUI())
                yield return s;
        }
    }
}

namespace MergeObjectSubData
{
    [Serializable]
    public class DataValues
    {
        public bool IsEnabled;
        public int DataValue;

        public IEnumerable<(string, string)> InfoForUI()
        {
            yield return ("Data", DataValue.ToString());
        }
    }

    [Serializable]
    public class RobotValues
    {
        public bool IsEnabled;
        public int MaxCharge;
        public int DischargeRate;
        public GameObject RobotPrefab;

        public IEnumerable<(string, string)> InfoForUI()
        {
            yield return ("Max Charge", $"{MaxCharge}");
            yield return ("Discharge", $"{DischargeRate} per sec");
        }
    }

    [Serializable]
    public class PowerValues
    {
        public bool IsEnabled;
        public int PerMinute;

        public IEnumerable<(string, string)> InfoForUI()
        {
            yield return ("Charge Rate", $"{PerMinute} per min");
        }
    }
}