using UnityEngine;
using System.Collections;
using System.IO;
using System;
using Microsoft.Win32;

public class floorcalibrator : MonoBehaviour {
	public float controllerFuzzfactor = 0.05f;
    private Transform camerarig;
	private TextMesh resulttext;

	ArrayList floorvalues = new ArrayList();

	string chaperoneJSON = "";
	string steamPath="";

	// Use this for initialization
	void Start() {
        camerarig = GameObject.Find("[CameraRig]").transform;
		resulttext = GameObject.FindObjectOfType<TextMesh>();

		resulttext.text = "Place your controller on the floor";

		steamPath=Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam", "SteamPath","").ToString();
		if ( steamPath == "") {
			resulttext.text = "Could not find steam path in registry";
			return;
		}
		Debug.Log("Steam found at "+steamPath);

		chaperoneJSON = File.ReadAllText(steamPath+"/config/chaperone_info.vrchap");

		File.Copy(steamPath + "/config/chaperone_info.vrchap", steamPath + "/config/chaperone_info.vrchap_previous_" + DateTime.Now.ToString("yy-MM-dd_HHmm.ss"));


		StartCoroutine(CheckFloor());
	}

	IEnumerator CheckFloor() {
		float lastMedian = 10000000f;
        while (true) {
            yield return new WaitForSeconds(5f);
			if (floorvalues.Count != 0) {
				floorvalues.Sort();
				float min = (float)floorvalues[0];
				float max = (float)floorvalues[floorvalues.Count - 1];
				float median = (float)floorvalues[Mathf.FloorToInt(floorvalues.Count / 2)];
				string resultstring = "Collected " + floorvalues.Count + " floor values\nranging from " + min + " to " + max + "\nwith accepted median " + median + "\n\nPlease restart SteamVR";
				Debug.Log(resultstring);
				resulttext.text = resultstring;

				camerarig.transform.localPosition = Vector3.up * -median;

				// save value
				//File.WriteAllText("floorlevel.ini", "" + median, System.Text.Encoding.UTF8);
				JSONObject chap=JSONObject.Create(chaperoneJSON);
				Debug.Log(chap["universes"]);
				for (int i = 0; i < chap["universes"].Count; i++) {
					//chap["universes"][i]["standing"][1] = chap["universes"][i]["standing"][1].f - median;
					chap["universes"][i]["standing"]["translation"][1] = JSONObject.Create( chap["universes"][i]["standing"]["translation"][1].f - median );
					Debug.Log( chap["universes"][i]["standing"]["translation"][1] );
					//chap["universes"][i]["standing"]["translation"][1] = JSONObject.Create( 0 );
				}
				File.WriteAllText("C:\\Steam\\config\\chaperone_info.vrchap", chap.ToString(true), System.Text.Encoding.ASCII);

				floorvalues = new ArrayList();
				/*
				if (Mathf.Abs(lastMedian - median) < 0.01f) {
					Debug.Log("threshold reached");
					Application.Quit();
				}
				*/

				lastMedian = median;
			}
		}
    }

    // Update is called once per frame
    void Update() {
		float minfloor = 10000; // unrealistic starting position
		SteamVR_TrackedObject[] trackedObjects = GameObject.FindObjectsOfType<SteamVR_TrackedObject>();
		foreach (SteamVR_TrackedObject trackedObject in trackedObjects) {
			if (trackedObject.index != SteamVR_TrackedObject.EIndex.Hmd) {
				Renderer[] trackedRenderers = trackedObject.transform.GetComponentsInChildren<Renderer>();
				foreach (Renderer trackedRenderer in trackedRenderers) {
					minfloor = Mathf.Min(-camerarig.transform.localPosition.y + trackedRenderer.bounds.min.y + controllerFuzzfactor, minfloor);
				}
			}
		}
		if (minfloor < 10000) {
			floorvalues.Add(minfloor);
		}
    }

	void OnUnload() {
	}

}
