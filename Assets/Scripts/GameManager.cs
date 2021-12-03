using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public GameObject buttonsPanel;
	public GameObject newGamePanel;
	public GameObject settingsPanel;

	public Slider animationSpeed;
	public Text animationSpeedValue;

	public Slider soundVolume;
	public Text soundVolumeValue;

	public Toggle playerOneToggle;
	public Toggle playerTwoToggle;

	// Use this for initialization
	void Start () {
		GameSettings.IsAI0 = false;
		GameSettings.IsAI1 = false;
	}
	
	// Update is called once per frame
	void Update () {
		animationSpeedValue.text = animationSpeed.value.ToString ();
		GameSettings.animationSpeed = animationSpeed.value;

		soundVolumeValue.text = soundVolume.value.ToString ();
		GameSettings.soundVolume = soundVolume.value;
	}

	public void NewGame() {
		buttonsPanel.SetActive (false);
		newGamePanel.SetActive (true);
	}

	public void StartGame() {
		SceneManager.LoadSceneAsync (1);
	}

	public void GoBack() {
		buttonsPanel.SetActive (true);
		newGamePanel.SetActive (false);
		settingsPanel.SetActive (false);
	}

	public void Exit() {
		Application.Quit ();
	}

	public void OnValueChangedPlayerOneToggle() {
		if (playerOneToggle.isOn == true) {
			GameSettings.IsAI0 = false;
		} else {
			GameSettings.IsAI0 = true;
		}
	}

	public void OnValueChangedPlayerTwoToggle() {
		if (playerTwoToggle.isOn == true) {
			GameSettings.IsAI1 = false;
		} else {
			GameSettings.IsAI1 = true;
		}
	}


	public void Settings() {
		settingsPanel.SetActive (true);
		buttonsPanel.SetActive (false);
		newGamePanel.SetActive (false);
	}
}
