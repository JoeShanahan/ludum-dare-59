using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class HexGrid : MonoBehaviour
{
    public event Action OnSelectionChange;

    [SerializeField]
    private LayerMask _hexLayer;

    [SerializeField]
    private Transform _highlightObj;

    [SerializeField]
    private Transform[] _blueHighlights;

    [SerializeField]
    private TextAsset _levelJson;

    [SerializeField]
    private Transform _hexParent;

    [SerializeField]
    private Transform _objParent;

    [SerializeField]
    private HexType[] _typeLookup;

    [SerializeField]
    private MergeFamilyData[] _allFamilies;

    private Vector3 _minPos = new Vector3(999, 0, 999);
    private Vector3 _maxPos = new Vector3(-999, 0, -999);

    private InputSystem_Actions _actions;

    private MapHex _highlightHex;
    private MapHex _overHex;

    private Camera _cam;

    private MergeObject _draggingObject;

    public MapHex OverHex => _overHex;
    public MergeObject DraggingObject => _draggingObject;
    private List<MapHex> _allHexes;

    public IEnumerable<MapHex> GetAllFogTiles()
    {
        foreach (MapHex hex in _allHexes)
        {
            if (hex.CanBeDefogged)
            {
                yield return hex;
            }
        }
    }

    private void SingleTileFlipped()
    {
        RefreshFogStatus();
    }

    public void ClampCamera(Transform followParent)
    {
        float x = Mathf.Clamp(followParent.position.x, _minPos.x, _maxPos.x);
        float z = Mathf.Clamp(followParent.position.z, _minPos.z, _maxPos.z);
        followParent.transform.position = new Vector3(x, 0, z);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnMap();

        _cam = Camera.main;
        _actions = new();
        _actions.Enable();
        _actions.UI.Click.performed += ClickPerformed;

        HideAllHighlights();
    }

    private void SpawnMap()
    {
        _allHexes = new();
        Dictionary<string, HexType> hexLookup = new();
        Dictionary<string, MergeObjectData> objLookup = new();

        foreach (HexType ht in _typeLookup)
        {
            hexLookup[ht.TileCharacter] = ht;
        }

        foreach (MergeFamilyData fam in _allFamilies)
        {
            foreach (MergeObjectData mer in fam.Objects)
            {
                objLookup[mer.name] = mer;
            }
        }


        _minPos = new Vector3(999, 0, 999);
        _maxPos = new Vector3(-999, 0, -999);

        LevelJson lvl = JsonUtility.FromJson<LevelJson>(_levelJson.text);

        foreach (TileInfo ti in lvl.GetTiles())
        {
            hexLookup.TryGetValue(ti.Material, out HexType ht);
            GameObject newTile = Instantiate(ht.Prefab, _hexParent);
            newTile.transform.localPosition = new Vector3(ti.Position.x * 2, 0, -ti.Position.y * 1.5f);

            if (ti.Position.y % 2 == 1)
            {
                newTile.transform.localPosition += new Vector3(1, 0, 0);            
            }

            var mhex = newTile.GetComponent<MapHex>();
            mhex.SetStartingState(ti.Fog);
            mhex.OnTileFlip += SingleTileFlipped;
            _allHexes.Add(mhex);

            _minPos.x = Mathf.Min(_minPos.x, newTile.transform.localPosition.x);
            _minPos.z = Mathf.Min(_minPos.z, newTile.transform.localPosition.z);

            _maxPos.x = Mathf.Max(_maxPos.x, newTile.transform.localPosition.x);
            _maxPos.z = Mathf.Max(_maxPos.z, newTile.transform.localPosition.z);

            if (ti.ObjectOnTop != null && objLookup.TryGetValue(ti.ObjectOnTop, out MergeObjectData mo))
            {
                GameObject newMo = Instantiate(mo.Prefab, _objParent);
                newMo.transform.localPosition = newTile.transform.localPosition;
                MergeObject merObj = newMo.GetComponent<MergeObject>();
                mhex.SetObject(merObj);
                merObj.SetCurrentHex(mhex);
            }
        }

        foreach (MapHex hex in _allHexes)
        {
            hex.FindNeighbours(_allHexes);
        }

        foreach (MapHex hex in _allHexes)
        {
            hex.DetermineFogStatus();
        }

        foreach (MergeObject obj in transform.GetComponentsInChildren<MergeObject>())
        {
            foreach (MapHex hex in _allHexes)
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

    private void RefreshFogStatus()
    {
        foreach (MapHex hex in _allHexes)
        {
            hex.DetermineFogStatus();
        }
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
        OnSelectionChange?.Invoke();
    }

    private void ClickReleased()
    {
        if (_draggingObject == null)
            return;

        bool isFree = _overHex.ObjectOnTop == _draggingObject || _overHex.ObjectOnTop == null;
        bool isInvalid = _overHex.Data.IsWater;

        if (!isInvalid && _overHex.CurrentFog > 0)
        {
            if (_overHex.CanBeDefogged && _draggingObject.Data.CanDispelFog)
            {
                int remainder = Mathf.Max(_draggingObject.Data.DataVals.DataValue - _overHex.CurrentFog, 0);
                _overHex.RemoveFog(_draggingObject.Data.DataVals.DataValue);
                _draggingObject.CurrentHex.SetObject(null);
                _draggingObject.DoSpendData(_overHex, 0.25f);
                _draggingObject = null;
                OnSelectedHexChange(_overHex);
                HideAllHighlights();
                return;
            }

            isInvalid = true;
        }

        if (isInvalid)
        {
            _draggingObject.GoToCurrentHex();
            _draggingObject = null;
            OnSelectedHexChange(_overHex);
            HideAllHighlights();
            return;
        }
        else if (isFree)
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

        OnSelectionChange?.Invoke();
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

        if (_overHex.CurrentFog > 0)
            return result;

        if (_overHex.Data.IsWater)
        {
            return result;
        }

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
}
