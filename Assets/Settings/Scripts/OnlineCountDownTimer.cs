using UnityEngine;
using Unity.Netcode;

public class OnlineCountDownTimer : NetworkBehaviour
{
    public float maxTime = 99f;
    public Animator animator;
    public OnlineRoundManager roundManager;

    public GameObject timerUIRoot;
    NetworkVariable<float> syncedTime = new NetworkVariable<float>(99f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    NetworkVariable<bool> isRunning = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    bool timeOverFired = false;
    bool initialized = false;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        syncedTime.OnValueChanged += OnTimeChanged;
        isRunning.OnValueChanged += OnRunningChanged;
    }

    public override void OnNetworkDespawn() {
        base.OnNetworkDespawn();
        syncedTime.OnValueChanged -= OnTimeChanged;
        isRunning.OnValueChanged -= OnRunningChanged;
    }

    public void Initialize() {
        if (initialized) return;
        initialized = true;

        if (timerUIRoot != null) timerUIRoot.SetActive(true);

        animator.speed = 0f;
        UpdateVisual();

        if (IsServer) {
            syncedTime.Value = maxTime;
            isRunning.Value = false;
        }
    }

    void Update() {
        if (!IsServer || !isRunning.Value) return;

        syncedTime.Value -= Time.deltaTime;
        syncedTime.Value = Mathf.Max(syncedTime.Value, 0f);

        if (syncedTime.Value <= 0f && !timeOverFired) {
            timeOverFired = true;
            isRunning.Value = false;
            roundManager.OnTimeOver();
        }
    }

    void OnTimeChanged(float prev, float curr) {
        if (!initialized) 
            return;
        UpdateVisual();
    }

    void OnRunningChanged(bool prev, bool curr) {
        if (!initialized) 
            return;
        animator.speed = curr ? 1f : 0f;
    }

    void UpdateVisual() {
        if (animator == null || !animator.gameObject.activeInHierarchy) return;

        float normalized = syncedTime.Value / maxTime;
        float prevSpeed = animator.speed;
        animator.speed = 1f;
        animator.Play("CountdownTimer", 0, 1f - normalized);
        animator.speed = prevSpeed;
    }

    public void StartTimer() {
        if (!IsServer) return;
        timeOverFired = false;
        isRunning.Value = true;
    }

    public void StopTimer() {
        if (!IsServer) return;
        isRunning.Value = false;
    }

    public void ResetTimer() {
        if (!IsServer) return;
        syncedTime.Value = maxTime;
        UpdateVisual();
    }

    public bool IsRunning() => isRunning.Value;
}