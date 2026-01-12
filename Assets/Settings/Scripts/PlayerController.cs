using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    Animator animator;

    public float speed = 5f;
    public bool canMove = true;
    public float inputBuffer = 0.1f;
    public bool autoHeavy = true;

    bool queuedJab;
    bool queuedHeavy;
    bool queuedKick;
    int comboStepChain; 
    Vector2 moveInput;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (!canMove) {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocityY);
        animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocityX));
    }

    public void OnMove(InputAction.CallbackContext context) {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJab(InputAction.CallbackContext context) {
        if (context.started) {
            queuedJab = true;
            TryStartNextAttack();
        }
    }

    public void OnHeavyPunch(InputAction.CallbackContext context)
    {
        if (context.started) {
            queuedHeavy = true;
            TryStartNextAttack();
        }
    }

    public void OnKick(InputAction.CallbackContext context)
    {
        if (context.started) {
            queuedKick = true;
            TryStartNextAttack();
        }
    }

    void TryStartNextAttack() {
        if (!canMove) { 
            return; 
        }

        if (comboStepChain == 0 && queuedJab) {
            queuedJab = false;
            StartJab();
            comboStepChain = 1; 
        }
        else if (comboStepChain == 1 && queuedJab) {
            queuedJab = false;
            StartJab();

            if (autoHeavy && queuedHeavy) {
                comboStepChain = 2; 
            }
            else {
                comboStepChain = 0; 
            }
        }
        else if (comboStepChain == 2 && queuedHeavy) {
            queuedHeavy = false;
            StartHeavy();
            comboStepChain = 0;
        }
        else if (queuedHeavy) {
            queuedHeavy = false;
            StartHeavy();
            comboStepChain = 0;
        }
        else if (queuedKick) {
            queuedKick = false;
            StartKick();
            comboStepChain = 0;
        }
    }

    void StartJab() {
        canMove = false;
        animator.SetInteger("ComboStepChain", comboStepChain);
        animator.SetTrigger("Jab");
    }

    void StartHeavy() {
        canMove = false;
        animator.SetTrigger("HeavyPunch");
    }

    void StartKick() {
        canMove = false;
        animator.SetTrigger("Kick");
    }

    public void EndAttack() {
        rb.linearVelocity = Vector2.zero;
        canMove = true;

        if (!autoHeavy && comboStepChain == 0)
            comboStepChain = 0;

        TryStartNextAttack();
    }
}
