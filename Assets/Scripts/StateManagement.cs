using UnityEngine;

public class StateManagement : MonoBehaviour
{
    ThirdPersonController controller;

	void Start()
	{
		controller = ThirdPersonController.Instance;
	}

	void Update()
	{
		if (Input.GetKey(KeyCode.LeftShift) && controller.State == State.Grounded)
			controller.State = State.Running;
		else if (!Input.GetKey(KeyCode.LeftShift) && controller.State == State.Running)
			controller.State = State.Grounded;
	}

	void OnCollisionEnter(Collision collision)
	{
		if (controller.State == State.Jumped)
			controller.State = State.Grounded;
	}

	void OnCollisionExit(Collision collision)
	{
		// @Todo @Problem
		// Si le joueur 'tombe' d'une hauteur avec une arme en main
		// il sera toujours considéré comme grounded.
		// Ajouter un état falling ?
		// Mettre un collider sur les pieds pour check le grounded ?
	}

	void OnCollisionStay(Collision collision)
	{
		ContactPoint[] points = collision.contacts;
		for (int i = 0; i < points.Length; i++)
		{
			if (Vector3.Angle(points[i].normal, Vector3.up) < controller.MaxGroundSlope)
			{
				if (controller.State != State.Running)
				{
					controller.State = State.Grounded;
				}
			}
		}
	}
}
