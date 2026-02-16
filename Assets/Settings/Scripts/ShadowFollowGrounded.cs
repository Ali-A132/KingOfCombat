using UnityEngine;

public class ShadowFollowGrounded : MonoBehaviour
{
    public Transform player;
    public float groundY = -10.75f;

    void LateUpdate()
    {
        if (!player) return;

        transform.position = new Vector3(
            player.position.x,
            groundY,
            player.position.z
        );
    }
}
