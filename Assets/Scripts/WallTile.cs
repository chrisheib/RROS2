using UnityEngine;
using System.Collections;

public class WallTile : MonoBehaviour {

	public bool inMiningQueue = false;
	public Material matWhenInQueue;
	Renderer ownRenderer;
	bool materialSet = false;

	// Use this for initialization
	void Start () {
		ownRenderer = GetComponent<Renderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (inMiningQueue && !materialSet) {
			ownRenderer.material = matWhenInQueue;
			materialSet = true;
		}
	}
}
