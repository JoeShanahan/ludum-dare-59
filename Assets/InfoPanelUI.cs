using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelUI : MonoBehaviour
{
    [SerializeField] private HexGrid _grid;
    [SerializeField] private Text _titleText;
    [SerializeField] private Text _familyText;
    [SerializeField] private Text _descriptionText;
    [SerializeField] private Text _labelsText;
    [SerializeField] private Text _valuesText;

    private HexObject _overObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _grid.OnSelectionChange += GridSelectionChanged;
    }

    private void GridSelectionChanged()
    {
        if (_grid.OverHex == null)
        {
            _titleText.text = "???";
            _familyText.text = "";
            _descriptionText.text = "";
            _labelsText.text = "";
            _valuesText.text = "";
            return;
        }

        List<string> labels = new();
        List<string> values = new();

        HexObject hexObj = _grid.DraggingObject;
        hexObj ??= _grid.OverHex.ObjectOnTop;
        _overObject = _grid.OverHex.ObjectOnTop;

        if (hexObj == null)
        {
            _titleText.gameObject.SetActive(false);
            _familyText.gameObject.SetActive(false);
            _descriptionText.gameObject.SetActive(false);
        }
        else if (hexObj is MergeObject mergeObj)
        {
            _titleText.gameObject.SetActive(true);
            _familyText.gameObject.SetActive(true);
            _descriptionText.gameObject.SetActive(true);

            _titleText.text = mergeObj.Data.ObjectName;
            _familyText.text = $"{mergeObj.Data.Family.FamilyName} Level {mergeObj.Data.Level}";
            _descriptionText.text = mergeObj.Data.Description;

            foreach ((string k, string v) in mergeObj.GetInfo())
            {
                labels.Add(k);
                values.Add(v);
            }
        }
        else if (hexObj is ProducerObject prodObj)
        {
            _titleText.gameObject.SetActive(true);
            _familyText.gameObject.SetActive(true);
            _descriptionText.gameObject.SetActive(true);

            _titleText.text = prodObj.Data.ObjectName;
            _familyText.text = "Cannot be moved";
            _descriptionText.text = prodObj.Data.Description;

            foreach ((string k, string v) in prodObj.GetInfo())
            {
                labels.Add(k);
                values.Add(v);
            }
        }

        labels.Add("Tile");

        if (_grid.OverHex.CurrentFog <= 0)
        {
            values.Add(_grid.OverHex.Data.TileName);
        }
        else
        {
            values.Add("???");

            if (_grid.OverHex.CanBeDefogged)
            {
                labels.Add("Reveal");
                values.Add($"{_grid.OverHex.CurrentFog} Data");
            }
        }

        _labelsText.text = string.Join('\n', labels);
        _valuesText.text = string.Join('\n', values);
    }

    public void Update()
    {
        if (_grid.OverHex != null && _grid.OverHex.ObjectOnTop != _overObject)
        {
            Debug.LogWarning("HEy that thing happened!");
            // Something changed about the current hex while we were hovering over it - probably a producer depositing here
            GridSelectionChanged();
        }

        RefreshDynamicValues();
    }

    private void RefreshDynamicValues()
    {
        if (_overObject == null)
            return;

        // only need to do this for producers
        if (_overObject is MergeObject)
            return;

        List<string> labels = new();
        List<string> values = new();

        if (_overObject is ProducerObject prodObj)
        {
            foreach ((string k, string v) in prodObj.GetInfo())
            {
                labels.Add(k);
                values.Add(v);
            }
        }

        labels.Add("Tile");

        if (_grid.OverHex.CurrentFog <= 0)
        {
            values.Add(_grid.OverHex.Data.TileName);
        }
        else
        {
            values.Add("???");

            if (_grid.OverHex.CanBeDefogged)
            {
                labels.Add("Reveal");
                values.Add($"{_grid.OverHex.CurrentFog} Data");
            }
        }

        _labelsText.text = string.Join('\n', labels);
        _valuesText.text = string.Join('\n', values);
    }
}
