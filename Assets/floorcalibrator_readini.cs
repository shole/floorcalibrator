using UnityEngine;
using System.Collections;
using System.IO;

public class floorcalibrator_readini : MonoBehaviour {

	void Start () {
		GameObject playercamerarig=GameObject.FindObjectOfType<SteamVR_PlayArea>().gameObject;
		float floorlevel = 0;
		if (!File.Exists("floorlevel.ini")) {
			Debug.LogError("could not find floorlevel.ini!");
		} else {
			string floorlevel_s = File.ReadAllText("floorlevel.ini", System.Text.Encoding.UTF8);
			if (!float.TryParse(floorlevel_s, out floorlevel)) {
				Debug.LogError("could not read floor calibration!");
			} else {
				playercamerarig.transform.localPosition = Vector3.up * -floorlevel;
			}
		}
	}
}
