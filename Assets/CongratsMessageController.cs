using UnityEngine;

public class CongratsMessageController : MonoBehaviour
{
    [SerializeField]
    private Transform _message;

    public void CertificateGet()
    {
        _message.gameObject.SetActive(true);
    }

    public void BtnPressClose()
    {
        _message.gameObject.SetActive(false);
    }
}
