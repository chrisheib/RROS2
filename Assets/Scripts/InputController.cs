using UnityEngine;
using System.Collections;

public class InputController : MonoBehaviour {

	public float cameraVerticalMovespeed;
	public float cameraHorizontalMovespeed;
	public GameObject afterMined;

	Vector3 targetPosition;
	GameObject targetObject;
	UnityEngine.AI.NavMeshAgent agent;
	enum NavStatus {noAction, calculatingPath, movingAlongPath};
	NavStatus mainNavStatus = NavStatus.noAction;

	void Start(){
		agent = GameObject.FindWithTag("Worker").GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
	}

	// Update is called once per frame
	void Update () {
		CameraMovement();
		DetectClick ();
		CheckArrived ();
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

				if (objectHit.CompareTag("WallTile")) {
					//Debug.Log ("is wall tile");

					//move
					UnityEngine.AI.NavMeshPath path;
					if (findShortestPathToTile(agent,objectHit.transform.position,out path)) {
						agent.path = path;
						targetObject = objectHit;
						//Debug.Log (objectHit.name);
						mainNavStatus = NavStatus.movingAlongPath;	
					}
					//mine (objectHit);
				}
			}
		}
	}

	void CheckArrived(){
		if (mainNavStatus == NavStatus.movingAlongPath) {
			if (agent.remainingDistance == 0f) {
				mainNavStatus = NavStatus.noAction;
				mine (targetObject);	
			}
		}
	}

	void mine(GameObject tile){
		
		//Instantiate(afterMined, tile.transform.parent.position,Quaternion.identity);
		Destroy(tile.transform.gameObject);

	}

	float PathLength(UnityEngine.AI.NavMeshPath path) {
		if (path.corners.Length < 2)
			return 0;

		Vector3 previousCorner = path.corners[0];
		float lengthSoFar = 0.0F;
		int i = 1;
		while (i < path.corners.Length) {
			Vector3 currentCorner = path.corners[i];
			lengthSoFar += Vector3.Distance(previousCorner, currentCorner);
			previousCorner = currentCorner;
			i++;
		}
		return lengthSoFar;
	}

	public bool findShortestPathToTile(UnityEngine.AI.NavMeshAgent agent, Vector3 position, out UnityEngine.AI.NavMeshPath path){
		//Try to find a path
		UnityEngine.AI.NavMeshPath[] paths = new UnityEngine.AI.NavMeshPath[4];
		paths[0] = new UnityEngine.AI.NavMeshPath();
		paths[1] = new UnityEngine.AI.NavMeshPath();
		paths[2] = new UnityEngine.AI.NavMeshPath();
		paths[3] = new UnityEngine.AI.NavMeshPath();
		//Debug.Log (agent.gameObject.transform.position);
		//Debug.Log (objectHit.transform.position);

		//do 4 times: Calc path to offseted target, check if complete, return shortest 
		agent.CalculatePath(new Vector3(position.x+0.5f,position.y,position.z), paths[0]);
		agent.CalculatePath(new Vector3(position.x,position.y,position.z+0.5f), paths[1]);
		agent.CalculatePath(new Vector3(position.x+0.5f,position.y,position.z+1f), paths[2]);
		agent.CalculatePath(new Vector3(position.x+1f,position.y,position.z+0.5f), paths[3]);

		float shortestDistance = Mathf.Infinity;
		int shortestIndex = 0;
		bool foundViablePath = false;
		for (int i = 0; i < 4; i++) {
			if (paths [i].status == UnityEngine.AI.NavMeshPathStatus.PathComplete) {
				float thisDistance = PathLength (paths [i]);
				Debug.Log ("Path " + (i.ToString()) + " is viable. Length: " + thisDistance.ToString());
				if (thisDistance < shortestDistance) {
					shortestDistance = thisDistance;
					shortestIndex = i;
					foundViablePath = true;
				}
			}
		}

		if (foundViablePath) {
			path = paths [shortestIndex];
			return true;
		} else {
			path = null;
			return false;
		}

	}

}
