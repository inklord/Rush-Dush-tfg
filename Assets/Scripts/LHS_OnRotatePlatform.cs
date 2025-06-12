using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LHS_OnRotatePlatform : MonoBehaviour
{
    private Transform platformTransform;
    private Vector3 lastPlatformPosition;
    private Quaternion lastPlatformRotation;
    private Rigidbody playerRigidbody;
    private bool isOnPlatform;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (isOnPlatform && platformTransform != null)
        {
            // Calculate platform movement
            Vector3 platformMovement = platformTransform.position - lastPlatformPosition;
            
            // Calculate platform rotation delta
            Quaternion rotationDelta = platformTransform.rotation * Quaternion.Inverse(lastPlatformRotation);
            Vector3 rotationDeltaEuler = rotationDelta.eulerAngles;
            
            // Apply platform movement to player
            playerRigidbody.MovePosition(playerRigidbody.position + platformMovement);
            
            // Apply rotation around platform center
            Vector3 toPlatform = platformTransform.position - transform.position;
            Vector3 newPosition = platformTransform.position + (rotationDelta * -toPlatform);
            playerRigidbody.MovePosition(newPosition);

            // Update last known platform state
            lastPlatformPosition = platformTransform.position;
            lastPlatformRotation = platformTransform.rotation;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            platformTransform = collision.transform;
            lastPlatformPosition = platformTransform.position;
            lastPlatformRotation = platformTransform.rotation;
            isOnPlatform = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = false;
            platformTransform = null;
        }
    }
}
