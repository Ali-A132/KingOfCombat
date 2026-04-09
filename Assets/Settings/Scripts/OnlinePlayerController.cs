using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Netcode.Components;

[RequireComponent(typeof(NetworkTransform))]
public class OnlinePlayerController : PlayerController {
    NetworkVariable<float> netHealth = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    NetworkVariable<float> netStamina = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    bool isInDamageState = false;

    public override void OnNetworkSpawn() {
        netHealth.OnValueChanged += OnHealthChanged;
        netStamina.OnValueChanged += OnStaminaChanged;

        healthBar.SetHealth(netHealth.Value, maxHealth);
        staminaBar.SetStamina(netStamina.Value, maxStamina);

        LockControls();
    }

    public override void OnNetworkDespawn() {
        netHealth.OnValueChanged -= OnHealthChanged;
        netStamina.OnValueChanged -= OnStaminaChanged;
    }

    void OnHealthChanged(float prev, float curr) {
        healthBar.SetHealth(curr, maxHealth);
    }

    void OnStaminaChanged(float prev, float curr) {
        staminaBar.SetStamina(curr, maxStamina);
    }


    protected override void FixedUpdate() {
        if (!IsOwner) return;
        base.FixedUpdate();
    }

    protected override void Update() {
        if (!IsOwner) return;
        base.Update();
        SubmitStaminaServerRpc(currStamina);
    }

    public new void OnMove(InputAction.CallbackContext context) {
        if (!IsOwner) return;
        base.OnMove(context);
    }

    public new void OnJab(InputAction.CallbackContext context) {
        if (!IsOwner || !context.started) return;
        bool up = moveInput.y > 0.5f;
        base.OnJab(context);
        SyncAnimServerRpc(up ? "Taunt" : "Jab");
    }

    public new void OnHeavyPunch(InputAction.CallbackContext context) {
        if (!IsOwner || !context.started) return;
        bool up = moveInput.y > 0.5f;
        base.OnHeavyPunch(context);
        SyncAnimServerRpc(up ? "Launch" : "HeavyPunch");
    }

    public new void OnKick(InputAction.CallbackContext context) {
        if (!IsOwner || !context.started) return;
        bool up = moveInput.y > 0.5f;
        base.OnKick(context);
        SyncAnimServerRpc(up ? "Special" : "Kick");
    }

    public new void OnLaunch(InputAction.CallbackContext context) {
        if (!IsOwner || !context.started) return;
        if (moveInput.y <= 0.5f) return;
        base.OnLaunch(context);
        SyncAnimServerRpc("Launch");
    }

    public new void OnSpecial(InputAction.CallbackContext context) {
        if (!IsOwner || !context.started) return;
        if (moveInput.y <= 0.5f) return;
        base.OnSpecial(context);
        SyncAnimServerRpc("Special");
    }

    public new void OnChain(InputAction.CallbackContext context) {
        if (!IsOwner || !context.started) return;
        base.OnChain(context);
        SyncAnimServerRpc("Chain");
    }

    public new void OnBlock(InputAction.CallbackContext context) {
        if (!IsOwner) return;
        base.OnBlock(context);
        if (context.started) SyncBlockServerRpc(true);
        if (context.canceled) SyncBlockServerRpc(false);
    }

    [ServerRpc]
    void SubmitStaminaServerRpc(float stamina) {
        netStamina.Value = stamina;
    }

    public void ApplyDamage(AttackType attackType, Vector3 hitPos) {
        if (!IsServer) return;

        float damage = attackType switch {
            AttackType.Jab => damageJab,
            AttackType.Heavy => damageHeavy,
            AttackType.Kick => damageKick,
            AttackType.Special => damageSpecial,
            AttackType.Launch => damageLaunch,
            AttackType.Chain => damageChain,
            _ => 0f
        };

        if (isInvincible) {
            damage *= 0.2f;
            float newHealth = currHealth - damage;
            currHealth = Mathf.Max(newHealth, Mathf.Min(currHealth, 20f));
        }
        else {
            currHealth -= damage;
        }

        currHealth = Mathf.Clamp(currHealth, 0f, maxHealth);
        netHealth.Value = currHealth;
        ApplyDamageClientRpc((int)attackType, hitPos, currHealth);
    }

    [ClientRpc]
    void ApplyDamageClientRpc(int attackTypeInt, Vector3 hitPos, float newHealth) {
        AttackType attack = (AttackType)attackTypeInt;

        if (roundManager.roundOver || knockedDown) return;

        isInDamageState = true;
        currHealth = newHealth;
        healthBar.SetHealth(currHealth, maxHealth);

        if (!isInvincible) {
            blockHeld = false;
            animator.SetBool("Block", false);
            shadowAnimator.SetBool("Block", false);
        }

        SpawnHitFX(hitPos, attack, isInvincible);

        if (currHealth <= 0) {
            KnockedOut();
            return;
        }

        if (isInvincible) return;

        switch (attack) {
            case AttackType.Launch:
                SyncAnimPlayServerRpc("Falling Down");
                break;

            case AttackType.Heavy:
            case AttackType.Special:
                SyncAnimPlayServerRpc("Damage 2");
                break;

            default:
                SyncAnimPlayServerRpc("Damage 1");
                break;
        }

        Invoke(nameof(ExitDamageState), 0.3f);
    }

    void ExitDamageState() {
        isInDamageState = false;
    }

    public override void EnableHitbox() {
        OnlineHitBox hitbox = GetComponentInChildren<OnlineHitBox>();
        if (hitbox != null)
            hitbox.EnableHitbox();
    }

    public override void DisableHitbox() {
        OnlineHitBox hitbox = GetComponentInChildren<OnlineHitBox>();
        if (hitbox != null)
            hitbox.DisableHitbox();
    }

    protected override void KnockedOut() {
        canMove = false;
        animator.Play("Falling Down");
        if (shadowAnimator != null) shadowAnimator.gameObject.SetActive(false);
        if (IsServer) roundManager.OnPlayerKO(this);
    }

    [ServerRpc]
    void SyncAnimServerRpc(string trigger) {
        SyncAnimClientRpc(trigger);
    }

    [ClientRpc]
    void SyncAnimClientRpc(string trigger) {
        if (IsOwner) return;
        if (isInDamageState) return;
        animator.SetTrigger(trigger);
        if (shadowAnimator != null) shadowAnimator.SetTrigger(trigger);
    }

    [ServerRpc]
    void SyncAnimPlayServerRpc(string trigger)
    {
        SyncAnimPlayClientRpc(trigger);
    }

    [ClientRpc]
    void SyncAnimPlayClientRpc(string trigger)
    {
        if (IsOwner) return;
        animator.Play(trigger, 0, 0f);
        if (shadowAnimator != null) shadowAnimator.Play(trigger, 0, 0f);
    }

    [ServerRpc]
    void SyncBlockServerRpc(bool blocking) {
        SyncBlockClientRpc(blocking);
    }

    [ClientRpc]
    void SyncBlockClientRpc(bool blocking) {
        if (IsOwner) return;
        animator.SetBool("Block", blocking);
        if (shadowAnimator != null) shadowAnimator.SetBool("Block", blocking);
    }

    [ServerRpc]
    void SyncVelocityAnimServerRpc(float xVel) {
        SyncVelocityAnimClientRpc(xVel);
    }

    [ClientRpc]
    void SyncVelocityAnimClientRpc(float xVel)
    {
        if (IsOwner) return;
        if (isInDamageState) return; 

        animator.SetFloat("xVelocity", xVel);
        if (shadowAnimator != null) shadowAnimator.SetFloat("xVelocity", xVel);
    }

    protected override void FixedUpdate_PostBase() {
        if (!IsOwner) return;
        SyncVelocityAnimServerRpc(Mathf.Abs(rb.linearVelocityX));
    }
}