using UnityEngine;
using System.Collections;
using System.IO;

public class floorcalibrator : MonoBehaviour {
	public float controllerFuzzfactor = 0.05f;
    private Transform camerarig;
	private TextMesh resulttext;

	ArrayList floorvalues = new ArrayList();

    // Use this for initialization
    void Start() {
        StartCoroutine(CheckFloor());
        camerarig = GameObject.Find("[CameraRig]").transform;
		resulttext = GameObject.FindObjectOfType<TextMesh>();

		resulttext.text = "Place your controller on the floor";
	}

    IEnumerator CheckFloor() {
        while (true) {
            yield return new WaitForSeconds(5f);
			if (floorvalues.Count != 0) {
				floorvalues.Sort();
				float min = (float)floorvalues[0];
				float max = (float)floorvalues[floorvalues.Count - 1];
				float median = (float)floorvalues[Mathf.FloorToInt(floorvalues.Count / 2)];
				string resultstring = "Collected " + floorvalues.Count + " floor values\nranging from " + min + " to " + max + "\nwith accepted median " + median;
				Debug.Log(resultstring);
				resulttext.text = resultstring;

				camerarig.transform.localPosition = Vector3.up * -median;

				// save value
				File.WriteAllText("floorlevel.ini", "" + median, System.Text.Encoding.UTF8);

				floorvalues = new ArrayList();
			}
		}
    }

    // Update is called once per frame
    void Update() {
		float minfloor = 10000; // unrealistic starting position
		SteamVR_TrackedObject[] trackedObjects = GameObject.FindObjectsOfType<SteamVR_TrackedObject>();
		foreach (SteamVR_TrackedObject trackedObject in trackedObjects) {
			Renderer[] trackedRenderers = trackedObject.transform.GetComponentsInChildren<Renderer>();
			foreach (Renderer trackedRenderer in trackedRenderers) {
				if (trackedRenderer.gameObject.layer != LayerMask.NameToLayer("UI")) {
					minfloor = Mathf.Min(-camerarig.transform.localPosition.y + trackedRenderer.bounds.min.y + controllerFuzzfactor, minfloor);
				}
			}
		}
		if (minfloor < 10000) {
			floorvalues.Add(minfloor);
		}
    }
}
