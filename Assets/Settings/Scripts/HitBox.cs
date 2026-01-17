using System.ComponentModel;
using UnityEngine;


public class HitBox : MonoBehaviour
{
    public LayerMask opponentLayer;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & opponentLayer) != 0)
        {
            PlayerController defender = other.GetComponent<PlayerController>();
            Debug.Log(defender);
            PlayerController attacker = GetComponentInParent<PlayerController>();
            Debug.Log(attacker);

            if (defender == null || attacker == null || attacker == defender)
            {
                return;
            }

            Debug.Log("HIT opponent");
            defender.ReceiveDamage();
        }
    }
}
