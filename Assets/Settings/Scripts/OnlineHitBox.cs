using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(BoxCollider2D))]
public class OnlineHitBox : NetworkBehaviour {
    public LayerMask opponentLayer;

    OnlinePlayerController attacker;
    bool hasHit;
    BoxCollider2D col;

    void Awake() {
        attacker = GetComponentInParent<OnlinePlayerController>();
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
        hasHit = false;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (!IsOwner) return; 
        if (hasHit) return;
        if (!other.CompareTag("Player")) return;

        OnlinePlayerController defender = other.GetComponentInParent<OnlinePlayerController>();
        if (defender == null || attacker == null || defender == attacker)
            return;

        hasHit = true;

        Vector3 hitPos = other.ClosestPoint(col.bounds.center);

        RegisterHitServerRpc(defender.NetworkObject, (int)attacker.CurrentAttack, hitPos);
        Invoke(nameof(ResetHit), 0.1f);
    }

    void ResetHit() {
        hasHit = false;
    }

    [ServerRpc]
    void RegisterHitServerRpc(NetworkObjectReference defenderRef, int attackTypeInt, Vector3 hitPos)  {
        if (!defenderRef.TryGet(out NetworkObject defenderNetObj)) return;

        OnlinePlayerController defender = defenderNetObj.GetComponent<OnlinePlayerController>();
        if (defender == null) return;

        defender.ApplyDamage((OnlinePlayerController.AttackType)attackTypeInt, hitPos);
    }
}