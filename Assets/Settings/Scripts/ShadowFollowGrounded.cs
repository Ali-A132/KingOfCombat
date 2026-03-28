using UnityEngine;

[System.Serializable]
public class StageShadowData
{
    public float mahskY;
    public float payetY;
}

public class ShadowFollowGrounded : MonoBehaviour
{
    public Transform player;
    public StageShadowData[] stageShadowData = new StageShadowData[4];

    float GetGroundY()
    {
        if (player == null) return -10f;
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller == null) return -10f;

        int stage = MatchData.stageIndex;
        if (stage < 0 || stage >= stageShadowData.Length)
            return -10f;

        StageShadowData data = stageShadowData[stage];
        return controller.characterType == PlayerController.CharacterType.Mahsk ? data.mahskY : data.payetY;
    }

    void LateUpdate()
    {
        if (!player) return;

        transform.position = new Vector3(
            player.position.x,
            GetGroundY(),
            player.position.z
        );
    }
}