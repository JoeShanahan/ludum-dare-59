using UnityEngine;

[CreateAssetMenu]
public class HexType : ScriptableObject
{
    public string TileName;
    public string TileCharacter;
    public Material Material;
    public GameObject Prefab;
    public bool IsWater;
}
