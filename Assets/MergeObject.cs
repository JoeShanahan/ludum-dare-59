using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeObject : MonoBehaviour
{
    public MergeObjectData Data;

    [SerializeField]
    private MapHex _currentHex;

    public MapHex CurrentHex => _currentHex;

    public void SetCurrentHex(MapHex hex)
    {
        _currentHex = hex;
    }

    public void KillCoroutines()
    {
        StopAllCoroutines();
    }

    public void GoToCurrentHex()
    {
        StopAllCoroutines();
        StartCoroutine(MoveToHex());
    }

    private IEnumerator MoveToHex()
    {
        Vector3 pos = CurrentHex.transform.position;

        while (Vector3.Distance(pos, transform.position) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * 13);
            yield return null;
        }

        transform.position = pos;
    }
}
