using Unity.Netcode;
using UnityEngine;


public class SessionWatcher : NetworkBehaviour {
    public GameObject lobbyPanel;
    public GameObject stage;
    public OnlineMatchSetup matchSetup;
    public Animator animator;

    public int requiredPlayers = 2;
    public static int SelectedCharacter = 0;

    bool collectingChoices = false;
    bool matchStarted = false;


    public void OnCharacterSelected(int index) {
        SelectedCharacter = index;
        Debug.Log($"SessionWatcher Character selected locally: {index}");
    }

    void Update() {
        if (!IsServer || matchStarted) return;
        int connected = NetworkManager.Singleton.ConnectedClients.Count;
        animator.SetTrigger("Host");

        if (!collectingChoices && connected >= requiredPlayers) {
            bool allSpawned = true;
            foreach (var kvp in NetworkManager.Singleton.ConnectedClients) {
                if (kvp.Value.PlayerObject == null || kvp.Value.PlayerObject.GetComponent<PlayerData>() == null) {
                    allSpawned = false;
                    break;
                }
            }

            if (!allSpawned) return;

            collectingChoices = true;
            animator.SetTrigger("Found");
            Debug.Log("SessionWatcher 2 players connected requesting character choices.");
            RequestCharacterChoicesClientRpc();
        }

        if (collectingChoices && !matchStarted){
            bool allSubmitted = true;
            foreach (var kvp in NetworkManager.Singleton.ConnectedClients) {
                var data = kvp.Value.PlayerObject?.GetComponent<PlayerData>();
                if (data == null || !data.hasSubmitted.Value) {
                    allSubmitted = false;
                    break;
                }
            }

            if (!allSubmitted) return;

            matchStarted = true;

            int p1CharIndex = 0;
            int p2CharIndex = 0;
            ulong p1ClientId = NetworkManager.ServerClientId;
            ulong p2ClientId = NetworkManager.ServerClientId;

            foreach (var kvp in NetworkManager.Singleton.ConnectedClients) {
                var data = kvp.Value.PlayerObject.GetComponent<PlayerData>();
                if (kvp.Key == NetworkManager.ServerClientId) {
                    p1CharIndex = data.characterIndex.Value;
                    p1ClientId = kvp.Key;
                }
                else {
                    p2CharIndex = data.characterIndex.Value;
                    p2ClientId = kvp.Key;
                }
            }

            Debug.Log($"SessionWatcher P1 (client {p1ClientId}): char {p1CharIndex} | P2 (client {p2ClientId}): char {p2CharIndex}");


            matchSetup.ActivateAndAssignOwnership(
                p1CharIndex, p1ClientId,
                p2CharIndex, p2ClientId
            );
        }
    }

    [ClientRpc]
    void RequestCharacterChoicesClientRpc() {
        if (NetworkManager.Singleton.LocalClient?.PlayerObject == null) {
            Debug.LogWarning("[SessionWatcher] LocalClient PlayerObject is null.");
            return;
        }

        var data = NetworkManager.Singleton.LocalClient.PlayerObject
                       .GetComponent<PlayerData>();

        if (data != null)
            data.SubmitCharacterChoice();
        else
            Debug.LogWarning("SessionWatcher PlayerData not found on local player object.");
    }
}