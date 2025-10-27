using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
  public float sensitivity = 5f;
  public GameObject ball;
  private Vector3 prevBallPos;

  void Start()
  {
    // Calculate where the camera is in relation to the player (ball)
    transform.LookAt(ball.transform);
    prevBallPos = ball.transform.position;
  }

  void Update()
  {
    //If the left mouse button held down, rotate the camera using mouse position
    if (Input.GetKey("mouse 0"))
    {
        float mouse = Input.GetAxis("Mouse Y");
        transform.Rotate(new Vector3(mouse * sensitivity * -1, 0, 0));
        float look = Input.GetAxis("Mouse X") * sensitivity;
        transform.RotateAround(ball.transform.position, Vector3.up, look);
    }
    // Moves the camera by the same amount the ball has moved
    transform.Translate(ball.transform.position - prevBallPos, Space.World);
    prevBallPos = ball.transform.position;  
  }
}