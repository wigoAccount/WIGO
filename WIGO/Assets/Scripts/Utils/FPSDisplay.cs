using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
	float deltaTime = 0.0f;

	void Update()
	{
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
	}

	void OnGUI()
	{
		int w = Screen.width, h = Screen.height;
		int padding = w / 8;
		var labelHeight = h * 2 / 100;

		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(padding, h - labelHeight - 10, w, labelHeight);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = labelHeight;
		style.normal.textColor = new Color(1.0f, 0.8f, 0.15f, 1.0f);
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);
	}
}
