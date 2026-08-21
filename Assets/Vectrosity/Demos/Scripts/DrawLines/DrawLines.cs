using UnityEngine;
using Vectrosity;
using System.Collections.Generic;
using SpringGUI;
public class DrawLines : MonoBehaviour {
	
	public float rotateSpeed = 90.0f;
	public float maxPoints = 500;
	public VectorLine line;
	private bool endReached;
	private bool continuous = true;
	private bool oldContinuous = true;
	private bool fillJoins = false;
	private bool oldFillJoins = false;
	private bool weldJoins = false;
	private bool oldWeldJoins = false;
	private bool thickLine = false;
	private bool canClick = true;
	public HotspotManger hotspotManger;

	public Texture texture;
	void Start () {
		SetLine();

	}
	
	public void SetLine () {
		VectorLine.Destroy (ref line);
		
		if (!continuous) {
			fillJoins = false;
		}
		var lineType = (continuous? LineType.Continuous : LineType.Discrete);
		var joins = (fillJoins? Joins.Fill : Joins.None);
		var lineWidth = (thickLine? 24 : 2);
		
		line = new VectorLine("Line", new List<Vector2>(), lineWidth, lineType, joins);
		line.drawTransform = transform;
		line.color = Color.red;
		line.texture = texture;
		//line.layer = 9;//²ã

		endReached = false;
	}
	
	void Update () {
		
		if (hotspotManger.lineShow)
		{
			if (canClick && !endReached)
			{
				
				line.lineWidth = 0.3f;//LineWidth
				line.points2.Add(hotspotManger.linePosition);
				// Log.Debug(line.lineWidth);
				// Start off with 2 points
				if (line.points2.Count == 1)
				{
					line.points2.Add(Vector2.zero);
				}

				if (line.points2.Count == maxPoints)
				{
					endReached = true;
				}
				hotspotManger.lineShow = false;
			}

			// The last line point should always be where the mouse is; only draw when there are enough points
			if (line.points2.Count >= 2)
			{
				line.points2[line.points2.Count - 1] = hotspotManger.linePosition;
				line.Draw();
			}

			// Rotate around midpoint of screen
			transform.RotateAround(new Vector2(Screen.width / 2, Screen.height / 2), Vector3.forward, Time.deltaTime * rotateSpeed * Input.GetAxis("Horizontal"));

		}

	}
}