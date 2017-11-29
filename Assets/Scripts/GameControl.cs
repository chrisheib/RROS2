using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovementOrder{

	public GameObject target;

}

public class MiningOrder{

	public WallTileScript target;

	//Toolrequirementcheck etc
	public MiningOrder(WallTileScript target){
		this.target = target;
	}

}

public class WorkerQuad{
	public Worker worker{get;set;}
	public UnityEngine.AI.NavMeshAgent navAgent{get;set;}
	public float distance{get;set;}
	public UnityEngine.AI.NavMeshPath path{get;set;}

	public WorkerQuad(Worker worker,  UnityEngine.AI.NavMeshAgent navAgent, float distance,UnityEngine.AI.NavMeshPath path){
		this.worker = worker;
		this.navAgent = navAgent;
		this.distance = distance;
		this.path = path;
	}
}

public class GameControl : MonoBehaviour {

	public GameObject wallPrefab;
	public GameObject floorPrefab;
	public GameObject wallWithEnergyCrystalPrefab;
	public GameObject[,] map = new GameObject[50,50];
	public GameObject floor;
	public GameObject player;

	int freeWorkerCount = 0;

	List<MiningOrder> miningOrders = new List<MiningOrder>();

	// Use this for initialization
	void Start () {
		for (int i = 0; i < map.GetLength(0); i++) {
			for (int j = 0; j < map.GetLength(1); j++) {
				bool willBeFloorTile = (Random.value < 0.2);
				if (willBeFloorTile) {
					//map[i,j] = (GameObject)	Instantiate (floorPrefab, new Vector3 (i + 0.5f, 0, j + 0.5f), Quaternion.identity);
				} else {
					float distanceFromPlayer = (player.transform.position - new Vector3 (i + 0.5f, 0, j + 0.5f)).magnitude;
					bool isFarEnoughFromPlayer = distanceFromPlayer > 3;
					if (!isFarEnoughFromPlayer) {
						continue;
					}
					bool hasEnergyCrystal = (Random.value < 0.05);
					if (!hasEnergyCrystal) {
						//map[i,j] = (GameObject)	Instantiate (wallPrefab, new Vector3 (i + 0.5f, 0, j + 0.5f), Quaternion.identity, floor.transform);
						map[i,j] = (GameObject)	Instantiate (wallPrefab, new Vector3 (i + 0.5f, 0, j + 0.5f), Quaternion.identity);
					} else {
						//map[i,j] = (GameObject)	Instantiate (wallWithEnergyCrystalPrefab, new Vector3 (i + 0.5f, 0, j + 0.5f), Quaternion.identity, floor.transform);
						map[i,j] = (GameObject)	Instantiate (wallWithEnergyCrystalPrefab, new Vector3 (i + 0.5f, 0, j + 0.5f), Quaternion.identity);
					}
				}
			}
		}
	}

	void FixedUpdate () {
		processMiningOrders ();

	}

	void processMiningOrders(){
		//if stack not empty, select nearest reachable free worker and let it do the job. if not reachable, but freeWorkerCount > 0, check if other orders can be reached. 
		for (int i = 0; (i < miningOrders.Count) && (freeWorkerCount > 0); i++) {

			if (miningOrders.Count > 0) {
				WallTileScript target = miningOrders [i].target;
				List<Worker> freeWorkers = findFreeWorkers ();

				List<WorkerQuad> workerDistanceList = new List<WorkerQuad>();

				bool viableWorkerFound = false;

				if (miningOrders[i].target == null) {
					miningOrders.RemoveAt(i);
					i--;
				}

				//find all free workers who are in range of target
				foreach (Worker worker in freeWorkers) {
					UnityEngine.AI.NavMeshPath path;
					UnityEngine.AI.NavMeshAgent agent = worker.navAgent;
					float shortestDistance;
					bool pathFound = findShortestPathToTile(agent, target.transform.position, out path, out shortestDistance);
					if (pathFound) {
						workerDistanceList.Add (new WorkerQuad(worker,agent,shortestDistance, path));
						viableWorkerFound = true;
					}
				}

				//if no viable worker is found, check next miningOrder
				if (!viableWorkerFound) {
					continue;
					//cant be reached :( delete from order list. 

					//miningOrders.RemoveAt(i);
					//i--;

				}

				//if at least one worker is found, select the one who is closest to target
				float shortestPathDistance = Mathf.Infinity;
				int shortestPathIndex = 0;

				for (int j = 0; j < workerDistanceList.Count; j++) {
					if (workerDistanceList[j].distance < shortestPathDistance ) {
						shortestPathDistance = workerDistanceList [j].distance;
						shortestPathIndex = j;
					}
				}

				UnityEngine.AI.NavMeshPath shortestPath = workerDistanceList [shortestPathIndex].path;
				Worker workerOfChoice = workerDistanceList [shortestPathIndex].worker;

				workerOfChoice.mineTile (target, shortestPath);

				miningOrders.RemoveAt(i);
				i--;

			}	
		}
	}

	public void addMiningOrder(MiningOrder miningOrder){
		WallTileScript wall = miningOrder.target;
		if (wall.inMiningQueue) {
			Debug.Log ("Already in mining queue");
			return;
		}
		wall.inMiningQueue = true;
		miningOrders.Add (miningOrder);
	}

	List<Worker> findFreeWorkers(){
		Worker[] workers = FindObjectsOfType<Worker>();
		List<Worker> freeWorkers = new List<Worker>();
		foreach (var worker in workers) {
			if (worker.state == Worker.State.free) {
				freeWorkers.Add (worker);
				Debug.Log ("added to free workers: " + worker.name);
			}
		}
		return freeWorkers;
	}

	public void incFreeWorkerCount(){
		freeWorkerCount++;
	}

	public void decFreeWorkerCount(){
		freeWorkerCount--;
	}
		
	//copied from unity manual
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

	public bool findShortestPathToTile(UnityEngine.AI.NavMeshAgent agent, Vector3 position, out UnityEngine.AI.NavMeshPath path, out float shortestDistance){
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

		shortestDistance = Mathf.Infinity;
		int shortestIndex = 0;
		bool foundViablePath = false;
		for (int i = 0; i < 4; i++) {
			if (paths [i].status == UnityEngine.AI.NavMeshPathStatus.PathComplete) {
				float thisDistance = PathLength (paths [i]);
				//Debug.Log ("Path " + (i.ToString()) + " is viable. Length: " + thisDistance.ToString());
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