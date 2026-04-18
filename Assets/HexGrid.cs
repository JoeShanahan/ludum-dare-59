using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class HexGrid : MonoBehaviour
{
    [SerializeField]
    private LayerMask _hexLayer;

    [SerializeField]
    private Transform _highlightObj;

    [SerializeField]
    private Transform[] _blueHighlights;

    private InputSystem_Actions _actions;

    private MapHex _highlightHex;
    private MapHex _overHex;

    private Camera _cam;

    private MergeObject _draggingObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cam = Camera.main;
        InitMap();
        _actions = new();
        _actions.Enable();
        _actions.UI.Click.performed += ClickPerformed;

        HideAllHighlights();
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
        _draggingObject?.KillCoroutines();
        DetermineDropStatus();
    }

    private void ClickReleased()
    {
        if (_draggingObject == null)
            return;

        bool isFree = _overHex.ObjectOnTop == _draggingObject || _overHex.ObjectOnTop == null;

        if (isFree)
        {
            _draggingObject.CurrentHex.SetObject(null);
            _draggingObject.SetCurrentHex(null);

            _draggingObject.SetCurrentHex(_overHex);
            _overHex.SetObject(_draggingObject);
        }
        else
        {
            MapHex otherHex = _overHex;
            MapHex thisHex = _draggingObject.CurrentHex;

            if (otherHex != thisHex)
            {
                MergeObject otherObj = otherHex.ObjectOnTop;
                MergeObject thisObj = _draggingObject;
                
                otherHex.SetObject(thisObj);
                thisHex.SetObject(otherObj);

                otherObj.SetCurrentHex(thisHex);
                thisObj.SetCurrentHex(otherHex);
                
                otherObj.GoToCurrentHex();

            }
        }

        List<MapHex> mergableHexes = GetMergeableHexes();

        if (mergableHexes.Count > 2)
        {
            MergeObjectData newData = _draggingObject.Data.Next;

            if (newData != null)
            {
                foreach (MapHex hex in mergableHexes)
                {
                    MergeObject onTop = hex.ObjectOnTop;
                    hex.SetObject(null);
                    onTop.DoMerge(_overHex, 0.25f);
                }

                GameObject newObj = Instantiate(newData.Prefab, _draggingObject.transform.parent);
                newObj.transform.position = _overHex.transform.position;
                MergeObject newMergeObj = newObj.GetComponent<MergeObject>();
                newMergeObj.SetCurrentHex(_overHex);
                _overHex.SetObject(newMergeObj);
                newObj.transform.localScale = Vector3.zero;
                newObj.transform.DOScale(1, 0.25f).SetEase(Ease.OutExpo).SetDelay(0.2f);

                if (mergableHexes.Count == 5)
                {
                    GameObject newObj2 = Instantiate(newData.Prefab, _draggingObject.transform.parent);
                    newObj2.transform.position = _overHex.transform.position;
                    MergeObject newMergeObj2 = newObj2.GetComponent<MergeObject>();
                    newMergeObj2.SetCurrentHex(mergableHexes[1]);
                    mergableHexes[1].SetObject(newMergeObj2);
                    newMergeObj2.GoToCurrentHex(0.2f);

                    newObj2.transform.localScale = Vector3.zero;
                    newObj2.transform.DOScale(1, 0.25f).SetEase(Ease.OutExpo).SetDelay(0.2f);
                }
            }
        }

        _draggingObject.GoToCurrentHex();
        _draggingObject = null;
        OnSelectedHexChange(_overHex);
        HideAllHighlights();
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


    private void InitMap()
    {
        List<MapHex> allHexes = new();

        foreach (MapHex hex in transform.GetComponentsInChildren<MapHex>())
        {
            allHexes.Add(hex);
        }

        foreach (MapHex hex in allHexes)
        {
            hex.FindNeighbours(allHexes);
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
        else
        {
            DetermineDropStatus();
        }
    }

    private void DetermineDropStatus()
    {
        if (_draggingObject == null)
        {
            HideAllHighlights();
            return;
        }

        List<MapHex> mergableHexes = GetMergeableHexes();

        if (mergableHexes.Count > 2)
        {
            int top = Mathf.Min(mergableHexes.Count, _blueHighlights.Length);

            for (int i=0; i<top; i++)
            {
                _blueHighlights[i].transform.position = mergableHexes[i].transform.position;
                _blueHighlights[i].gameObject.SetActive(true);
            }

            for (int j=mergableHexes.Count; j<_blueHighlights.Length; j++)
            {
                _blueHighlights[j].gameObject.SetActive(false);
            }

            return;
        }

        HideAllHighlights();
        _blueHighlights[0].gameObject.SetActive(true);
        _blueHighlights[0].transform.position = _overHex.transform.position;
    }

    private void HideAllHighlights()
    {
        foreach (Transform t in _blueHighlights)
        {
            t.gameObject.SetActive(false);
        }
    }

    private List<MapHex> GetMergeableHexes()
    {
        var result = new List<MapHex>();
        var todo = new Queue<MapHex>();

        if (_draggingObject.Data.Next == null)
            return result;

        todo.Enqueue(_overHex);

        while (todo.Count > 0 && result.Count < 5)
        {
            var current = todo.Dequeue();
            result.Add(current);

            foreach (MapHex nbor in current.Neighbours)
            {            
                if (result.Contains(nbor) || todo.Contains(nbor) || nbor.ObjectOnTop == null)
                    continue;

                if (nbor == _draggingObject.CurrentHex && _overHex != _highlightHex)
                    continue;

                if (nbor.ObjectOnTop.Data != _draggingObject.Data)
                    continue;

                todo.Enqueue(nbor);
            }
        }

        if (result.Count == 4)
            result.RemoveAt(3);

        return result;
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
