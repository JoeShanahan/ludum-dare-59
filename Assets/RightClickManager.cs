using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class RightClickManager : MonoBehaviour
{
    private InputSystem_Actions _input;
    private Camera _cam;

    [SerializeField]
    private LayerMask _hexLayer;

    [SerializeField]
    private GameObject _robotMenuPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _input = new InputSystem_Actions();
        _input.Enable();
        _cam = Camera.main;
        _input.UI.RightClick.performed += RightClickPerformed;
    }

    void RightClickPerformed(CallbackContext ctx)
    {
        if (ctx.control.IsPressed() == false)
            return;
            
        Ray worldRay = _cam.ScreenPointToRay(Mouse.current.position.value);
        if (Physics.Raycast(worldRay, out RaycastHit hit, 999, _hexLayer))
        {
            if (hit.collider.gameObject.TryGetComponent(out MapHex hex))
            {
                if (hex.ObjectOnTop == null)
                    return;

                if (hex.ObjectOnTop is MergeObject mergeObj)
                {
                    RightClickOnMergeObject(mergeObj);
                }
            }
        }
    }

    private void RightClickOnMergeObject(MergeObject mergeObj)
    {
        if (mergeObj.Data.RobotValues.IsEnabled)
        {
            var cmenu = W2C.InstantiateAs<RobotContextMenu>(_robotMenuPrefab);
            cmenu.InitAsleep(mergeObj);
        }
    }
}
