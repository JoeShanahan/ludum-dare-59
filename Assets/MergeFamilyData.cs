using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MergeFamilyData : ScriptableObject
{
    public string FamilyName;
    public List<MergeObjectData> Objects;
}
