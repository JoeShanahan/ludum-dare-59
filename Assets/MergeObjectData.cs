using UnityEngine;

[CreateAssetMenu]
public class MergeObjectData : ScriptableObject
{
    public string ObjectName;
    public int Level;
    public MergeObjectData Next;
    public MergeFamilyData Family;
    public GameObject Prefab;
}
