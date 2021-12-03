﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStone : MonoBehaviour {

	// Use this for initialization
	void Start () {
		theStateManager = GameObject.FindObjectOfType<StateManager> ();
		theMouseController = GameObject.FindObjectOfType<MouseController> ();

		targetPosition = this.transform.position;
	}

	public Tile StartingTile;
	public Tile CurrentTile {get; protected set; }

	public int PlayerId;
	public StoneStorage MyStoneStorage;

	StateManager theStateManager;
	MouseController theMouseController;

	Tile[] moveQueue;
	int moveQueueIndex;

	bool isAnimating = false;

	Vector3 targetPosition;
	Vector3 velocity = Vector3.zero;
	float smoothTime = 0.25f;
	float smoothTimeVertical = 0.1f;
	float smoothDistance = 0.01f;
	float smoothHeight = 0.5f;

	PlayerStone stoneToBop;

	public Tile destinationTile;

	bool scoreMe = false;
	bool selectMe = false;

	// Update is called once per frame
	void Update()
	{
		if (isAnimating == false)
		{
			// Nothing for us to do.
			return;
		}

		if (Vector3.Distance(
			new Vector3(this.transform.position.x, targetPosition.y, this.transform.position.z),
			targetPosition) < smoothDistance)
		{				
			// We've reached the target position --  do we still have moves in the queue?
			if (
				(moveQueue == null || moveQueueIndex == (moveQueue.Length)) 
				&&
				((this.transform.position.y - smoothDistance) > targetPosition.y)
			)
			{
				// The movement queue is empty, if the stone was scored move it to the scored pile 
				if (scoreMe == true) {
					ScoreThisStone ();
					return;
				}

				// We are totally out of moves (and too high up), the only thing left to do is drop down
				this.transform.position = Vector3.SmoothDamp(
					this.transform.position, 
					new Vector3(this.transform.position.x, targetPosition.y, this.transform.position.z), 
					ref velocity, 
					smoothTimeVertical / GameSettings.animationSpeed);

				// Check for bops
				if (stoneToBop != null) {
					stoneToBop.ReturnToStorage ();
					stoneToBop = null;
				}
			}
			else
			{
				// Right position, right height -- let's advance the queue
				AdvanceMoveQueue();
			}
		}
		else if (this.transform.position.y < (smoothHeight - smoothDistance))
		{
			// We want to rise up before we move sideways.
			this.transform.position = Vector3.SmoothDamp(
				this.transform.position, 
				new Vector3(this.transform.position.x, smoothHeight, this.transform.position.z), 
				ref velocity, 
				smoothTimeVertical / GameSettings.animationSpeed);
		}
		else
		{
			// Normal movement (sideways)
			this.transform.position = Vector3.SmoothDamp(
				this.transform.position, 
				new Vector3(targetPosition.x, smoothHeight, targetPosition.z), 
				ref velocity, 
				smoothTime / GameSettings.animationSpeed);
		}

	}
		
	void AdvanceMoveQueue() {
		if (moveQueue != null && moveQueueIndex < moveQueue.Length) {
			Tile nextTile = moveQueue [moveQueueIndex];

			if (nextTile.IsScoringSpace == true) {
				// We are probably being scored. We want to move above our place in the scored piled.
				Vector3 pos = MyStoneStorage.GetScoredDestinationTransform().position + 1 * Vector3.up;
				SetNewTargetPosition (pos);
				moveQueueIndex++;
			} else {
				SetNewTargetPosition (nextTile.transform.position);
				moveQueueIndex++;
			}
		} else {
			// The movement queue is empty, so we are done animating.
			this.isAnimating = false;
			//theStateManager.IsDoneAnimating = true;
			theStateManager.AnimationsPlaying--;

			// Are we on a roll again space?
			if (CurrentTile != null && CurrentTile.IsRollAgain == true) {
				theStateManager.RollAgain ();
			}
		}
	}

	void SetNewTargetPosition( Vector3 pos ) {
		targetPosition = pos;
		velocity = Vector3.zero;
		isAnimating = true;
	}

	void OnMouseDown() {
		selectMe = true;
	}

	void OnMouseUp() {
		if (selectMe == true) {
			// TODO: Is the mouse over a UI element? In which case, ignore this click.
			MoveMe ();
		}
	}

	void OnMouseEnter() {
		// If this is our stone, we have rolled the dice and no animations are playing
		if (theStateManager.CurrentPlayerId == this.PlayerId && theStateManager.PlayerAIs[this.PlayerId] == null && theStateManager.IsDoneRolling && theStateManager.AnimationsPlaying == 0) {
			HighlightDestination ();
		}
	}

	void OnMouseOver() {
	}
		
	void OnMouseExit() {
		selectMe = false;
		theMouseController.highlight.SetActive (false);
	}

	public void MoveMe() {
		// Is this the correct player?
		if (theStateManager.CurrentPlayerId != this.PlayerId) {
			return;
		}

		// Have we rolled the dice?
		if (theStateManager.IsDoneRolling == false) {
			// We can't move yet.
			return;
		}

		if (theStateManager.IsDoneClicking == true) {
			// We've already done a move!
			return;
		}

		// Where should we end up?
		int spacesToMove = theStateManager.DiceTotal;

		// If we rolled a zero, the player's turn ends immediately.
		if (spacesToMove == 0) {
			return;
		}

		// Where should we end up?
		moveQueue = GetTilesAhead(spacesToMove);
		Tile finalTile = moveQueue [moveQueue.Length - 1];

		// Check to see if the destination is legal!
		if (CanLegallyMoveTo(finalTile) == false) {
			// Not allowed!
			finalTile = CurrentTile;
			moveQueue = null;
			return;
		}

		// If there is an enemy tile in our legal space, then we kick it out
		if (finalTile.PlayerStone != null) {
			stoneToBop = finalTile.PlayerStone;
			stoneToBop.CurrentTile.PlayerStone = null;
			stoneToBop.CurrentTile = null;
		}

		// If the final tile in the queue is the destination tile, we must place the stone in the scored pile 
		if (finalTile.IsScoringSpace == true) {
			scoreMe = true;
		}

		this.transform.SetParent (null); // Become Batman

		// Remove ourselves from our old tile
		if (CurrentTile != null) {
			CurrentTile.PlayerStone = null;
		}

		// Even before the animation is done, set our current tile to the new tile
		CurrentTile = finalTile;
		if (finalTile.IsScoringSpace == false) { // "Scoring" tiles are always "empty"
			finalTile.PlayerStone = this;
		}
			
		moveQueueIndex = 0;

		theStateManager.IsDoneClicking = true;
		this.isAnimating = true;
		theStateManager.AnimationsPlaying++;
	}

	public void HighlightDestination() {
		Tile destinationTile = GetTileAhead(theStateManager.DiceTotal);
		if (CanLegallyMoveTo (destinationTile) == false) {
			return;
		}

		theMouseController.highlight.SetActive (true);

		if (destinationTile.IsScoringSpace == true) {
			theMouseController.highlight.transform.position = MyStoneStorage.GetScoredDestinationTransform().position;
		} else {
			theMouseController.highlight.transform.position = destinationTile.transform.position;
		}
	}
		
	// Return the list of tiles __ moves ahead of us
	Tile[] GetTilesAhead(int spacesToMove) {
		if (spacesToMove == 0) {
			return null;
		}

		// Where should we end up?

		Tile[] listOfTiles = new Tile[spacesToMove];
		Tile finalTile = CurrentTile;

		for (int i = 0; i < spacesToMove; i++) {
			if (finalTile == null) {
				finalTile = StartingTile;
			} else {
				if (finalTile.NextTiles == null || finalTile.NextTiles.Length == 0) {
					// This means we are overshooting the victory -- so just return some nulls in the array
					// Just break and we'll return the array, which is going to have nulls
					// at the end.
					break;
				} else if (finalTile.NextTiles.Length > 1) {
					// Branch based on player id
					finalTile = finalTile.NextTiles [this.PlayerId];
				} else {
					finalTile = finalTile.NextTiles [0];
				}
			}

			listOfTiles [i] = finalTile;
		}

		return listOfTiles;

	}

	public Tile GetTileAhead() {
		return GetTileAhead (theStateManager.DiceTotal);
	}

	// Return the final tile we'd land on if we moved __ spaces
	public Tile GetTileAhead(int spacesToMove) {
		Tile[] tiles = GetTilesAhead (spacesToMove);

		if (tiles == null) {
			// We aren't moving at all, so just return our current tile?
			return CurrentTile;
		}

		return tiles[tiles.Length - 1];
	}
		
	public bool CanLegallyMoveAhead (int spacesToMove) {
		Tile theTile = GetTileAhead (spacesToMove);
		return CanLegallyMoveTo (theTile);
	}

	bool CanLegallyMoveTo(Tile destinationTile) {

		if (destinationTile == null) {

			// NOTE!  A null tile means we are overshooting the victory roll
			// and this is NOT legal (apparently) in the Royal Game of Ur
			return false;
		}

		// Is the tile empty
		if (destinationTile.PlayerStone == null) {
			return true;		
		}

		// Is it one of our stones?
		if (destinationTile.PlayerStone.PlayerId == this.PlayerId) {
			// We can't land on our own stone.
			return false;
		}

		// If it's an enemy stone, is it in a safe square?
		if (destinationTile.IsRollAgain == true) {
			// Can't bop someone on a safe tile!
			return false;
		}

		// If we've gotten here, it means we can legally land on the enemy stone and
		// kick it off the board.
		//destinationTile.PlayerStone.ReturnToStorage ();
		return true;
	}

	public void ReturnToStorage() {

		moveQueue = null;

		this.isAnimating = true;
		theStateManager.AnimationsPlaying++;

		// Save our current position
		Vector3 savePosition = this.transform.position;

		MyStoneStorage.AddStoneToStorage (this.gameObject);

		// Set our new position to the animation target
		SetNewTargetPosition (this.transform.position);

		// Restore our saved position
		this.transform.position = savePosition;
	}

	public void ScoreThisStone() {

		scoreMe = false;
		theStateManager.Scores [PlayerId]++;

		moveQueue = null;

		// Save our current position
		Vector3 savePosition = this.transform.position;

		MyStoneStorage.AddStoneToScoredPile (this.gameObject);

		// Set our new position to the animation target
		SetNewTargetPosition (this.transform.position);

		// Restore our saved position
		this.transform.position = savePosition;
	}
}
