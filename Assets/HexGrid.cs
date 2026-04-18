using UnityEngine;
using UnityEngine.InputSystem;

public class HexGrid : MonoBehaviour
{
    [SerializeField]
    private LayerMask _hexLayer;

    [SerializeField]
    private Transform _highlightObj;

    private MapHex _currentHex;

    private Camera _cam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Ray worldRay = _cam.ScreenPointToRay(Mouse.current.position.value);
        if (Physics.Raycast(worldRay, out RaycastHit hit, 999, _hexLayer))
        {
            if (hit.collider.gameObject.TryGetComponent(out MapHex hex))
            {
                _currentHex = hex;
                _highlightObj.position = _currentHex.transform.position;
            }
        }
    }

    [ContextMenu("SYNC ALL")]
    public void SyncAllTiles()
    {
        foreach (Transform t in transform)
        {
            if(t.TryGetComponent(out MapHex hex))
            {
                hex.SyncVisuals();
            }
        }
    }
}
