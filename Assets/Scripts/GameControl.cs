using UnityEngine;
using System.Collections;

public class GameControl : MonoBehaviour {

	public GameObject wallPrefab;
	public GameObject floorPrefab;
	public GameObject wallWithEnergyCrystalPrefab;
	public GameObject[,] map = new GameObject[50,50];
	public GameObject floor;
	public GameObject player;

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
	
	// Update is called once per frame
	void Update () {
	
	}
}