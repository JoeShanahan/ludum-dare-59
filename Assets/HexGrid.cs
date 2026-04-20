using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class HexGrid : MonoBehaviour
{
    public event Action OnSelectionChange;

    [SerializeField]
    private bool _forceAllFogOff;

    [SerializeField]
    private LayerMask _hexLayer;

    [SerializeField]
    private Transform _highlightObj;

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
    
    [SerializeField]
    private ProducerData[] _allProducers;

    [SerializeField]
    private GameObject _dataParticlePrefab;


    [Header("The highlights")]
    [SerializeField]
    private MeshRenderer[] _highlightHexes;

    [SerializeField]
    private Material[] _validMats;

    [SerializeField]
    private Material[] _mergeMats;

    [SerializeField]
    private Material[] _invalidMats;


    private Vector3 _minPos = new Vector3(999, 0, 999);
    private Vector3 _maxPos = new Vector3(-999, 0, -999);

    private InputSystem_Actions _actions;

    private MapHex _highlightHex;
    private MapHex _overHex;

    private Camera _cam;

    private HexObject _draggingObject;

    public MapHex OverHex => _overHex;
    public HexObject DraggingObject => _draggingObject;
    private List<MapHex> _allHexes;

    public IEnumerable<MapHex> AllTiles => _allHexes;

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

    public IEnumerable<MapHex> GetAllFreeTiles()
    {
        foreach (MapHex hex in _allHexes)
        {
            if (hex.CurrentFog > 0)
                continue;

            if (hex.ObjectOnTop != null)
                continue;

            if (hex.Data.IsWater)
                continue;
            
            yield return hex;
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

    private void SetHexColors(Material[] mats)
    {
        foreach (MeshRenderer mr in _highlightHexes)
        {
            if (mr.sharedMaterials[0] == mats[0])
                continue;

            mr.sharedMaterials = mats;
        }
    }

    private void SpawnMap()
    {
        _allHexes = new();
        Dictionary<string, HexType> hexLookup = new();
        Dictionary<string, MergeObjectData> objLookup = new();
        Dictionary<string, ProducerData> prodLookup = new();

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

        foreach (ProducerData d in _allProducers)
        {
            prodLookup[d.name] = d;
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
            mhex.SetStartingState(_forceAllFogOff ? 0 : ti.Fog);
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
                mhex.SetObjectInit(merObj);
                merObj.SetCurrentHex(mhex);
            }
            else if (ti.ObjectOnTop != null && prodLookup.TryGetValue(ti.ObjectOnTop, out ProducerData pd))
            {
                GameObject newMo = Instantiate(pd.Prefab, _objParent);
                newMo.transform.localPosition = newTile.transform.localPosition;
                ProducerObject prodObj = newMo.GetComponent<ProducerObject>();
                mhex.SetObjectInit(prodObj);
                prodObj.SetCurrentHex(mhex);
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
            if (EventSystem.current.IsPointerOverGameObject() == false)
                ClickPressed();
        }
        else
        {
            ClickReleased();  
        }
    }

    private void ClickPressed()
    {
        if (_highlightHex == null || _highlightHex.ObjectOnTop == null || _highlightHex.CurrentFog > 0)
            return;

        if (RobotBase.CURRENT_SELECTED != null)
        {
            RobotBase.CURRENT_SELECTED.PickTarget(_highlightHex.ObjectOnTop);
            return;
        }

        if (_highlightHex.ObjectOnTop.CanBeMoved == false)
            return;

        _draggingObject = _highlightHex.ObjectOnTop;
        _draggingObject?.KillCoroutines();
        DetermineDropStatus();
        OnSelectionChange?.Invoke();
    }

    private void SendData(MapHex sourceHex, int remainder)
    {
        if (remainder == 0)
            return;

        List<MapHex> potentialHexes = new(GetAllFogTiles());
        potentialHexes.OrderBy(h => Vector3.Distance(h.transform.position, sourceHex.transform.position));
        potentialHexes.Remove(sourceHex);

        while (remainder > 0 && potentialHexes.Count > 0)
        {
            MapHex hex0 = potentialHexes[0];
            potentialHexes.RemoveAt(0);

            int toSend = Mathf.Min(hex0.CurrentFog, remainder);

            for (int i=0; i<toSend; i++)
            {
                GameObject newObj = Instantiate(_dataParticlePrefab);
                newObj.GetComponent<DataParticle>().Init(sourceHex, hex0);
            }

            remainder -= toSend;
        }
    }

    private void ClickReleased()
    {
        if (_draggingObject == null)
            return;

        bool isFree = _overHex.ObjectOnTop == _draggingObject || _overHex.ObjectOnTop == null;
        bool isInvalid = _overHex.Data.IsWater;
        MergeObject mergeObj = _draggingObject as MergeObject;

        if (!isInvalid && _overHex.CurrentFog > 0)
        {

            if (mergeObj != null && _overHex.CanBeDefogged && mergeObj.Data.CanDispelFog)
            {
                int remainder = Mathf.Max(mergeObj.Data.DataVals.DataValue - _overHex.CurrentFog, 0);
                SendData(_overHex, remainder);

                _overHex.RemoveFog(mergeObj.Data.DataVals.DataValue);
                _draggingObject.CurrentHex.SetObject(null);
                mergeObj.DoSpendData(_overHex, 0.25f);
                _draggingObject = null;
                OnSelectedHexChange(_overHex);
                HideAllHighlights();
                return;
            }

            isInvalid = true;
        }

        if (_overHex.ObjectOnTop != null && _overHex.ObjectOnTop.CanBeMoved == false)
            isInvalid = true;

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
                HexObject otherObj = otherHex.ObjectOnTop;
                HexObject thisObj = _draggingObject;
                
                otherHex.SetObject(thisObj);
                thisHex.SetObject(otherObj);

                otherObj.SetCurrentHex(thisHex);
                thisObj.SetCurrentHex(otherHex);
                
                otherObj.GoToCurrentHex();
            }
        }

        List<MapHex> mergableHexes = GetMergeableHexes();

        if (mergeObj != null && mergableHexes.Count > 2)
        {
            MergeObjectData newData = mergeObj.Data.Next;

            if (newData != null)
            {
                foreach (MapHex hex in mergableHexes)
                {
                    MergeObject onTop = hex.ObjectOnTop as MergeObject;
                    hex.SetObject(null);
                    onTop.DoMerge(_overHex, 0.25f);
                }

                GameObject newObj = Instantiate(newData.Prefab, _draggingObject.transform.parent);
                newObj.transform.position = _overHex.transform.position;
                MergeObject newMergeObj = newObj.GetComponent<MergeObject>();
                newMergeObj.OnNewlyCreated();
                newMergeObj.SetCurrentHex(_overHex);
                _overHex.SetObject(newMergeObj);
                newObj.transform.localScale = Vector3.zero;
                newObj.transform.DOScale(1, 0.25f).SetEase(Ease.OutExpo).SetDelay(0.2f);

                if (mergableHexes.Count == 5)
                {
                    GameObject newObj2 = Instantiate(newData.Prefab, _draggingObject.transform.parent);
                    newObj2.transform.position = _overHex.transform.position;
                    MergeObject newMergeObj2 = newObj2.GetComponent<MergeObject>();
                    newMergeObj2.OnNewlyCreated();

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
        if (EventSystem.current.IsPointerOverGameObject() == false)
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
        }
        
        if (_draggingObject != null)
        {
            Vector3 tgtPos = _overHex.transform.position + Vector3.up;
            _draggingObject.transform.position = Vector3.Lerp(_draggingObject.transform.position, tgtPos, Time.deltaTime * 13);
        }
    }

    public void SelectHexButOnlyUseThisOnFirstStartupPlease(MapHex newHex)
    {
        OnSelectedHexChange(newHex);
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

        SetHexColors(_validMats);
        List<MapHex> mergableHexes = GetMergeableHexes();

        if (mergableHexes.Count > 2)
        {
            int top = Mathf.Min(mergableHexes.Count, _highlightHexes.Length);

            for (int i=0; i<top; i++)
            {
                _highlightHexes[i].transform.position = mergableHexes[i].transform.position;
                _highlightHexes[i].gameObject.SetActive(true);
            }

            for (int j=mergableHexes.Count; j<_highlightHexes.Length; j++)
            {
                _highlightHexes[j].gameObject.SetActive(false);
            }

            SetHexColors(_mergeMats);
            return;
        }

        HideAllHighlights();
        _highlightHexes[0].gameObject.SetActive(true);
        _highlightHexes[0].transform.position = _overHex.transform.position;
    }

    private void HideAllHighlights()
    {
        foreach (MeshRenderer mr in _highlightHexes)
        {
            mr.gameObject.SetActive(false);
        }
    }

    private List<MapHex> GetMergeableHexes()
    {
        var result = new List<MapHex>();
        var todo = new Queue<MapHex>();
        var dragObj = _draggingObject as MergeObject;

        if (_overHex.CurrentFog > 0)
        {
            if (dragObj != null && dragObj.Data.DataVals.IsEnabled == false)
                SetHexColors(_invalidMats);
                
            return result;
        }

        if (_overHex.ObjectOnTop != null && _overHex.ObjectOnTop.CanBeMoved == false)
        {
            SetHexColors(_invalidMats);
            return result;
        }


        if (dragObj == null)
            return result;

        if (_overHex.Data.IsWater)
        {
            SetHexColors(_invalidMats);
            return result;
        }

        if (dragObj.Data.Next == null)
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

                MergeObject nborMerge = nbor.ObjectOnTop as MergeObject;

                if (nborMerge == null || nborMerge.Data != dragObj.Data)
                    continue;

                if (nbor.CurrentFog > 0)
                    continue;

                todo.Enqueue(nbor);
            }
        }

        if (result.Count == 4)
            result.RemoveAt(3);

        return result;
    }
}
