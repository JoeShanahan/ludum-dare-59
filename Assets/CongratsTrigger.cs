using UnityEngine;

public class CongratsTrigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FindFirstObjectByType<CongratsMessageController>().CertificateGet();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
