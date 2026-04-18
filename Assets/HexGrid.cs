using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class HexGrid : MonoBehaviour
{
    [SerializeField]
    private LayerMask _hexLayer;

    [SerializeField]
    private Transform _highlightObj;

    private InputSystem_Actions _actions;

    private MapHex _highlightHex;
    private MapHex _overHex;

    private Camera _cam;

    private MergeObject _draggingObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cam = Camera.main;
        CollectObjects();
        _actions = new();
        _actions.Enable();
        _actions.UI.Click.performed += ClickPerformed;
    }

    private void ClickPerformed(CallbackContext ctx)
    {
        if (ctx.control.IsPressed())
        {
            ClickPressed();
        }
        else
        {
            ClickReleased();  
        }
    }

    private void ClickPressed()
    {
        if (_highlightHex == null || _highlightHex.ObjectOnTop == null)
            return;

        _draggingObject = _highlightHex.ObjectOnTop;
        StopAllCoroutines();
    }

    private void ClickReleased()
    {
        if (_draggingObject == null)
            return;

        StartCoroutine(MoveToHex(_draggingObject));
        _draggingObject = null;
        OnSelectedHexChange(_overHex);
    }

    // Update is called once per frame
    void Update()
    {
        Ray worldRay = _cam.ScreenPointToRay(Mouse.current.position.value);
        if (Physics.Raycast(worldRay, out RaycastHit hit, 999, _hexLayer))
        {
            if (hit.collider.gameObject.TryGetComponent(out MapHex hex))
            {
                if (hex != _overHex)
                {
                    OnSelectedHexChange(hex);
                }
            }
        }

        if (_draggingObject != null)
        {
            Vector3 tgtPos = _overHex.transform.position + Vector3.up;
            _draggingObject.transform.position = Vector3.Lerp(_draggingObject.transform.position, tgtPos, Time.deltaTime * 13);
        }
    }

    private IEnumerator MoveToHex(MergeObject mergeObj)
    {
        Vector3 pos = mergeObj.CurrentHex.transform.position;

        while (Vector3.Distance(pos, mergeObj.transform.position) > 0.01f)
        {
            mergeObj.transform.position = Vector3.Lerp(mergeObj.transform.position, pos, Time.deltaTime * 13);
            yield return null;
        }

        mergeObj.transform.position = pos;
    }

    private void CollectObjects()
    {
        List<MapHex> allHexes = new();

        foreach (MapHex hex in transform.GetComponentsInChildren<MapHex>())
        {
            allHexes.Add(hex);
        }

        foreach (MergeObject obj in transform.GetComponentsInChildren<MergeObject>())
        {
            foreach (MapHex hex in allHexes)
            {
                float dist = Vector3.Distance(hex.transform.position, obj.transform.position);

                if (dist < 0.5f)
                {
                    hex.SetObject(obj);
                    obj.SetCurrentHex(hex);
                    break;
                }
            }
        }
    }

    private void OnSelectedHexChange(MapHex newHex)
    {
        _overHex = newHex;

        if (_draggingObject == null)
        {
            _highlightHex = newHex;
            _highlightObj.position = _highlightHex.transform.position;
        }
    }

    [ContextMenu("SYNC ALL")]
    public void SyncAllTiles()
    {
        foreach (MapHex hex in transform.GetComponentsInChildren<MapHex>())
        {
            hex.SyncVisuals();
        }
    }
}
