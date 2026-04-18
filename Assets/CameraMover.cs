using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [SerializeField]
    private HexGrid _grid;

    [SerializeField]
    private float _moveSpeed = 4;

    [SerializeField]
    private float _gravity = 4;

    private InputSystem_Actions _input;
    private Vector3 _velocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _input = new();
        _input.Enable();    
    }

    // Update is called once per frame
    void Update()
    {
        var move = _input.Player.Move.ReadValue<Vector2>();

        Vector3 moveVec = new Vector3(move.x * _moveSpeed * Time.deltaTime, 0, move.y * _moveSpeed * Time.deltaTime);
        _velocity = Vector3.Lerp(_velocity, moveVec, Time.deltaTime * _gravity);

        transform.position += _velocity;
        _grid.ClampCamera(transform);
    }
}
