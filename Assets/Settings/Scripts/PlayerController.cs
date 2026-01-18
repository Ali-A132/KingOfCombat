using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using static PlayerController;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    Animator animator;

    public float speed = 5f;
    public bool canMove = true;
    public float comboTimeout = 0.35f;
    Vector2 moveInput;
    List<AttackType> inputSequence = new List<AttackType>();
    float comboTimer;
    bool upHeld = false;
    float halfWidth;

    public enum AttackType {
        Jab,
        Heavy,
        Kick
    }

    public AttackType CurrentAttack { get; private set; }

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        halfWidth = GetComponent<Collider2D>().bounds.extents.x;
    }

    private void FixedUpdate() {
        if (!canMove) {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        if (moveInput.y > 0.5f)
        {
            upHeld = true;
        }
        else {
            upHeld = false;
        }

        float x = Mathf.Abs(moveInput.x) > 0.01f ? Mathf.Sign(moveInput.x) * speed: 0f;

        rb.linearVelocity = new Vector2(x, rb.linearVelocityY);
        animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocityX));
    }

    private void Update() {
        Camera cam = Camera.main;

        float camHalfWidth = cam.orthographicSize * cam.aspect;
        float minX = cam.transform.position.x - camHalfWidth + halfWidth;
        float maxX = cam.transform.position.x + camHalfWidth - halfWidth;
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);

        transform.position = pos;
        if (inputSequence.Count > 0) {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                inputSequence.Clear();
        }
    }

    public void OnMove(InputAction.CallbackContext context) {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJab(InputAction.CallbackContext context) {
        if (!context.started) return;
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
            QueueInput(AttackType.Heavy);
        }
    }

    public void OnFlyingKnee(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (upHeld == true)
        {
            QueueInput(AttackType.Kick);
        }
    }

    public void OnTaunt(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (upHeld == true)
        {
            QueueInput(AttackType.Jab);
        }
    }

    public void OnRightHook(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        QueueInput(AttackType.Jab);

    }

    void QueueInput(AttackType attack) {
        if (inputSequence.Count >= 4) {
            inputSequence.RemoveAt(0);
        }

        inputSequence.Add(attack);
        comboTimer = comboTimeout;
        TryStartNextAttack();
    }

    void TryStartNextAttack() {
        if (!canMove) return;
        if (inputSequence.Count == 0) return;

        if (IsRightHook())
        {
            inputSequence.Clear();
            canMove = false;
            CurrentAttack = AttackType.Heavy;
            animator.SetTrigger("RightHook");
            return;
        }

        AttackType lastAttack = inputSequence[inputSequence.Count - 1];
        inputSequence.Clear();

        if (upHeld == true && lastAttack == AttackType.Heavy)
        {
            StartLaunch();
            return;
        }
        else if (upHeld == true && lastAttack == AttackType.Kick) {
            StartFlyingKnee();
            return;
        }

        else if (upHeld == true && lastAttack == AttackType.Jab)
        {
            StartTaunt();
            return;
        }

        switch (lastAttack)
        {
            case AttackType.Jab:
                CurrentAttack = AttackType.Jab;
                canMove = false;
                animator.SetTrigger("Jab");

                break;
            case AttackType.Heavy:
                CurrentAttack = AttackType.Heavy;
                canMove = false;
                animator.SetTrigger("HeavyPunch");
                break;

            case AttackType.Kick:
                CurrentAttack = AttackType.Kick;
                canMove = false;
                animator.SetTrigger("Kick");
                break;
        }
    }

    private void StartLaunch() {
        CurrentAttack = AttackType.Heavy;
        canMove = false;
        animator.SetTrigger("Launch");
    }
    private void StartFlyingKnee() {
        CurrentAttack = AttackType.Heavy;
        canMove = false;
        speed = 12f;
        animator.SetTrigger("FlyingKnee");
    }
    private void StartTaunt()
    {
        canMove = false;
        animator.SetTrigger("Taunt");
    }

    bool IsRightHook() {
        if (inputSequence.Count < 3) return false;
        int count = inputSequence.Count;
        return inputSequence[count - 3] == AttackType.Jab && inputSequence[count - 2] == AttackType.Heavy && inputSequence[count - 1] == AttackType.Jab;
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

    public void LaunchSpeedBoost() {
        speed = 8f;
        canMove = true;

        if (inputSequence.Count > 0)
            TryStartNextAttack();
    }

    public void CompleteStop()
    {
        speed = 0f;
        canMove = false;
    }

    public void TrueEndAttack() {
        rb.linearVelocity = Vector2.zero;
        canMove = true;
        speed = 5f;
        rb.AddForce(Vector2.down * 10f, ForceMode2D.Impulse);

        if (inputSequence.Count > 0)
            TryStartNextAttack();
    }

    public void ReceiveDamage(AttackType attackType) {
        StopAllCoroutines();
        inputSequence.Clear();
        canMove = false;
        rb.linearVelocity = Vector2.zero;

        animator.ResetTrigger("Jab");
        animator.ResetTrigger("HeavyPunch");
        animator.ResetTrigger("Kick");
        animator.ResetTrigger("Launch");
        animator.ResetTrigger("FlyingKnee");
        animator.ResetTrigger("Taunt");
        animator.ResetTrigger("RightHook");
        switch (attackType) {
            case AttackType.Heavy:
                animator.Play("Damage 2", 0, 0f);
                break;
            default:
                animator.Play("Damage 1", 0, 0f);
                break;

        }
    }

    public void EnableHitbox()
    {
        HitBox hitbox = GetComponentInChildren<HitBox>();
        if (hitbox != null)
            hitbox.EnableHitbox();
    }

    public void DisableHitbox()
    {
        HitBox hitbox = GetComponentInChildren<HitBox>();
        if (hitbox != null)
            hitbox.DisableHitbox();
    }


}
