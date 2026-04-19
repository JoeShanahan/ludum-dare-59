using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldToCanvas;

/// W2C = World To Canvas  
/// Takes a World Position or Transform, and puts this UI object on top of it
public abstract class W2C : MonoBehaviour
{
    // Position info
    private Transform _toFollow;
    private Vector3 _worldPosition;
    
    // Tracking information
    private Camera _camera;
    private Canvas _canvas;
    private bool _dontFollow;

    // Other components
    protected RectTransform _trackRect;  // don't do position tweens on this
    protected CanvasGroup _canvasGroup;

    void Awake()
    {
        _trackRect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    void LateUpdate()
    {
        if (_dontFollow)
            return;

        Vector3 pos = _toFollow  ? _toFollow.position + _worldPosition : _worldPosition;
        _trackRect.anchoredPosition = _camera.WorldToScreenPoint(pos)  / _canvas.scaleFactor;
    }

    /// <summary> 
    /// Set the transform to follow (with an offset) 
    /// </summary>
    public void SetPosition(Transform t, Vector3 p)
    {
        _toFollow = t;
        _worldPosition = p;
    }

    /// <summary> 
    /// Set the transform to follow (without an offset)
    /// </summary>
    public void SetPosition(Transform t)
    {
        _toFollow = t;
    }

    /// <summary> 
    /// Set the world position to follow 
    /// </summary>
    public virtual void SetPosition(Vector3 p)
    {
        _worldPosition = p;
    }

    public void Initialize(Canvas canvas, Camera cam)
    {
        _canvas = canvas;
        _camera = cam;
    }

    public void StopFollowing()
    {
        LateUpdate();
        _dontFollow = true;
    }

    // Shortcuts so it's easier to instantiate things
    public static W2C Instantiate(GameObject prefab, Transform parent=null) => W2CManager.InstantiateW2C(prefab, parent);
    
    public static T InstantiateAs<T>(GameObject prefab, Transform parent=null) where T : W2C => W2CManager.InstantiateAs<T>(prefab, parent);
}
