using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelJson
{
    public string[] Hexes;
    public string[] Fog;
    public string[] Objects;

    public IEnumerable<TileInfo> GetTiles()
    {
        if (Hexes.Length != Fog.Length)
        {
            Debug.LogError("Hexes and fog are different length!");
            yield break;
        }

        for (int i=0; i<Hexes.Length; i++)
        {
            if (Hexes[i].Length != Fog[i].Length)
            {
                Debug.LogError($"Line {i+1} mismatch in length!");
                yield break;
            }
        }

        Dictionary<(int, int), string> objectPositions = new();

        foreach (string objString in Objects)
        {
            string[] split = objString.Split(',');
            int row = int.Parse(split[0]);
            int column = int.Parse(split[1]);
            objectPositions[(column, row)] = split[2];
        }

        string abc = "0123456789ABCDEFGHIJKLMNOP";

        for (int row=0; row<Hexes.Length; row++)
        {
            string hexString = Hexes[row].Replace(" ", "");
            string fogString = Fog[row].Replace(" ", "");
            
            for (int column=0; column<hexString.Length; column++)
            {
                char hexChar = hexString[column];
                char fogChar = fogString[column];

                if (hexChar == '.')
                    continue;

                objectPositions.TryGetValue((column, row), out string objOnTop);

                yield return new TileInfo
                {
                    Position = new Vector2Int(column, row),
                    Fog = abc.IndexOf(fogChar),
                    Material = hexChar.ToString(),
                    ObjectOnTop = objOnTop,
                };
            }
        }
    }

}

public class TileInfo
{
    public Vector2Int Position;
    public int Fog;
    public string Material;
    public string ObjectOnTop;
}
