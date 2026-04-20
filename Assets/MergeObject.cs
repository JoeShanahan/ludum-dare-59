using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MergeObject : HexObject
{
    public MergeObjectData Data;

    public RobotBase Reservation;

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

    public override IEnumerable<(string, string)> GetInfo()
    {
        foreach ((string k, string v) in Data.InfoForUI())
        {
            yield return (k, v);
        }
    }
}
