using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WorldToCanvas;

public class HexDataCostOverlay : MonoBehaviour
{
    [SerializeField]
    private GameObject _prefab;

    [SerializeField]
    private HexGrid _grid;

    [SerializeField]
    private CanvasGroup _grp;

    private List<HexDataCostUI> _spawnedChildren = new();
    private MergeObject _heldObject;

    private void Start()
    {
        _grid.OnSelectionChange += SelectionChanged;
    }

    private void SelectionChanged()
    {
        if (_heldObject == _grid.DraggingObject)
            return;

        _heldObject = _grid.DraggingObject;

        if (_heldObject == null || _heldObject.Data.CanDispelFog == false)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    public void Show()
    {
        foreach (var costUi in _spawnedChildren)
        {
            Destroy(costUi.gameObject);
        }

        DOTween.Kill(_grp);
        _grp.alpha = 0;
        _grp.DOFade(1, 0.5f).SetEase(Ease.OutExpo);
        gameObject.SetActive(true);
        _spawnedChildren.Clear();

        foreach (MapHex hex in _grid.GetAllFogTiles())
        {
            HexDataCostUI hd = W2CManager.InstantiateAs<HexDataCostUI>(_prefab, transform);
            _spawnedChildren.Add(hd);
            hd.SetHex(hex);
        }
    }

    public void Hide()
    {
        _grp.DOFade(0, 0.5f).OnComplete(() => gameObject.SetActive(false));
    }
}
