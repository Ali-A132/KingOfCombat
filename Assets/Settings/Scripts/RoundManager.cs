using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RoundManager : NetworkBehaviour {
    protected Vector3 p1StartPos;
    protected Vector3 p2StartPos;
    public PlayerController player1;
    public PlayerController player2;
    public Image[] p1_Rounds;
    public Image[] p2_Rounds;
    public Image fadeImage;
    public CountDownTimer roundTimer;
    public RoundUIController roundWorldUI;

    public float fadeDuration = 1f;
    public int roundsToWin = 3;
    public float roundResetDelay = 2f;
    protected int p1Wins = 0;
    protected int p2Wins = 0;
    protected int currentRound = 1;
    public bool roundOver = false;
    protected bool roundStarting = false;
    protected bool tieGame = false;

    protected virtual void Start() {
        StartCoroutine(DelayedStart());
    }

    protected virtual IEnumerator DelayedStart() {
        yield return null;
        p1StartPos = player1.transform.position;
        p2StartPos = player2.transform.position;
        StartCoroutine(BeginMatch());
    }

    protected virtual IEnumerator BeginMatch() {
        yield return StartCoroutine(Fade(1f, 1f));
        currentRound = 1;
        yield return StartCoroutine(StartRoundSequence());
    }

    protected virtual IEnumerator StartRoundSequence() {
        if (roundStarting) yield break;

        roundStarting = true;
        roundOver = true;

        player1.LockControls();
        player2.LockControls();

        roundTimer.ResetTimer();

        yield return StartCoroutine(Fade(1f, 0f));
        if (roundWorldUI != null)
            yield return StartCoroutine(roundWorldUI.PlayRoundIntro(currentRound));

        player1.UnlockControls();
        player2.UnlockControls();
        roundOver = false;
        roundTimer.StartTimer();
        roundStarting = false;
    }

    protected virtual void EndRound() {
        roundOver = true;
        roundTimer.StopTimer();
    }

    public virtual void OnPlayerKO(PlayerController loser) {
        if (roundOver) return;
        StartCoroutine(ProcessKO(loser));
    }

    protected virtual IEnumerator ProcessKO(PlayerController loser) {
        yield return null;
        if (roundOver) yield break;

        EndRound();
        bool isTie = player1.currHealth <= 0f && player2.currHealth <= 0f;

        if (isTie) {
            tieGame = true;
        }
        else {
            tieGame = false;
            PlayerController winner = loser == player1 ? player2 : player1;

            if (loser == player1) {
                p2Wins++;
                UpdateRoundUI(p2_Rounds, p2Wins);
            }
            else {
                p1Wins++;
                UpdateRoundUI(p1_Rounds, p1Wins);
            }

            winner?.PlayVictoryTauntDelayed(2f);
        }
        CheckMatchEnd();
    }

    public virtual void OnTimeOver() {
        if (roundOver) return;
        EndRound();
        PlayerController winner = null;
        if (player1.currHealth > player2.currHealth)
            winner = player1;
        else if (player2.currHealth > player1.currHealth)
            winner = player2;

        if (winner != null) {
            tieGame = false;
            winner.PlayVictoryTauntDelayed(1.5f);
            if (winner == player1) { 
                p1Wins++; UpdateRoundUI(p1_Rounds, p1Wins); 
            }
            else { 
                p2Wins++; 
                UpdateRoundUI(p2_Rounds, p2Wins); 
            }
        }
        else {
            tieGame = true;
            player1.PlayVictoryTauntDelayed(2f);
            player2.PlayVictoryTauntDelayed(2f);
        }
        CheckMatchEnd();
    }

    protected virtual void CheckMatchEnd() {
        if (p1Wins >= roundsToWin || p2Wins >= roundsToWin)
            EndMatch();
        else {
            if (!tieGame) 
                currentRound++;
            StartCoroutine(RoundTransition());
        }
    }

    protected virtual void EndMatch() {
        Debug.Log("MATCH OVER");
        roundTimer.StopTimer();
    }

    protected virtual IEnumerator RoundTransition()
    {
        float delay = tieGame ? 3f : roundResetDelay;
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(Fade(0f, 1f));

        player1.LockControls();
        player2.LockControls();

        ResetPlayersPosition();
        player1.ResetForNewRound();
        player2.ResetForNewRound();

        yield return new WaitForSeconds(0.25f);
        yield return StartCoroutine(StartRoundSequence());
    }

    protected void UpdateRoundUI(Image[] rounds, int wins) {
        for (int i = 0; i < rounds.Length; i++)
            rounds[i].enabled = i < wins;
    }

    protected void ResetPlayersPosition() {
        player1.transform.position = p1StartPos;
        player2.transform.position = p2StartPos;
        var rb1 = player1.GetComponent<Rigidbody2D>();
        var rb2 = player2.GetComponent<Rigidbody2D>();
        if (rb1) rb1.linearVelocity = Vector2.zero;
        if (rb2) rb2.linearVelocity = Vector2.zero;
    }

    protected IEnumerator Fade(float from, float to) {
        float t = 0f;
        Color c = fadeImage.color;
        while (t < fadeDuration) {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }
        fadeImage.color = new Color(c.r, c.g, c.b, to);
    }
}