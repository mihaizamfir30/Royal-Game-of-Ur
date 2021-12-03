using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour {

	public Slider animationSpeed;
	public Text animationSpeedValue;

	public Slider soundVolume;
	public Text soundVolumeValue;

	// Use this for initialization
	void Start () {
		animationSpeed.value = GameSettings.animationSpeed;
		soundVolume.value = GameSettings.soundVolume;
	}
	
	// Update is called once per frame
	void Update () {
		animationSpeedValue.text = animationSpeed.value.ToString ();
		GameSettings.animationSpeed = animationSpeed.value;

		soundVolumeValue.text = soundVolume.value.ToString ();
		GameSettings.soundVolume = soundVolume.value;
	}
}
