using DG.Tweening;
using UnityEngine;

public class BoxRobot : RobotBase
{
    [SerializeField] private Transform _mainBody;
    [SerializeField] private Transform _arialA;
    [SerializeField] private Transform _arialB;
    [SerializeField] private Transform _leftWheel;
    [SerializeField] private Transform _rightWheel;
    [SerializeField] private float _turnSpeed = 1;
    [SerializeField] private float _moveSpeed = 1;

    private void Update()
    {
        
    }

    private void Rotate(float amount)
    {
        float frameAmount = amount * Time.deltaTime;
        transform.Rotate(0, frameAmount, 0);

        _rightWheel.Rotate(-2 * frameAmount, 0, 0);
        _leftWheel.Rotate(-2 * frameAmount, 0, 0);
    }

    private void MoveForwards(float amount)
    {
        float linearDistance = Time.deltaTime * amount;
        transform.position += transform.forward * linearDistance;
        _rightWheel.Rotate(linearDistance * 150, 0, 0);
        _leftWheel.Rotate(linearDistance * -150, 0, 0);
    }

    public override void MoveTo(MapHex hex)
    {
        
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
