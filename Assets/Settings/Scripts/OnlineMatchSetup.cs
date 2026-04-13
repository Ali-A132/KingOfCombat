using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class OnlineMatchSetup : NetworkBehaviour {
    public GameObject mahsk;
    public GameObject mahskTitleCard;
    public GameObject mahskFlipped;
    public GameObject mahskFlippedTitleCard;
    public GameObject payet;
    public GameObject payetTitleCard;
    public GameObject payetFlipped;
    public GameObject payetFlippedTitleCard;

    public OnlineRoundManager roundManager;
    public CinemachineTargetGroup targetGroup;
    public CinemachineConfiner2D confiner;
    public GameObject stage;
    public GameObject lobbyPanel;
    public OnlineCountDownTimer countDownTimer;

    GameObject p1Object;
    GameObject p2Object;

    public override void OnNetworkSpawn() {
        countDownTimer.StopTimer();
        base.OnNetworkSpawn();
        HideAllPlayers();
        HideAllTitleCards();
    }

    public void ActivateAndAssignOwnership(int p1CharIndex, ulong p1ClientId, int p2CharIndex, ulong p2ClientId) {
        if (!IsServer) return;

        GameObject p1Obj = p1CharIndex == 0 ? mahsk : payetFlipped;
        GameObject p2Obj = p2CharIndex == 0 ? mahskFlipped : payet;

        var p1NetObj = p1Obj.GetComponent<NetworkObject>();
        var p2NetObj = p2Obj.GetComponent<NetworkObject>();

        if (p1NetObj != null) 
            p1NetObj.ChangeOwnership(p1ClientId);
        else 
            Debug.LogError("OnlineMatchSetup P1 missing NetworkObject!");

        if (p2NetObj != null) 
            p2NetObj.ChangeOwnership(p2ClientId);
        else 
            Debug.LogError("OnlineMatchSetup P2 missing NetworkObject!");

        ShowPlayersClientRpc(p1CharIndex, p2CharIndex);
    }

    [ClientRpc]
    void ShowPlayersClientRpc(int p1CharIndex, int p2CharIndex) {
        if (stage != null) stage.SetActive(true);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (countDownTimer != null) countDownTimer.Initialize();

        StartCoroutine(RefreshConfiner());
        p1Object = p1CharIndex == 0 ? mahsk : payetFlipped;
        p2Object = p2CharIndex == 0 ? mahskFlipped : payet;

        GameObject p1TitleCard = p1CharIndex == 0 ? mahskTitleCard : payetFlippedTitleCard;
        GameObject p2TitleCard = p2CharIndex == 0 ? mahskFlippedTitleCard : payetTitleCard;

        DeactivateUnused(p1Object, p2Object);
        ShowPlayer(p1Object); p1TitleCard.SetActive(true);
        ShowPlayer(p2Object); p2TitleCard.SetActive(true);

        roundManager.player1 = p1Object.GetComponent<PlayerController>();
        roundManager.player2 = p2Object.GetComponent<PlayerController>();
        targetGroup.Targets = new List<CinemachineTargetGroup.Target>
        {
            new CinemachineTargetGroup.Target { Object = p1Object.transform, Weight = 1f, Radius = 1f },
            new CinemachineTargetGroup.Target { Object = p2Object.transform, Weight = 1f, Radius = 1f }
        };

        if (NetworkManager.Singleton.IsServer)
            roundManager.BeginOnlineMatch();
        UnfreezePlayersY();
    }

    public void UnfreezePlayersY() {
        FreezeY(p1Object, false);
        FreezeY(p2Object, false);
    }

    void DeactivateUnused(GameObject active1, GameObject active2) {
        GameObject[] all = { mahsk, mahskFlipped, payet, payetFlipped };
        foreach (var go in all) {
            if (go == null) 
                continue;
            if (go != active1 && go != active2) 
                go.SetActive(false);
        }
    }

    void HideAllPlayers() {
        HidePlayer(mahsk); 
        HidePlayer(mahskFlipped);
        HidePlayer(payet); 
        HidePlayer(payetFlipped);
    }

    void HideAllTitleCards() {
        if (mahskTitleCard != null) mahskTitleCard.SetActive(false);
        if (mahskFlippedTitleCard != null) mahskFlippedTitleCard.SetActive(false);
        if (payetTitleCard != null) payetTitleCard.SetActive(false);
        if (payetFlippedTitleCard != null) payetFlippedTitleCard.SetActive(false);
    }

    void HidePlayer(GameObject player) {
        if (player == null) return;
        foreach (var sr in player.GetComponentsInChildren<SpriteRenderer>(true))
            sr.enabled = false;
        foreach (var col in player.GetComponentsInChildren<Collider2D>(true))
            col.enabled = false;
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) { 
            rb.simulated = false; 
            rb.linearVelocity = Vector2.zero; 
        }
        var pc = player.GetComponent<OnlinePlayerController>();
        if (pc != null) 
            pc.enabled = false;
    }

    void ShowPlayer(GameObject player) {
        if (player == null) return;
        foreach (var sr in player.GetComponentsInChildren<SpriteRenderer>(true))
            sr.enabled = true;
        foreach (var col in player.GetComponentsInChildren<Collider2D>(true))
            col.enabled = true;
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) 
            rb.simulated = true;
        var pc = player.GetComponent<OnlinePlayerController>();
        if (pc != null) 
            pc.enabled = true;
    }

    void FreezeY(GameObject player, bool freeze) {
        if (player == null) return;
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        rb.constraints = freeze ? RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.FreezeRotation;
    }

    IEnumerator RefreshConfiner() {
        yield return null;
        if (confiner != null) 
            confiner.InvalidateBoundingShapeCache();
    }
}