using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ArcherAgent : Agent {

    public Transform target;
    public float arrowVelocity = 70f;
    bool isFired = false;
    Rigidbody rb;
    Vector3 initialPos;
    Quaternion initialRot;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialPos = transform.position;
        initialRot = transform.rotation;
    }

    // Reset arrow to random position
    public override void OnEpisodeBegin() {
        rb.isKinematic = true;
        rb.useGravity = false;

        isFired = false;
        rb.velocity = Vector3.zero;
        transform.SetPositionAndRotation(initialPos, initialRot);

        // TODO: randomize location and wind!

        RequestDecision();
    }

    public override void CollectObservations(VectorSensor sensor) {
        // Arrow location relative to target (2)
        sensor.AddObservation(transform.localPosition.x - target.localPosition.x);
        sensor.AddObservation(transform.localPosition.z - target.localPosition.z);

        // Wind vector (2)
        // TODO: add wind!
        sensor.AddObservation(0f);
        sensor.AddObservation(0f);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        if (isFired) {
            Debug.Log("oops, action when fired");
            return;
        }

        var contActions = actions.ContinuousActions;

        // Set rotation
        transform.rotation = Quaternion.Euler(
            contActions[0] * 20,
            contActions[1] * 30,
            0
        );

        rb.velocity = transform.forward * arrowVelocity;

        rb.useGravity = true;
        rb.isKinematic = false;
        isFired = true;
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = -0.195f;
        continuousActionsOut[1] = 0f;
    }

    // Update is called once per physics step
    void FixedUpdate() {
        // Projectile motion update
        if (isFired)
            transform.rotation = Quaternion.LookRotation(rb.velocity);

        // Stop if under floor
        if (transform.localPosition.y < 0f) {
            SetReward(-1f);
            EndEpisode();
        }

        // TODO: wind force!
    }
}
