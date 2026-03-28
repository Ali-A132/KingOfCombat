using UnityEngine;
using Unity.Cinemachine;

public class MatchSetup : MonoBehaviour
{
    public CinemachineConfiner2D confiner;
    public GameObject mahsk;
    public GameObject mahskFlipped;
    public GameObject payet;
    public GameObject payetFlipped;

    public GameObject mahskTitleLeft;
    public GameObject mahskTitleRight;
    public GameObject payetTitleLeft;
    public GameObject payetTitleRight;

    public RoundManager roundManager;
    public CinemachineTargetGroup targetGroup;

    void Awake() {
        SetupCharacters();
    }

    void SetupCharacters() {
        mahsk.SetActive(false);
        mahskFlipped.SetActive(false);
        payet.SetActive(false);
        payetFlipped.SetActive(false);

        mahskTitleLeft.SetActive(false);
        mahskTitleRight.SetActive(false);
        payetTitleLeft.SetActive(false);
        payetTitleRight.SetActive(false);

        GameObject p1 = null;
        GameObject p2 = null;

        if (MatchData.p1Character == PlayerController.CharacterType.Mahsk) {
            p1 = mahsk;
            mahskTitleLeft.SetActive(true);
        } else {
            p1 = payetFlipped;
            payetTitleLeft.SetActive(true);
        }

 
        if (MatchData.p2Character == PlayerController.CharacterType.Mahsk) {
            p2 = mahskFlipped;
            mahskTitleRight.SetActive(true);
        } else {
            p2 = payet;
            payetTitleRight.SetActive(true);
        }

        p1.SetActive(true);
        p2.SetActive(true);

        targetGroup.Targets = new System.Collections.Generic.List<CinemachineTargetGroup.Target> {
            new CinemachineTargetGroup.Target{Object = p1.transform, Weight = 1f, Radius = 1f},
            new CinemachineTargetGroup.Target{Object = p2.transform, Weight = 1f, Radius = 1f}
        };
        roundManager.player1 = p1.GetComponent<PlayerController>();
        roundManager.player2 = p2.GetComponent<PlayerController>();
        StartCoroutine(RefreshConfiner());
    }

    System.Collections.IEnumerator RefreshConfiner() {
        yield return null;
        if (confiner != null) {
            confiner.InvalidateBoundingShapeCache();
        }
    }
}