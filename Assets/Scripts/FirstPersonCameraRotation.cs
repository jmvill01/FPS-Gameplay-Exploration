using UnityEngine;

/// <summary>
/// A simple FPP (First Person Perspective) camera rotation script.
/// Like those found in most FPS (First Person Shooter) games.
/// </summary>
public class FirstPersonCameraRotation : MonoBehaviour {

	public float Sensitivity {
		get { return sensitivity; }
		set { sensitivity = value; }
	}

	[SerializeField]
	private GameObject Player;

	[Range(0.1f, 9f)][SerializeField] float sensitivity = 2f;
	[Tooltip("Limits vertical camera rotation. Prevents the flipping that happens when rotation goes above 90.")]
	[Range(0f, 90f)][SerializeField] float yRotationLimit = 88f;

	Vector2 rotation = Vector2.zero;
	const string xAxis = "Mouse X"; //Strings in direct code generate garbage, storing and re-using them creates no garbage
	const string yAxis = "Mouse Y";

	void Start()
	{
		Player = GameObject.FindGameObjectWithTag("Player");
	}

	void Update(){

		// Rotate the camera based on input of mouse and given sensitivity
		rotation.x += Input.GetAxis(xAxis) * sensitivity;
		rotation.y += Input.GetAxis(yAxis) * sensitivity;

		// Clamp the rotation to prevent flipping
		rotation.y = Mathf.Clamp(rotation.y, -yRotationLimit, yRotationLimit);

		// var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
		// var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);


		// Perform the rotations (1st Player object, 2nd Camera)
		Player.transform.eulerAngles = new Vector3(0, rotation.x, 0);
		transform.eulerAngles = new Vector3(-rotation.y, rotation.x, 0);
	}
}
