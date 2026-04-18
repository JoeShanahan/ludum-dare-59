using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMover : MonoBehaviour
{
    [SerializeField]
    private HexGrid _grid;

    [SerializeField]
    private float _moveSpeed = 4;

    [SerializeField]
    private float _gravity = 4;

    [SerializeField]
    private Transform _closeTransform;

    [SerializeField]
    private Transform _farTransform;

    private Camera _cam;

    private float _currentZoom = 0.0f;
    private float _targetZoom = 0.0f;

    private InputSystem_Actions _input;
    private Vector3 _velocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _input = new();
        _input.Enable();    
        _input.Player.Zoom.performed += ZoomPerformed;
        _cam = Camera.main;
    }

    private void ZoomPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.control.IsPressed())
        {
            _targetZoom += _input.Player.Zoom.ReadValue<float>() * 0.125f;
            _targetZoom = Mathf.Clamp01(_targetZoom);
        }
    }

    // Update is called once per frame
    void Update()
    {
        var move = _input.Player.Move.ReadValue<Vector2>();

        Vector3 moveVec = new Vector3(move.x * _moveSpeed * Time.deltaTime, 0, move.y * _moveSpeed * Time.deltaTime);
        _velocity = Vector3.Lerp(_velocity, moveVec, Time.deltaTime * _gravity);

        transform.position += _velocity;
        _grid.ClampCamera(transform);

        _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, Time.deltaTime * 8);
        _cam.transform.position = Vector3.Lerp(_farTransform.position, _closeTransform.position, _currentZoom);
    }
}
