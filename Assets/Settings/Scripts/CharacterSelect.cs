using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectController : MonoBehaviour {
    public Transform p1Cursor;
    public Transform p2Cursor;

    public GameObject mahskIdle;
    public GameObject mahskIdleFlipped;
    public GameObject payetIdle;
    public GameObject payetIdleFlipped;

    public GameObject mahskTitleCard;
    public GameObject payetTitleCard;
    public GameObject mahskTitleCardFlipped;
    public GameObject payetTitleCardFlipped;

    public Animator stageAnimator;

    public float[] positions = new float[3] { -1.414f, 0.051f, 1.483f };

    private int p1Index = 0;
    private int p2Index = 2;

    private bool p1Locked = false;
    private bool p2Locked = false;

    public GameObject chooseCharacterScreen;
    public GameObject stageSelectScreen;

    private int stageIndex = 0;
    private bool stageLocked = false;
    private bool inStageSelect = false;

    void Start() {
        UpdateCursors();
        UpdateCharacterDisplay();

        stageSelectScreen.SetActive(false);
    }

    void Update() {
        if (!inStageSelect) {
            HandleP1Input();
            HandleP2Input();
        }
        else {
            if (!stageLocked)
                HandleStageInput();
        }
    }

    void HandleP1Input() {
        if (p1Locked) return;

        if (Input.GetKeyDown(KeyCode.A)) {
            p1Index--;
            if (p1Index < 0) p1Index = 2;
            UpdateCursors();
            UpdateCharacterDisplay();
        }
        else if (Input.GetKeyDown(KeyCode.D)) {
            p1Index = (p1Index + 1) % 3;
            UpdateCursors();
            UpdateCharacterDisplay();
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            p1Locked = true;
            Debug.Log("P1 Locked: " + p1Index);
            CheckBothLocked();
        }
    }

    void HandleP2Input() {
        if (p2Locked) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            p2Index--;
            if (p2Index < 0) p2Index = 2;
            UpdateCursors();
            UpdateCharacterDisplay();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            p2Index = (p2Index + 1) % 3;
            UpdateCursors();
            UpdateCharacterDisplay();
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            p2Locked = true;
            Debug.Log("P2 Locked: " + p2Index);
            CheckBothLocked();
        }
    }

    void UpdateCursors() {
        p1Cursor.localPosition = new Vector3(positions[p1Index], p1Cursor.localPosition.y, 0f);
        p2Cursor.localPosition = new Vector3(positions[p2Index], p2Cursor.localPosition.y, 0f);
    }


    void UpdateCharacterDisplay() {
        mahskIdle.SetActive(false);
        mahskIdleFlipped.SetActive(false);
        payetIdle.SetActive(false);
        payetIdleFlipped.SetActive(false);

        payetTitleCard.SetActive(false);
        mahskTitleCard.SetActive(false);
        payetTitleCardFlipped.SetActive(false);
        mahskTitleCardFlipped.SetActive(false);

        if (p1Index == 0) {
            mahskIdle.SetActive(true);
            mahskTitleCard.SetActive(true);
        }
        else if (p1Index == 2) {
            payetIdleFlipped.SetActive(true);
            payetTitleCardFlipped.SetActive(true);
        } if (p2Index == 0) {
            mahskIdleFlipped.SetActive(true);
            mahskTitleCardFlipped.SetActive(true);
        } else if (p2Index == 2)
        {
            payetIdle.SetActive(true);
            payetTitleCard.SetActive(true);
        }
    }

    void CheckBothLocked() {
        if (p1Locked && p2Locked) {
            Debug.Log("Both players locked in!");
            int finalP1 = ResolveRandom(p1Index);
            int finalP2 = ResolveRandom(p2Index);
            Debug.Log("P1: " + finalP1);
            Debug.Log("P2: " + finalP2);

            foreach (Transform child in chooseCharacterScreen.transform) {
                child.gameObject.SetActive(false);
            }

            stageSelectScreen.SetActive(true);
            inStageSelect = true;
        }
    }

    int ResolveRandom(int index) {
        if (index == 1) {
            return Random.Range(0, 2) == 0 ? 0 : 2;
        }
        return index;
    }


    void HandleStageInput() {
        if (Input.GetKeyDown(KeyCode.W)) {
            if (stageIndex >= 2) {
                stageIndex -= 2;
                stageAnimator.SetTrigger("Up");
            }
        }
        else if (Input.GetKeyDown(KeyCode.S)) {
            if (stageIndex <= 1) {
                stageIndex += 2;
                stageAnimator.SetTrigger("Down");
            }
        }
        else if (Input.GetKeyDown(KeyCode.A)) {
            if (stageIndex % 2 == 1) {
                stageIndex -= 1;
                stageAnimator.SetTrigger("Left");
            }
        }
        else if (Input.GetKeyDown(KeyCode.D)) {
            if (stageIndex % 2 == 0) {
                stageIndex += 1;
                stageAnimator.SetTrigger("Right");
            }
        }

        if (Input.GetKeyDown(KeyCode.Return)) {
            LockStage();
        }
    }

    void LockStage() {
        stageLocked = true;
        int finalP1 = ResolveRandom(p1Index);
        int finalP2 = ResolveRandom(p2Index);
        MatchData.p1Character = (PlayerController.CharacterType)finalP1;
        MatchData.p2Character = (PlayerController.CharacterType)finalP2;
        MatchData.stageIndex = stageIndex;

        string sceneName = stageIndex switch {
            0 => "MoonColony",
            1 => "HouseOfWaffles",
            2 => "RoomOfSpaceAndTime",
            3 => "DojoInTheSky",
            _ => "MoonColony"
        };
        SceneManager.LoadScene(sceneName);
    }
}