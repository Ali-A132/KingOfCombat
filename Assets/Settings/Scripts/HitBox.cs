using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HitBox : MonoBehaviour
{
    public LayerMask opponentLayer;

    private PlayerController attacker;
    private Collider2D col;
    private bool hasHit;

    private void Awake() {
        attacker = GetComponentInParent<PlayerController>();
        col = GetComponent<Collider2D>();

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

        if (((1 << other.gameObject.layer) & opponentLayer) == 0) {
            return;
        }

        PlayerController defender = other.GetComponentInParent<PlayerController>();

        if (defender == null || attacker == null || defender == attacker) {
            return;
        }

        if (defender.isInvincible) {
            return;
        }

        hasHit = true;
        Debug.Log("HIT");
        defender.ReceiveDamage(attacker.CurrentAttack, attacker);
    }
}
