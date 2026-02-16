using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    Animator animator;
    Collider2D bodyCollider;
    public UserInterface healthBar;
    public UserInterface staminaBar;
    public RoundManager roundManager;
    Camera cam;
    Vector2 moveInput;
    List<AttackType> inputSequence = new List<AttackType>();
    public GameObject hitEffects;
    public Animator shadowAnimator;

    public GameObject dustEffectPrefab;
    public Vector2 dustOffset = new Vector2(-0.3f, -0.6f);
    public Transform groundPoint;

    public float speed = 6f;
    public float comboTimeout = 0.15f;
    public float maxHealth = 100;
    public float currHealth;
    public float maxStamina = 100f;
    public float currStamina = 0;
    public float idleStaminaRegen = 20f;
    public float tiredRecoveryThreshold = 50f;

    // Damage Mapping, WIP
    public float damageJab = 2.5f;
    public float damageHeavy = 5f;
    public float damageKick = 3f;
    public float damageFlyingKnee = 15f;
    public float damageLaunch = 1.5f;
    public float damageRightHook = 6f;

    // Stamina Cost
    public float staminaJab = 6f;
    public float staminaKick = 17f;
    public float staminaHeavy = 15f;
    public float staminaLaunch = 10f;
    public float staminaFlyingKnee = 55f;
    public float staminaRightHook = 15f;
    public float staminaBlockDrainPerSecond = 0.1f;

    public bool isTired;
    float comboTimer;
    float halfWidth;
    float camHalfWidth;

    public bool canMove = true;
    bool upHeld = false;
    public bool isInvincible = false;
    bool movementLockedInAir = false;
    public bool knockedDown = false;
    bool blockHeld = false;
    public bool controlsLocked = false;
    public bool facingRight = true;

    public enum AttackType {
        Jab,
        Heavy,
        Kick,
        Launch,
        Block,
        FlyingKnee,
        RightHook
    }

    public AttackType CurrentAttack { get; private set; }

    private void Awake() {
        currStamina = maxStamina;
        staminaBar.SetStamina(currStamina, maxStamina);
        currHealth = maxHealth;
        cam = Camera.main;
        camHalfWidth = cam.orthographicSize * cam.aspect;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        halfWidth = GetComponent<Collider2D>().bounds.extents.x;
        bodyCollider = GetComponent<Collider2D>();
    }

    private void FixedUpdate() {
        if (!canMove) {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (moveInput.y > 0.50f) { 
            upHeld = true;
        } else { 
            upHeld = false; 
        }

        if (movementLockedInAir) {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float x = Mathf.Abs(moveInput.x) > 0.01f
        ? Mathf.Sign(moveInput.x) * speed
        : 0f;

        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);

        if (x > 0.01f)
            facingRight = true;
        else if (x < -0.01f)
            facingRight = false;

        animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocityX));
        shadowAnimator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocityX));

    }

    private void Update() {
        if (controlsLocked)
            return;

        if (Mathf.Abs(rb.linearVelocityX) > 0.01f) {
            float minX = cam.transform.position.x - camHalfWidth + halfWidth;
            float maxX = cam.transform.position.x + camHalfWidth - halfWidth;
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            transform.position = pos;
        }

        if (inputSequence.Count > 0) {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                inputSequence.Clear();
        }

        bool isIdle = canMove && Mathf.Abs(rb.linearVelocityX) < 0.01f && inputSequence.Count == 0 && !blockHeld && !movementLockedInAir;
        bool isWalking = canMove && Mathf.Abs(rb.linearVelocityX) > 0.01f && !blockHeld && !movementLockedInAir;

        float regenRate = 0f;

        if (!isTired && !blockHeld) {
            if (isIdle || isWalking)
                regenRate = idleStaminaRegen;

            currStamina += regenRate * Time.deltaTime;
            currStamina = Mathf.Clamp(currStamina, 0f, maxStamina);
            staminaBar.SetStamina(currStamina, maxStamina);
        }


        if (isTired) {
            currStamina += 8f * Time.deltaTime; 
            if (currStamina >= tiredRecoveryThreshold) {
                currStamina = tiredRecoveryThreshold;
                ExitTired();
            }
            staminaBar.SetStamina(currStamina, maxStamina);
        }


    }
    void PlayAnim(string trigger)
    {
        animator.SetTrigger(trigger);

        if (shadowAnimator != null)
            shadowAnimator.SetTrigger(trigger);
    }

    public void OnMove(InputAction.CallbackContext context) {
        if (controlsLocked) return;
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJab(InputAction.CallbackContext context) {
        if (!context.started) return;
        if (upHeld)
            StartTaunt();
        else
            QueueInput(AttackType.Jab);
    }

    public void OnHeavyPunch(InputAction.CallbackContext context) {
        if (!context.started) return;
        QueueInput(AttackType.Heavy);
    }

    public void OnKick(InputAction.CallbackContext context) {
        if (!context.started) return;
        QueueInput(AttackType.Kick);
    }

    public void OnLaunch(InputAction.CallbackContext context) {
        if (!context.started) return;
        if (upHeld == true) {
            QueueInput(AttackType.Launch);
        }
    }

    public void OnFlyingKnee(InputAction.CallbackContext context) {
        if (!context.started) return;
        if (upHeld == true) {
            QueueInput(AttackType.FlyingKnee);
        }
    }

    public void OnRightHook(InputAction.CallbackContext context) {
        if (!context.started) return;
        QueueInput(AttackType.RightHook);

    }

    public void OnBlock(InputAction.CallbackContext context)
    {
        if (controlsLocked || isTired)
            return;

        if (context.started && currStamina > 0f)
        {
            blockHeld = true;
            canMove = false;

            inputSequence.Clear(); 
            CurrentAttack = AttackType.Block;
            shadowAnimator.SetFloat("xVelocity", 0f);
            animator.SetBool("Block", true);
            shadowAnimator.SetBool("Block", true);
        }
        else if (context.canceled)
        {
            blockHeld = false;
            ReleaseBlock();
        }
    }


    void QueueInput(AttackType attack) {
        if (blockHeld)
            return;

        if (isTired)
            return;

        if (inputSequence.Count >= 3) {
            inputSequence.RemoveAt(0);
        }

        inputSequence.Add(attack);
        comboTimer = comboTimeout;
        TryStartNextAttack();
    }

    void TryStartNextAttack() {
        if (blockHeld)
            return;

        if (!canMove) return;
        if (inputSequence.Count == 0) return;

        AttackType lastAttack = inputSequence[inputSequence.Count - 1];
        inputSequence.Clear();

        if (upHeld == true && lastAttack == AttackType.Heavy) {
            StartLaunch();
            return;
        }
        else if (upHeld == true && lastAttack == AttackType.Kick) {
            StartFlyingKnee();
            return;
        }

        else if (upHeld == true && lastAttack == AttackType.Jab) {
            StartTaunt();
            return;
        }

        switch (lastAttack) {
            case AttackType.Block:
                CurrentAttack = AttackType.Block;
                canMove = false;
                PlayAnim("Block");
                break;
            case AttackType.RightHook:
                CurrentAttack = AttackType.RightHook;
                canMove = false;
                PlayAnim("RightHook");
                break;
            case AttackType.Jab:
                CurrentAttack = AttackType.Jab;
                canMove = false;
                PlayAnim("Jab");
                break;
            case AttackType.Heavy:
                CurrentAttack = AttackType.Heavy;
                canMove = false;
                PlayAnim("HeavyPunch");
                break;
            case AttackType.Kick:
                CurrentAttack = AttackType.Kick;
                canMove = false;
                PlayAnim("Kick");
                break;
        }
    }

    private void StartLaunch() {
        CurrentAttack = AttackType.Launch;
        canMove = false;
        PlayAnim("Launch");
    }
    private void StartFlyingKnee() {
        CurrentAttack = AttackType.FlyingKnee;
        canMove = false;
        speed = 12f;
        PlayAnim("FlyingKnee");
    }
    private void StartTaunt() {
        canMove = false;
        PlayAnim("Taunt");
    }

    public void EndAttack() {
        rb.linearVelocity = Vector2.zero;
        canMove = true;

        if (inputSequence.Count > 0)
            TryStartNextAttack();
    }

    public void SpeedBoost() {
        speed = 10f;
        canMove = true;
        rb.linearVelocity = new Vector2(rb.linearVelocityX, 0f);
        rb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);

        if (inputSequence.Count > 0)
            TryStartNextAttack();
    }

    public void LaunchJump() {
        canMove = true;
        rb.linearVelocity = new Vector2(rb.linearVelocityX, 0f);
        rb.AddForce(Vector2.up * 10f, ForceMode2D.Impulse);
    }

    public void BackUpJump() {
        canMove = true;
        rb.linearVelocity = new Vector2(rb.linearVelocityX, 0f);
        rb.AddForce(Vector2.up * 6f, ForceMode2D.Impulse);
    }

    public void LaunchSpeedBoost() {
        speed = 12f;
        canMove = true;

        if (inputSequence.Count > 0)
            TryStartNextAttack();
    }

    public void TrueEndAttack() {
        rb.linearVelocity = Vector2.zero;
        canMove = true;
        speed = 6f;
        rb.AddForce(Vector2.down * 10f, ForceMode2D.Impulse);

        if (inputSequence.Count > 0)
            TryStartNextAttack();
    }

    public void FallingDownPush() {
        rb.linearVelocity = Vector2.zero;
        canMove = true;
        speed = 6f;
        rb.AddForce(Vector2.down * 4f, ForceMode2D.Impulse);

        if (inputSequence.Count > 0)
            TryStartNextAttack();
    }

    public void ReceiveDamage(AttackType attackType, PlayerController attacker, Vector3 hitPos) {
        if (roundManager.roundOver)
            return;
        
        if (knockedDown) {
            return;
        }

        if (!isInvincible)
        {
            blockHeld = false;
            animator.SetBool("Block", false);
            shadowAnimator.SetBool("Block", false);
        }

        float damage = attackType switch {
            AttackType.Jab => damageJab,
            AttackType.Heavy => damageHeavy,
            AttackType.Kick => damageKick,
            AttackType.FlyingKnee => damageFlyingKnee,
            AttackType.Launch => damageLaunch,
            _ => 0f
        };

        if (attackType == AttackType.RightHook) {
            damage = damageRightHook;
        }

        if (isInvincible) {
            damage *= 0.2f;
            float newHealth = currHealth - damage;
            currHealth = Mathf.Max(newHealth, Mathf.Min(currHealth, 20f));
        } else
        {
            currHealth -= damage;
        }

        SpawnHitFX(hitPos, attackType, isInvincible);

        currHealth = Mathf.Clamp(currHealth, 0f, maxHealth);
        healthBar.SetHealth(currHealth, maxHealth);

        if (currHealth <= 0) {
            KnockedOut();
            return;
        }

        if (isInvincible) {
            return;
        }


        StopAllCoroutines();
        inputSequence.Clear();
        canMove = false;

        if (attackType == AttackType.Launch) {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            movementLockedInAir = true;
        } else {
            rb.linearVelocity = Vector2.zero;
        }

        if (attacker != null) {
            Collider2D attackerCol = attacker.GetComponent<Collider2D>();
            if (attackerCol != null && attackType == AttackType.Launch) {
                Physics2D.IgnoreCollision(bodyCollider, attackerCol, true);
                StartCoroutine(ReenableCollision(attackerCol, 0.10f));
            }
            StartCoroutine(ReenableCollision(attackerCol, 0.10f));
        }

        animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, 0f);

        switch (attackType) {
            case AttackType.Launch:
                animator.Play("Falling Down", 0, 0f);
                shadowAnimator.Play("Falling Down");
                currStamina += 10;
                break;
            case AttackType.Heavy:
            case AttackType.FlyingKnee:
                animator.Play("Damage 2", 0, 0f);
                shadowAnimator.Play("Damage 2", 0, 0f);
                if (currStamina < 50f && isTired)
                    EnterTired();
                else
                    currStamina += 5;
                break;
            default:
                animator.Play("Damage 1", 0, 0f);
                shadowAnimator.Play("Damage 1", 0, 0f);
                if (currStamina < 50f && isTired)
                    EnterTired();
                else
                    currStamina += 1;
                break;
        }
        speed = 6f;
    }

    void SpawnHitFX(Vector3 pos, AttackType attack, bool wasBlocked) {
        Vector3 spawnPos = new Vector3(pos.x, pos.y, -1f);
        Vector2 offset = new Vector2(0f, 0f);
        if (attack == AttackType.Kick) {
            offset = new Vector2(0f, -1.5f);
        } else if (attack == AttackType.Heavy) {
            offset = new Vector2(0f, -0.75f);
        } else if (attack == AttackType.Jab)
        {
            offset = new Vector2(0f, -0.75f);
        }

        spawnPos = new Vector3(
        pos.x + offset.x,
        pos.y + offset.y,
        -1f
    );

        GameObject fx = Instantiate(hitEffects, spawnPos, Quaternion.identity);
        Animator anim = fx.GetComponent<Animator>();
        int hitType = GetHitTypeFromAttack(attack);

        if (wasBlocked) {
            Debug.Log("BLOCKED BLOCKED");
            hitType = 6;
        }
        else {
            hitType = GetHitTypeFromAttack(attack);
        }
        anim.SetInteger("HitType", hitType);
    }

    public void SpawnDustFX()
    {
        if (dustEffectPrefab == null) return;

        Vector3 basePos = groundPoint.position;
        float xOffset = dustOffset.x;
        if (!facingRight)
            xOffset *= -1f;

        Vector3 spawnPos = new Vector3(
            basePos.x + xOffset,
            basePos.y + dustOffset.y,
            -1f
        );

        GameObject fx = Instantiate(dustEffectPrefab, spawnPos, Quaternion.identity);
        Vector3 scale = fx.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1 : -1);
        fx.transform.localScale = scale;

    }




    int GetHitTypeFromAttack(AttackType attack) {
        return attack switch {
            AttackType.Jab => 1,
            AttackType.Heavy => 2,
            AttackType.Kick => 3,
            AttackType.Launch => 4,
            AttackType.FlyingKnee => 5,
            AttackType.RightHook => 7,
            _ => 1
        };
    }


    public void RightHookAssignment() {
        CurrentAttack = AttackType.RightHook;
    }

    void KnockedOut() {
        canMove = false;
        animator.Play("Falling Down");
        if (shadowAnimator != null)
            shadowAnimator.gameObject.SetActive(false);
        roundManager.OnPlayerKO(this);
    }

    IEnumerator ReenableCollision(Collider2D attackerCol, float delay) {
        yield return new WaitForSeconds(delay);
        Physics2D.IgnoreCollision(bodyCollider, attackerCol, false);
    }

    public void FreezeBlockAnimation() {
        if (blockHeld)
            animator.speed = 0f;
            shadowAnimator.speed = 0f;
    }

    void ReleaseBlock() {
        blockHeld = false;
        isInvincible = false;

        canMove = true;
        animator.speed = 1f;
        shadowAnimator.speed = 1f;
        animator.SetBool("Block", false);
        shadowAnimator.SetBool("Block", false);
    }

    public void OnLanded() {
        movementLockedInAir = false;
        canMove = true;
    }

    public void CompleteStop() {
        rb.linearVelocity = Vector2.zero;
        canMove = false;
    }

    public void EnableInvincibility() {
        isInvincible = true;
    }

    public void DisableInvincibility() {
        isInvincible = false;
    }

    public void KnockedDownInvulnerability() {
        knockedDown = true;
    }

    public void KnockedDownInvulnerabilityOff() {
        knockedDown = false;
    }
    public void FreezeOnGround() {
        if (currHealth > 0f)
            return;

        rb.AddForce(Vector2.down * 200f, ForceMode2D.Impulse);
        animator.speed = 0f;
        canMove = false;
        knockedDown = true;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
    }

    void EnterTired() {
        if (isTired) return;

        isTired = true;
        canMove = true;
        blockHeld = false;
        currStamina = -10f;
        staminaBar.SetStamina(currStamina, maxStamina);
        animator.SetBool("Block", false);
        shadowAnimator.SetBool("Block", false);
        animator.SetBool("Tired", true);
    }

    void ExitTired() {
        isTired = false;
        animator.SetBool("Tired", false);
    }

    public void DrainStaminaEvent() {
        float amount = GetStaminaCostForAttack(CurrentAttack);
        DrainStamina(amount);
    }

    void DrainStamina(float amount) {
        currStamina -= amount;

        if (currStamina < 0f)
            EnterTired();

        currStamina = Mathf.Clamp(currStamina, -50f, maxStamina);
        staminaBar.SetStamina(currStamina, maxStamina);
    }

    float GetStaminaCostForAttack(AttackType attack) {
        return attack switch {
            AttackType.Jab => staminaJab,
            AttackType.Kick => staminaKick,
            AttackType.Heavy => staminaHeavy,
            AttackType.Launch => staminaLaunch,
            AttackType.FlyingKnee => staminaFlyingKnee,
            AttackType.RightHook => staminaRightHook,
            AttackType.Block => staminaBlockDrainPerSecond,
            _ => 0f
        };
    }


    public void EnableHitbox() {
        HitBox hitbox = GetComponentInChildren<HitBox>();
        if (hitbox != null)
            hitbox.EnableHitbox();
    }

    public void DisableHitbox() {
        HitBox hitbox = GetComponentInChildren<HitBox>();
        if (hitbox != null)
            hitbox.DisableHitbox();
    }

    public void PlayVictoryTauntDelayed(float delay = 2f) {
        if (currHealth <= 0f) return;
        StartCoroutine(VictoryTauntRoutine(delay));
    }

    IEnumerator VictoryTauntRoutine(float delay) {
        yield return new WaitForSeconds(delay);

        if (currHealth <= 0f) yield break;

        StopAllCoroutines();
        inputSequence.Clear();

        canMove = false;
        blockHeld = false;
        movementLockedInAir = false;

        animator.speed = 1f;
        rb.linearVelocity = Vector2.zero;
        animator.Play("Taunt", 0, 0f);
        shadowAnimator.Play("Taunt", 0, 0f);
        yield return new WaitForSeconds(2.19f);
        animator.Play("Idle", 0, 0f);
    }

    public void FreezeMovementForSeconds(float seconds) {
        StartCoroutine(FreezeMovementRoutine(seconds));
    }

    IEnumerator FreezeMovementRoutine(float seconds) {
        canMove = false;
        yield return new WaitForSeconds(seconds);
        canMove = true;
    }

    public void ResetForNewRound() {
        StopAllCoroutines();

        currHealth = maxHealth;
        currStamina = maxStamina;

        healthBar.SetHealth(currHealth, maxHealth);
        staminaBar.SetStamina(currStamina, maxStamina);

        isInvincible = false;
        isTired = false;
        knockedDown = false;

        canMove = false;
        controlsLocked = true;
        blockHeld = false;
        movementLockedInAir = false;

        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;

        animator.speed = 1f;
        animator.Rebind();
        animator.Update(0f);

        if (shadowAnimator != null)
            shadowAnimator.gameObject.SetActive(true);

        shadowAnimator.speed = 1f;
        shadowAnimator.Rebind();
        shadowAnimator.Update(0f);
    }
    public void LockControls() {
        controlsLocked = true;
        canMove = false;
        rb.linearVelocity = Vector2.zero;
    }
    public void UnlockControls() {
        controlsLocked = false;
        canMove = true;
    }


}
