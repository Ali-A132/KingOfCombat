using Unity.Netcode;
using UnityEngine;


public class PlayerData : NetworkBehaviour {

    public NetworkVariable<int> characterIndex = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );


    public NetworkVariable<bool> hasSubmitted = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn() {
 
    }


    public void SubmitCharacterChoice() {
        if (!IsOwner) return;

        int choice = SessionWatcher.SelectedCharacter;
        Debug.Log($"[PlayerData] Submitting character choice: {choice}");
        SetCharacterServerRpc(choice);
    }


    [ServerRpc]
    public void SetCharacterServerRpc(int index) {
        characterIndex.Value = index;
        hasSubmitted.Value = true;
        Debug.Log($"[PlayerData] Server received character {index} from client {OwnerClientId}");
    }
}