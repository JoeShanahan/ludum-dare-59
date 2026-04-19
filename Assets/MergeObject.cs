using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MergeObject : MonoBehaviour
{
    public MergeObjectData Data;

    [SerializeField]
    private MapHex _currentHex;

    public MapHex CurrentHex => _currentHex;

    private bool _isKilled;

    public void SetCurrentHex(MapHex hex)
    {
        _currentHex = hex;
    }

    public void KillCoroutines()
    {
        StopAllCoroutines();
    }

    public void DoMerge(MapHex target, float time)
    {
        _isKilled = true;
        KillCoroutines();
        transform.DOMove(target.transform.position, time).SetEase(Ease.OutSine).OnComplete(() => Destroy(gameObject));
    }

    public void DoSpendData(MapHex target, float time)
    {
        _isKilled = true;
        KillCoroutines();
        transform.DOMove(target.transform.position, time).SetEase(Ease.OutSine).OnComplete(() => Destroy(gameObject));
        transform.DOScale(0, time).SetEase(Ease.OutSine);
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
}
