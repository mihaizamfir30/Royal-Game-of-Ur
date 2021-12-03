using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StateManager : MonoBehaviour {

	public Button RestartButton;
	public Button SettingsButton;

	public Slider animationSpeed;
	public Text animationSpeedValue;

	public GameObject settingsPanel;

	public AIPlayer[] PlayerAIs;
	public int[] Scores;

	public int NumberOfPlayers = 2;
	public int CurrentPlayerId = 0;

	public int DiceTotal;

	// NOTE: enum/statemachine is probably a stronger choice, but I'm aiming for simpler to explain.
	public bool IsDoneRolling = false;
	public bool IsDoneClicking = false;
	// public bool IsDoneAnimating = false;
	public int AnimationsPlaying = 0;

	bool isOver = false;

	public GameObject NoLegalMovesPopup;
	public GameObject GameOverPopup;

	// Use this for initialization
	void Start () {
		InitBoard ();
	}

	public void InitBoard() {

		StoneStorage[] storages = FindObjectsOfType<StoneStorage> ();

		for (int i = 0; i < storages.Length; i++) {
			storages [i].InitStorage ();
		}

		PlayerAIs = new AIPlayer[NumberOfPlayers];
		Scores = new int[NumberOfPlayers];

		if (GameSettings.IsAI0 == true) {
			PlayerAIs [0] = new AIPlayer_UtilityAI (); 
		} else {
			PlayerAIs [0] = null; // Is a human player
		}

		if (GameSettings.IsAI1 == true) {
			PlayerAIs [1] = new AIPlayer_UtilityAI (); 
		} else {
			PlayerAIs [1] = null; // Is a human player
		}
			
		IsDoneRolling = false;
		IsDoneClicking = false;
		isOver = false;
		AnimationsPlaying = 0;

		CurrentPlayerId = 0;

		GameOverPopup.SetActive (false);
	}
		
	public void NewTurn() {
		
		// This is the start of a player's turn.
		// We don't have a roll for them yet.
		IsDoneRolling = false;
		IsDoneClicking = false;

		// Advance Player
		CurrentPlayerId = (CurrentPlayerId + 1) % NumberOfPlayers;
	}

	public void RollAgain() {
		// This is the start of a player's turn.
		// We don't have a roll for them yet.
		IsDoneRolling = false;
		IsDoneClicking = false;
	}

	public void GameOver() {

		Text text = GameOverPopup.GetComponentInChildren<Text> ();

		if (Scores [0] == 6) {
			text.text = "Player One Wins";
		} else {
			text.text = "Player Two Wins";
		}
		GameOverPopup.SetActive (true);

		isOver = true;
	}

	// Update is called once per frame
	void Update () {

		if (AnimationsPlaying == 0) {
			RestartButton.interactable = true;
			SettingsButton.interactable = true;
		} else {
			RestartButton.interactable = false;
			SettingsButton.interactable = false;
		}

		// Is the turn done?
		if (IsDoneRolling && IsDoneClicking && AnimationsPlaying == 0 && isOver == false) {

			// Check to see if the current player has won the game
			if (Scores[CurrentPlayerId] == 6) {
				GameOver();
				// return;
			} else {
				// If not, go to next roundv
				NewTurn ();
				return;
			}
		}

		if (PlayerAIs [CurrentPlayerId] != null) {
			PlayerAIs [CurrentPlayerId].DoAI ();
		}
	}

	public void CheckLegalMoves() {

		// If we rolled a zero, then we clearly have no legal moves
		if (DiceTotal == 0) {
			StartCoroutine (NoLegalMoveCoroutine());
			return;
		}

		// Loop through all of player's stones
		PlayerStone[] pss = GameObject.FindObjectsOfType<PlayerStone>();
		bool hasLegalMove = false;

		foreach (PlayerStone ps in pss) {
			if (ps.PlayerId == CurrentPlayerId) {
				if (ps.CanLegallyMoveAhead (DiceTotal)) {
					// TODO: Highlight stones that can be legally moved
					hasLegalMove = true;
				}
			}
		}

		// If no legal moves are possible, wait a sec then move to next player (probably give message)
		if (hasLegalMove == false) {
			StartCoroutine (NoLegalMoveCoroutine());
		}
	}

	IEnumerator NoLegalMoveCoroutine() {

		// Display message
		NoLegalMovesPopup.SetActive (true);

		// TODO: Trigger animations like have the stones shake or something
	
		// Wait 1 second
		yield return new WaitForSeconds (1f);

		// Clear message
		NoLegalMovesPopup.SetActive (false);

		NewTurn ();
	}

	public void Settings() {
		settingsPanel.SetActive (true);
	}

	public void GoBack() {
		settingsPanel.SetActive (false);
	}

	public void Exit() {
		SceneManager.LoadSceneAsync (0);
	}
}
