using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    Animator animator;

    public float speed = 4f;
    public bool canMove = true;
    public float comboTimeout = 0.35f;
    Vector2 moveInput;
    List<AttackType> inputSequence = new List<AttackType>();
    float comboTimer;

    enum AttackType {
        Jab,
        Heavy,
        Kick
    }

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate() {
        if (!canMove) {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocityY);
        animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocityX));
    }

    private void Update() {
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
            animator.SetTrigger("RightHook");
            return;
        }

        AttackType lastAttack = inputSequence[inputSequence.Count - 1];
        inputSequence.Clear();

        switch (lastAttack)
        {
            case AttackType.Jab:
                canMove = false;
                animator.SetTrigger("Jab");

                break;
            case AttackType.Heavy:
                canMove = false;
                animator.SetTrigger("HeavyPunch");
                break;

            case AttackType.Kick:
                canMove = false;
                animator.SetTrigger("Kick");
                break;
        }
    }


    bool IsRightHook() {
        if (inputSequence.Count < 4) return false;
        int count = inputSequence.Count;
        return inputSequence[count - 4] == AttackType.Jab && inputSequence[count - 3] == AttackType.Jab && inputSequence[count - 2] == AttackType.Heavy && inputSequence[count - 1] == AttackType.Jab;
    }

    public void EndAttack() {
        rb.linearVelocity = Vector2.zero;
        canMove = true;

        if (inputSequence.Count > 0)
            TryStartNextAttack();
    }
}
