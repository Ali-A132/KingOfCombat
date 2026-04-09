using UnityEngine;
using Unity.Netcode;

public class OnlineCountDownTimer : NetworkBehaviour {
    public float maxTime = 99f;
    public Animator animator;
    public OnlineRoundManager roundManager;

    NetworkVariable<float> syncedTime = new NetworkVariable<float>(99f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    NetworkVariable<bool> isRunning = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    bool _timeOverFired = false;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        syncedTime.OnValueChanged += OnTimeChanged;
        isRunning.OnValueChanged += OnRunningChanged;
        StopTimer();
        ResetTimer();
    }

    public override void OnNetworkDespawn() {
        base.OnNetworkDespawn();
        syncedTime.OnValueChanged -= OnTimeChanged;
        isRunning.OnValueChanged -= OnRunningChanged;
    }

    void Update() {
        if (!IsServer || !isRunning.Value) return;

        syncedTime.Value -= Time.deltaTime;
        syncedTime.Value = Mathf.Max(syncedTime.Value, 0f);

        if (syncedTime.Value <= 0f && !_timeOverFired) {
            _timeOverFired = true;
            isRunning.Value = false;
            roundManager.OnTimeOver();
        }
    }

    void OnTimeChanged(float prev, float curr) {
        UpdateVisual();
    }

    void OnRunningChanged(bool prev, bool curr) {
        animator.speed = curr ? 1f : 0f;
    }

    void UpdateVisual() {
        float normalized = syncedTime.Value / maxTime;
        animator.Play("CountdownTimer", 0, 1f - normalized);
    }

    public void StartTimer() {
        if (!IsServer) return;
        _timeOverFired = false;
        isRunning.Value = true;
        animator.speed = 1f;
    }

    public void StopTimer() {
        if (!IsServer) return;
        isRunning.Value = false;
        animator.speed = 0f;
    }

    public void ResetTimer() {
        if (!IsServer) return;
        syncedTime.Value = maxTime;
        UpdateVisual();
    }

    public bool IsRunning() => isRunning.Value;
}