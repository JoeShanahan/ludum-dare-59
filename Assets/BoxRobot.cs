using DG.Tweening;
using UnityEngine;

public class BoxRobot : RobotBase
{
    [SerializeField] private Transform _mainBody;
    [SerializeField] private Transform _arialA;
    [SerializeField] private Transform _arialB;
    [SerializeField] private Transform _leftWheel;
    [SerializeField] private Transform _rightWheel;


    protected override void Rotate(float amount)
    {
        transform.Rotate(0, amount, 0);

        _rightWheel.Rotate(-2 * amount, 0, 0);
        _leftWheel.Rotate(-2 * amount, 0, 0);
    }

    protected override void MoveForwards(float amount)
    {
        transform.position += transform.forward * amount;
        _rightWheel.Rotate(amount * 150, 0, 0);
        _leftWheel.Rotate(amount * -150, 0, 0);
    }

    public override void WakeUp()
    {
        _mainBody.DOLocalRotate(new Vector3(0, 0, 0), 0.4f).SetEase(Ease.OutBack);
        _arialA.DOLocalMoveZ(1.45f, 0.2f).SetDelay(0.5f);
        _arialB.DOLocalMoveZ(0.7f, 0.15f).SetDelay(0.65f);
    }

    public override void GoToSleep()
    {
        _mainBody.DOLocalRotate(new Vector3(-22, 0, 0), 0.4f).SetEase(Ease.OutBack).SetDelay(0.3f);
        _arialA.DOLocalMoveZ(0.75f, 0.2f).SetDelay(0.1f);
        _arialB.DOLocalMoveZ(0.1f, 0.15f);
    }
}
