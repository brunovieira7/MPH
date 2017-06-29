using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;
using System;

public class GameManager : MonoBehaviour {

	public GameObject[] gems;
	public Player player;
	public Enemy enemy;
	private string currentCast;

	public Image timeBar;
	public float maxCastTime;
	private float elapsedTime = 0f;

	private bool jutsuStage = true;
	private Jutsu castingJutsu;
	private float castingJutsuTimer = 0f;

	public Text recordText;
	public List<GameObject> instanceSeals;

	// Use this for initialization
	void Start () {
		//populateGemsFull ();
		shuffleGems ();
		currentCast = "";
	}

	// Update is called once per frame
	void Update () {

		updateBars ();

		if (jutsuStage) {
			if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
				if (Input.touchCount > 0) {
					if (Input.GetTouch (0).phase == TouchPhase.Began) {
						checkTouch (Input.GetTouch (0).position);
					}
				}
			} else if (Application.platform == RuntimePlatform.WindowsEditor) {
				if (Input.GetMouseButtonDown (0)) {
					checkTouch (Input.mousePosition);
				}
			}
		}

		if (castingJutsu != null) {
			castingJutsuTimer += Time.deltaTime;
			if (castingJutsuTimer >= castingJutsu.getActiveTimer ()) {
				Enemy enemyScript = enemy.GetComponent<Enemy> ();
				enemyScript.takeDamage ();
				castingJutsu = null;

				restartRound ();
			}
		}
	}

	void restartRound() {
		destroyGems ();
		shuffleGems ();
		elapsedTime = 0f;
		jutsuStage = true;
		castingJutsuTimer = 0f;
		currentCast = "";
	}
		
	void updateBars() {
		if (!jutsuStage) {
			return;
		}

		elapsedTime += Time.deltaTime;

		RectTransform rect = timeBar.GetComponent<RectTransform> ();

		float newScale = (float) elapsedTime / maxCastTime;

		Vector3 currScale = rect.localScale;
		currScale.x = newScale;

		rect.localScale = currScale;

		if (elapsedTime >= maxCastTime) {
			jutsuStage = false;
			elapsedTime = 0f;
		}
	}

	private void checkTouch(Vector3 pos){
		Vector3 wp = Camera.main.ScreenToWorldPoint(pos);
		Vector2 touchPos = new Vector2(wp.x, wp.y);
	     var hit = Physics2D.OverlapPoint(touchPos);
	     
	     if (hit) {
			Animator gem = hit.GetComponent<Animator> ();
			gem.SetTrigger ("explode");
			gem.SetInteger ("color", Int32.Parse (hit.GetComponent<SealClick> ().getSealCode ()));
			Destroy (hit.gameObject, gem.GetCurrentAnimatorStateInfo(0).length + 0f); 

			string sealCode = hit.GetComponent<SealClick> ().getSealCode ();
			player.GetComponent<Player> ().castSeal ();

			//Debug.Log ("got code " + sealCode);
			currentCast = currentCast + sealCode;
			Jutsu jutsu = player.GetComponent<Player> ().isCastSuccessful (currentCast);
			if (jutsu != null) {
				jutsuStage = false;
				castingJutsu = jutsu;
				//jutsu.playSound ();

				if (jutsu.newBestTime (elapsedTime)) {
					recordText.text = "New  Best  Time:  " + System.Math.Round (elapsedTime, 2);
					recordText.enabled = true;
				} 
				else {
					recordText.enabled = false;
				}
			}
			//Debug.Log(hit.transform.gameObject.name);
			//Debug.Log ("DOWNZZZ" + sealNum);


	     }
	 }

	private void checkTouchOld(Vector3 pos){
		Vector3 wp = Camera.main.ScreenToWorldPoint(pos);
		Vector2 touchPos = new Vector2(wp.x, wp.y);
		var hit = Physics2D.OverlapPoint(touchPos);

		if (hit) {


			string sealCode = hit.GetComponent<SealClick> ().getSealCode ();
			player.GetComponent<Player> ().castSeal ();

			//Debug.Log ("got code " + sealCode);
			currentCast = currentCast + sealCode;
			Jutsu jutsu = player.GetComponent<Player> ().isCastSuccessful (currentCast);
			if (jutsu != null) {
				jutsuStage = false;
				castingJutsu = jutsu;
				//jutsu.playSound ();

				if (jutsu.newBestTime (elapsedTime)) {
					recordText.text = "New  Best  Time:  " + System.Math.Round (elapsedTime, 2);
					recordText.enabled = true;
				} 
				else {
					recordText.enabled = false;
				}
			}
			//Debug.Log(hit.transform.gameObject.name);
			//Debug.Log ("DOWNZZZ" + sealNum);


		}
	}


	void destroyGems() { 
		foreach (GameObject go in instanceSeals) {
			Destroy (go);
		}
	}

	void populateGemsFull() {
		GameObject[] gemsClone = (GameObject[])gems.Clone ();
		gems = new GameObject[18];
		List<GameObject> list1 = new List<GameObject>(gemsClone);
		List<GameObject> listFin = new List<GameObject>(gemsClone);
		listFin.AddRange (list1);
		listFin.AddRange (list1);
		Debug.Log ("1size : " + gemsClone.Length);

		gems = listFin.ToArray ();
	}

	void shuffleGems() {
		instanceSeals = new List<GameObject> ();
		GameObject[] sealClone = (GameObject[])gems.Clone ();

		float y = 1.1f;
		int x = -2;

		Debug.Log ("size : " + sealClone.Length);
		for (int i = 0; i < gems.Length; i++) {
			GameObject instance = getRandomSeal (sealClone);

			List<GameObject> list = new List<GameObject>(sealClone);
			list.Remove (instance);
			sealClone = list.ToArray ();

			float xOffset = (float) ((i % 4) * 0.3);

			if (i % 4 == 0) {
				y -= 1.7f;
				x = -2;
			}
			instance = Instantiate (instance, new Vector3(x + xOffset,y,0), Quaternion.identity) as GameObject;
			instanceSeals.Add (instance);
			x++;
		}

		//GameObject instance = Instantiate (bullet, start, Quaternion.identity) as GameObject;

		//GameObject toInstantiate = seals[Random.Range (0,seals.Length)];
	}

	GameObject getRandomSeal(GameObject[] seals) {
		return seals[UnityEngine.Random.Range (0,seals.Length)];
	}
}