// ====================================================================================================================
//
// Created by Leslie Young
// http://www.plyoung.com/ or http://plyoung.wordpress.com/
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RadiusMarker : MonoBehaviour
{
	// ====================================================================================================================
	#region inspector properties

	public GameObject prefab;
	public MapNav.TilesLayout markerLayout = MapNav.TilesLayout.Hex;	// kind of tile layout
	public float markerSize = 1f;			// the size of one marker node (normally same size as the tiles)
	public float markerSpacing = 1f;		// spacing between amrker nodes (normally same as tile)
	public int markerRadius = 1;			// furthest tile this marker could indicate

	// the marker node groups are saved in this array. 0 will be the markers that indicate tiles directly
	// around the unit/tile of attention, and then next group would be at 2 radius out, etc. 
	public GameObject[] markerNodes;

	#endregion
	// ====================================================================================================================
	#region pub

	void Start()
	{
		HideAll();
	}

	/// <summary>Hide all nodes of marker</summary>
	public void HideAll()
	{
		foreach (GameObject go in markerNodes) go.SetActiveRecursively(false);
	}

	/// <summary>Show all nodes of the marker</summary>
	public void ShowAll()
	{
		foreach (GameObject go in markerNodes) go.SetActiveRecursively(true);
	}

	/// <summary>Show up to specified radius of nodes</summary>
	public void Show(Vector3 pos, int radius)
	{
		HideAll();

		// 0 or less means to show nothing, so return now
		if (radius <= 0) return;

		// check if within limit
		if (radius > markerNodes.Length) radius = markerNodes.Length;

		// now show nodes
		for (int i = 0; i < radius; i++)
		{
			markerNodes[i].SetActiveRecursively(true);
		}

		// move it to given position
		transform.position = pos;
	}

	/// <summary>This shows only the nodes that lay at the sepcified radius and nothing else</summary>
	public void ShowOutline(Vector3 pos, int radius)
	{
		HideAll();

		// 0 or less means to show nothing, so return now
		if (radius <= 0) return;
		radius--; // cause index into array starts at 0

		// check if within limit
		if (radius > markerNodes.Length) radius = markerNodes.Length;

		// now show nodes
		markerNodes[radius].SetActiveRecursively(true);

		// move it to given position
		transform.position = pos;
	}

	#endregion
	// ====================================================================================================================
	#region create tools

	public static void UpdateMarker(GameObject markerFab, RadiusMarker marker, MapNav.TilesLayout markerLayout, float markerSpacing, float markerSize, int markerRadius)
	{
		// delete old marker nodes
		List<GameObject> remove = new List<GameObject>();
		foreach (Transform t in marker.transform) remove.Add(t.gameObject);
		remove.ForEach(go => DestroyImmediate(go));

		if (markerFab == null) return;
		if (markerRadius < 1) markerRadius = 1;
		marker.prefab = markerFab;
		marker.markerLayout = markerLayout;
		marker.markerSpacing = markerSpacing;
		marker.markerSize = markerSize;
		marker.markerRadius = markerRadius;

		// and create new ones
		CreateMarkerNodes(markerFab, marker, markerLayout, markerSpacing, markerSize, markerRadius);
	}

	public static void CreateMarker(GameObject markerFab, MapNav.TilesLayout markerLayout, float markerSpacing, float markerSize, int markerRadius)
	{
		if (markerFab == null) return;
		if (markerRadius < 1) markerRadius = 1;
		GameObject go = new GameObject();
		go.name = "Marker";
		RadiusMarker marker = go.AddComponent<RadiusMarker>();
		marker.prefab = markerFab;
		marker.markerLayout = markerLayout;
		marker.markerSpacing = markerSpacing;
		marker.markerSize = markerSize;
		marker.markerRadius = markerRadius;
		CreateMarkerNodes(markerFab, marker, markerLayout, markerSpacing, markerSize, markerRadius);
	}

	private static void CreateMarkerNodes(GameObject markerFab, RadiusMarker marker, MapNav.TilesLayout markerLayout, float markerSpacing, float markerSize, int markerRadius)
	{
		if (markerLayout == MapNav.TilesLayout.Hex) CreateHexNodes(markerFab, marker, markerLayout, markerSpacing, markerSize, markerRadius);
		else if (markerLayout == MapNav.TilesLayout.Square_4) CreateSquar4Nodes(markerFab, marker, markerLayout, markerSpacing, markerSize, markerRadius);
		else if (markerLayout == MapNav.TilesLayout.Square_8) CreateSquar8Nodes(markerFab, marker, markerLayout, markerSpacing, markerSize, markerRadius);
	}

	private static void CreateHexNodes(GameObject markerFab, RadiusMarker marker, MapNav.TilesLayout markerLayout, float markerSpacing, float markerSize, int markerRadius)
	{
		GameObject go;
		marker.markerNodes = new GameObject[markerRadius];

		float xOffs = markerSpacing * 0.50f * Mathf.Sqrt(3f);
		float yOffs = markerSpacing * 0.75f;
		float offs = xOffs * 0.5f;

		for (int i = 0; i < markerRadius; i++)
		{
			GameObject parent = new GameObject();
			parent.name = (i < 10 ? "0" : "") + i.ToString();
			parent.transform.position = marker.transform.position + new Vector3(0f, 0.1f, 0f);
			parent.transform.parent = marker.transform;
			marker.markerNodes[i] = parent;

			// right
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(xOffs * (i + 1), 0f, 0f);
			for (int j = 0; j < i + 1; j++)
			{
				// up
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3((xOffs * (i + 1)) - (offs * (j + 1)), 0f, yOffs * (j + 1));
				// down
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3((xOffs * (i + 1)) - (offs * (j + 1)), 0f, -yOffs * (j + 1));
			}

			// left
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(-xOffs * (i + 1), 0f, 0f);
			for (int j = 0; j < i + 1; j++)
			{
				// up
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3((-xOffs * (i + 1)) + (offs * (j + 1)), 0f, yOffs * (j + 1));
				// down
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3((-xOffs * (i + 1)) + (offs * (j + 1)), 0f, -yOffs * (j + 1));
			}

			// up
			for (int j = 0; j < i; j++)
			{
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3((xOffs * j) - (offs * (i - 1)), 0f, yOffs * (i + 1));
			}

			// down
			for (int j = 0; j < i; j++)
			{
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3((xOffs * j) - (offs * (i - 1)), 0f, -yOffs * (i + 1));
			}
		}
	}

	private static void CreateSquar4Nodes(GameObject markerFab, RadiusMarker marker, MapNav.TilesLayout markerLayout, float markerSpacing, float markerSize, int markerRadius)
	{
		GameObject go;
		marker.markerNodes = new GameObject[markerRadius];

		for (int i = 0; i < markerRadius; i++)
		{
			GameObject parent = new GameObject();
			parent.name = (i < 10 ? "0" : "") + i.ToString();
			parent.transform.position = marker.transform.position + new Vector3(0f, 0.1f, 0f);
			parent.transform.parent = marker.transform;
			marker.markerNodes[i] = parent;

			// up
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(0f, 0f, markerSpacing * (i + 1));
			for (int j = 0; j < i; j++)
			{	// to right
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(markerSpacing * (j + 1), 0f, (markerSpacing * i) - (markerSpacing * j));
				// to left
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(-markerSpacing * (j + 1), 0f, (markerSpacing * i) - (markerSpacing * j));
			}


			// down
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(0f, 0f, -markerSpacing * (i + 1));
			for (int j = 0; j < i; j++)
			{	// to right
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(markerSpacing * (j + 1), 0f, -(markerSpacing * i) + (markerSpacing * j));
				// to left
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(-markerSpacing * (j + 1), 0f, -(markerSpacing * i) + (markerSpacing * j));
			}

			// right
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(+markerSpacing * (i + 1), 0f, 0f);

			// left
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(-markerSpacing * (i + 1), 0f, 0f);
		}
	}

	private static void CreateSquar8Nodes(GameObject markerFab, RadiusMarker marker, MapNav.TilesLayout markerLayout, float markerSpacing, float markerSize, int markerRadius)
	{
		GameObject go;

		marker.markerNodes = new GameObject[markerRadius];

		for (int i = 0; i < markerRadius; i++)
		{
			GameObject parent = new GameObject();
			parent.name = (i < 10 ? "0" : "") + i.ToString();
			parent.transform.position = marker.transform.position + new Vector3(0f, 0.1f, 0f);
			parent.transform.parent = marker.transform;
			marker.markerNodes[i] = parent;

			// up
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(0f, 0f, markerSpacing * (i + 1));
			for (int j = 0; j < i + 1; j++)
			{	// to right
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(markerSpacing * (j + 1), 0f, markerSpacing * (i + 1));
				// to left
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(-markerSpacing * (j + 1), 0f, markerSpacing * (i + 1));
			}

			// down
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(0f, 0f, -markerSpacing * (i + 1));
			for (int j = 0; j < i + 1; j++)
			{	// to right
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(markerSpacing * (j + 1), 0f, -markerSpacing * (i + 1));
				// to left
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(-markerSpacing * (j + 1), 0f, -markerSpacing * (i + 1));
			}

			// right
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(+markerSpacing * (i + 1), 0f, 0f);
			for (int j = 0; j < i; j++)
			{	// up
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(markerSpacing * (i + 1), 0f, markerSpacing * (j + 1));
				// down
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(markerSpacing * (i + 1), 0f, -markerSpacing * (j + 1));
			}

			// left
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(-markerSpacing * (i + 1), 0f, 0f);
			for (int j = 0; j < i; j++)
			{	// up
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(-markerSpacing * (i + 1), 0f, markerSpacing * (j + 1));
				// down
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(-markerSpacing * (i + 1), 0f, -markerSpacing * (j + 1));
			}
		}
	}

	#endregion
	// ====================================================================================================================
}
