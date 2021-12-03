using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneStorage : MonoBehaviour {

	public int PlayerId;

	// Use this for initialization
	void Start () {

		// Create one stone for each placeholder spot
		InitStorage();
	}

	public GameObject StonePrefab;
	public Tile StartingTile;

	public void InitStorage() {

		PlayerStone[] stones = FindObjectsOfType<PlayerStone> ();

		for (int i = 0; i < stones.Length; i++) {
			if (stones [i].PlayerId == this.PlayerId) {
				Destroy (stones [i].gameObject);
			}
		}

		for (int i = 0; i < this.transform.childCount / 2 ; i++) {
			// Instantiate a new copy of the stone prefab
			GameObject theStone = Instantiate (StonePrefab);
			theStone.GetComponent<PlayerStone>().StartingTile = this.StartingTile;
			theStone.GetComponent<PlayerStone>().MyStoneStorage = this;
			AddStoneToStorage(theStone, this.transform.GetChild (i));
		}
	}
		
	
	public void AddStoneToStorage( GameObject theStone, Transform thePlaceholder = null) {

		if (thePlaceholder == null) {
			// Find the first empty placeholder
			for (int i = 0; i < this.transform.childCount / 2; i++) {
				Transform p = this.transform.GetChild (i);
				if (p.childCount == 0) {
					// This placeholder is empty
					thePlaceholder = p;
					break; // Break out of the loop
				}
			}

			if (thePlaceholder == null) {
				Debug.Log ("We're trying to add a stone, but we don't have empty places. How did this happen?");
				return;
			}
		}

		// Parent the stone to the placeholder
		theStone.transform.SetParent (thePlaceholder);

		// Resete the stone's local position to 0, 0, 0
		theStone.transform.localPosition = Vector3.zero; 
	}

	public void AddStoneToScoredPile(GameObject theStone, Transform thePlaceholder = null) {
		if (thePlaceholder == null) {
			// Find the first empty placeholder
			for (int i = this.transform.childCount / 2; i < this.transform.childCount; i++) {
				Transform p = this.transform.GetChild (i);
				if (p.childCount == 0) {
					// This placeholder is empty
					thePlaceholder = p;
					break; // Break out of the loop
				}
			}

			if (thePlaceholder == null) {
				Debug.Log ("We're trying to add a stone, but we don't have empty places. How did this happen?");
				return;
			}
		}

		// Parent the stone to the placeholder
		theStone.transform.SetParent (thePlaceholder);

		// Resete the stone's local position to 0, 0, 0
		theStone.transform.localPosition = Vector3.zero; 

	}

	public Transform GetScoredDestinationTransform() {
		for (int i = this.transform.childCount / 2; i < this.transform.childCount; i++) {
			Transform t = this.transform.GetChild (i);
			if (t.childCount == 0) {
				return t;
			}
		}

		// We should never arrive here
		Debug.LogError("We try to score a stone but the scored pile is full -- how has this happened?");
		return null;
	}

}
