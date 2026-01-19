using System.Collections.Generic;
using UnityEngine;

// Script to handle the behaviour of the UFO's tractor beam.
public class MagnetPull : MonoBehaviour
{
    private GameObject magnetParent;
    private readonly List<Rigidbody> objectsInBeam = new();

    void Start() { magnetParent = transform.parent.gameObject; }

    void FixedUpdate() {
        // "pull" every object currently in the Magnet's beam.
        foreach (Rigidbody rb in objectsInBeam) {
            // Only pull if the object is in line with the magnet the Magnet.
            if (rb != null) {
                //float distanceToMagnet = Vector3.Distance(rb.position, magnetParent.transform.position);
                Vector3 directionToMagnet = (magnetParent.transform.position - rb.position).normalized;
                rb.AddForce(directionToMagnet * 20f, ForceMode.Acceleration);
            }
        }
    }

    // When Objects enter the UFO's beam.
    private void OnTriggerEnter(Collider other) {
        // Check if the object has a Rigidbody to move or isn't already in the beam.
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && !objectsInBeam.Contains(rb)) {
            objectsInBeam.Add(rb); // Add to list of objects in the beam.
        }
    }

    // When Objects exit the Magnets's beam.
    private void OnTriggerExit(Collider other) {
        // Remove the exiting object from the Magnets's beam list.
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && objectsInBeam.Contains(rb)) {
            removeObj(rb);
        }
    }

    public void removeObj(Rigidbody rb) {
        objectsInBeam.Remove(rb);
    }
}