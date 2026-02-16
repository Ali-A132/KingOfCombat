using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float lifeTime = 0.4f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
