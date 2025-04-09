using System;
using UnityEngine;


public class sliding : MonoBehaviour
{
     public Transform orientation;
     public Transform playerObj;
     private Rigidbody rb;
     private PlayerController pm;

     public float maxSlideTime;
     public float slideForce;
     private float slideTimer;

     public float slideYScale;
     private float startYScale;
     
     public KeyCode slideKey = KeyCode.LeftControl;
     private float horizontalInput;
     private float verticalInput;

     private bool isSliding;

     private void Start()
     {
          rb = GetComponent<Rigidbody>();
          pm = GetComponent<PlayerController>();
          
          startYScale = playerObj.localScale.y;
     }

     private void Update()
     {
          horizontalInput = Input.GetAxis("Horizontal");
          verticalInput = Input.GetAxis("Vertical");

          if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
          {
               StartSlide();
          }
          if (Input.GetKeyDown(slideKey) && isSliding)
          {
               StopSlide();
          }
     }

     private void FixedUpdate()
     {
          if (isSliding)
          {
               SlidingMovement();
          }
     }

     private void StartSlide()
     {
          isSliding = true;
          
          playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
          
          rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
          
          slideTimer = maxSlideTime;
     }

     private void SlidingMovement()
     {
          Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
          
          rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
          
          slideTimer -= Time.deltaTime;

          if (slideTimer <= 0)
          {
               StopSlide();
          }
     }
     
     private void StopSlide()
     {
          isSliding = false;
          
          playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
     }
}
