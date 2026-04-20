using System;
using System.Collections.Generic;
using MergeObjectSubData;
using UnityEngine;

[CreateAssetMenu]
public class ProducerData : ScriptableObject
{
    public string ObjectName;
    public GameObject Prefab;

    [TextArea(1,5)]
    public string Description;

    public int PowerRequirement;
    public MergeObjectData Produces;
    public int MaxProduction;
}
