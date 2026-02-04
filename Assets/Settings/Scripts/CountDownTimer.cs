using UnityEngine;

public class CountDownTimer : MonoBehaviour
{
    public float maxTime = 99f;
    public Animator animator;
    public RoundManager roundManager;

    float currentTime;
    bool isRunning = false;

    void Awake() {
        ResetTimer();
        StopTimer();
    }

    void Update() {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(currentTime, 0f);
        UpdateVisual();

        if (currentTime <= 0f) {
            isRunning = false;
            roundManager.OnTimeOver();
        }
    }

    void UpdateVisual() {
        float normalized = currentTime / maxTime;
        animator.Play("CountdownTimer", 0, 1f - normalized);
    }

    public void StartTimer() {
        animator.speed = 1f;
        isRunning = true;
    }

    public void StopTimer() {
        animator.speed = 0f;
        isRunning = false;
    }

    public void ResetTimer() {
        currentTime = maxTime;
        UpdateVisual();
    }

    public bool IsRunning() {
        return isRunning;
    }
}
