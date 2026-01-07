using System.Collections.Generic;
using UnityEngine;

// Script to handle the behaviour of the UFO's tractor beam.
public class UFOBeam : MonoBehaviour
{
    private GameObject ufoParent;
    private readonly List<Rigidbody> objectsInBeam = new();

    void Start() { ufoParent = transform.parent.gameObject; }

    void FixedUpdate() {
        // "pull up" every object currently in the UFO's beam.
        foreach (Rigidbody rb in objectsInBeam) {
            // Only pull object up if the object is below the UFO.
            if (rb != null && rb.position.y < ufoParent.transform.position.y - 10) {
                rb.AddForce(Vector3.up * 30, ForceMode.Acceleration);  }
        }
    }

    // When Objects enter the UFO's beam.
    private void OnTriggerEnter(Collider other) {
        // Check if the object has a Rigidbody to move or isn't already in the beam.
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && !objectsInBeam.Contains(rb)) {
            objectsInBeam.Add(rb); // Add to list of objects in the beam.
            rb.useGravity = false; // disable gravity for smooth movement.
        }
    }

    // When Objects exit the UFO's beam.
    private void OnTriggerExit(Collider other) {
        // Remove the exiting object from the UFO's beam list.
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && objectsInBeam.Contains(rb)) {
            objectsInBeam.Remove(rb); // Remove from list of objects in the beam.
            rb.useGravity = true; // re-enable gravity.
        }
    }
}
