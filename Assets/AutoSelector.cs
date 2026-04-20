using UnityEngine;

// Selects the nearest hex on startup
[DefaultExecutionOrder(+500)]
public class AutoSelector : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float smallest = 999;
        MapHex best = null;

        HexGrid hg = FindFirstObjectByType<HexGrid>();
        foreach (MapHex mh in hg.AllTiles)
        {
            float dist = Vector3.Distance(transform.position, mh.transform.position);

            if (dist < smallest)
            {
                smallest = dist;
                best = mh;
            }
        }

        hg.SelectHexButOnlyUseThisOnFirstStartupPlease(best);
    }
}
