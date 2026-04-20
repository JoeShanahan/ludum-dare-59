using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ProducerObject : HexObject
{
    public ProducerData Data => _data;
    public override bool CanBeMoved => false;

    [SerializeField]
    private ProducerData _data;
}
