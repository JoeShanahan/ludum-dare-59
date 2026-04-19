using DG.Tweening;
using UnityEngine;

public class DataParticle : MonoBehaviour
{
    [SerializeField]
    private float _jumpHeight = 2;

    [SerializeField]
    private float _minMoveTime = 0.2f;

    [SerializeField]
    private float _maxMoveTime = 0.3f;

    private Transform _cam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cam = Camera.main.transform;    
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(_cam);
    }

    public void Init(MapHex startHex, MapHex endHex)
    {
        float dist = Vector3.Distance(startHex.transform.position, endHex.transform.position);
        if (dist < 2)
        {
            dist = Mathf.Lerp(dist, 2, 0.5f);
        }

        transform.position = startHex.transform.position + Vector3.up;
        transform.localScale = Vector3.zero;
        transform.DOScale(.12f, 0.3f).SetEase(Ease.OutExpo);

        float travelTime = Random.Range(_minMoveTime, _maxMoveTime) * dist;

        float jumpHeight = Random.Range(_jumpHeight - 0.43f, _jumpHeight + 0.3f);
        transform.DOJump(endHex.transform.position + Vector3.up, jumpHeight, 1, travelTime).SetEase(Ease.InSine).OnComplete(() =>
        {
            endHex.RemoveFog(1);
            GetComponent<SpriteRenderer>().enabled = false;
            Destroy(gameObject, 3);
        });
    }
}
