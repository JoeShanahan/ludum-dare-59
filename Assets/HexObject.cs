using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HexObject : MonoBehaviour
{
    public MapHex CurrentHex => _currentHex;

    public virtual bool CanBeMoved => true;

    [SerializeField]
    protected MapHex _currentHex;

    protected bool _isKilled;

    public void SetCurrentHex(MapHex hex)
    {
        _currentHex = hex;
    }

    public void GoToCurrentHex(float delay=0.0f)
    {
        if (_isKilled)
            return;

        StopAllCoroutines();
        StartCoroutine(MoveToHex(delay));
    }

    private IEnumerator MoveToHex(float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        Vector3 pos = CurrentHex.transform.position;

        while (Vector3.Distance(pos, transform.position) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * 13);
            yield return null;
        }

        transform.position = pos;
    }

    public void KillCoroutines()
    {
        StopAllCoroutines();
    }
}