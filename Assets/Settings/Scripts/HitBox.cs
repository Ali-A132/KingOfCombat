using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class HitBox : MonoBehaviour
{
    public LayerMask opponentLayer;

    private PlayerController attacker;
    private bool hasHit;
    private BoxCollider2D col;

    private void Awake() {
        attacker = GetComponentInParent<PlayerController>();
        col = GetComponent<BoxCollider2D>();

        col.isTrigger = true;
        col.enabled = false;
    }

    public void EnableHitbox() {
        hasHit = false;
        col.enabled = true;
    }

    public void DisableHitbox() {
        col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (!other.CompareTag("Player")) return;

        PlayerController defender = other.GetComponentInParent<PlayerController>();

        if (defender == null || attacker == null || defender == attacker) {
            return;
        }

        Vector3 hitPos = GetHitPosition();
        hasHit = true;
        Debug.Log($"{attacker.name} hit {defender.name}");
        defender.ReceiveDamage(attacker.CurrentAttack, attacker, hitPos);
    }

    Vector3 GetHitPosition()
    {
        Vector3 center = col.bounds.center;
        float halfWidth = col.bounds.extents.x;
        float dir = Mathf.Sign(transform.lossyScale.x);
        Vector3 pos = center;
        pos.x += halfWidth * dir;

        return pos;
    }
}
