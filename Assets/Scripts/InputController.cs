using UnityEngine;
using System.Collections;

public class InputController : MonoBehaviour {

	public float cameraVerticalMovespeed;
	public float cameraHorizontalMovespeed;
	public GameObject afterMined;
	GameControl gameControl;

	Vector3 targetPosition;
	GameObject targetObject;
	//UnityEngine.AI.NavMeshAgent agent;
	//enum NavStatus {noAction, calculatingPath, movingAlongPath};
	//NavStatus mainNavStatus = NavStatus.noAction;

	void Start(){
		//agent = GameObject.FindWithTag("Worker").GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
		gameControl = FindObjectOfType<GameControl> ();
	}

	// Update is called once per frame
	void Update () {
		CameraMovement();
		DetectClick ();
	}
		
	void CameraMovement(){
		float horizontalMovement = Input.GetAxis ("Horizontal");
		float verticalMovement = Input.GetAxis ("Vertical");

		horizontalMovement = horizontalMovement * Time.deltaTime * cameraHorizontalMovespeed;
		verticalMovement = verticalMovement * Time.deltaTime * cameraVerticalMovespeed;

		GameObject camera = GameObject.FindWithTag ("MainCamera");

		float oldYPosition = camera.transform.position.y;
		camera.transform.Translate (horizontalMovement, verticalMovement, 0);
		Vector3 pos = camera.transform.position;
		pos.y = oldYPosition;
		camera.transform.position = pos;
	}

	void DetectClick(){
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			Camera camera = Camera.main;
			Ray ray = camera.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast(ray, out hit)) {
				GameObject objectHit = hit.transform.parent.gameObject;
				Debug.Log ("hit" + hit.transform.position.ToString());

				//if (objectHit.CompareTag("WallTile")) {
				WallTileScript wall = objectHit.GetComponent<WallTileScript>();
				if (wall){
					Debug.Log ("is wall tile");
					gameControl.addMiningOrder(new MiningOrder(wall));


					//move
					//UnityEngine.AI.NavMeshPath path;
					//if (findShortestPathToTile(agent,objectHit.transform.position,out path)) {
					//	agent.path = path;
					//	targetObject = objectHit;
					//	//Debug.Log (objectHit.name);
					//	mainNavStatus = NavStatus.movingAlongPath;	
					//}
					//mine (objectHit);
				}
			}
		}
	}
}
