using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {
    public Animator animator;
    private int currentIndex = 0;
    private int maxIndex = 3;
    private bool isMoving = false;

    void Update() {
        if (isMoving) return;

        if (Input.GetKeyDown(KeyCode.S)) {
            MoveDown();
        }
        else if (Input.GetKeyDown(KeyCode.W)) {
            MoveUp();
        }

        if (Input.GetKeyDown(KeyCode.Return)) {
            SelectOption();
        }
    }

    void MoveDown() {
        if (isMoving) return;
        currentIndex = (currentIndex + 1) % (maxIndex + 1);
        animator.SetTrigger("Down");
        StartCoroutine(MoveCooldown());
    }

    void MoveUp() {
        if (isMoving) return;
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = maxIndex;

        animator.SetTrigger("Up");
        StartCoroutine(MoveCooldown());
    }

    void SelectOption() {
        switch (currentIndex) {
            case 0:
                SceneManager.LoadScene("CharacterSelect");
                break;
            case 1:
                Debug.Log("Online (not implemented)");
                break;
            case 2:
                Debug.Log("Training (not implemented)");
                break;
            case 3:
                Debug.Log("Settings (not implemented)");
                break;
        }
    }

    System.Collections.IEnumerator MoveCooldown()
    {
        isMoving = true;
        yield return new WaitForSeconds(0.2f);
        isMoving = false;
    }
}