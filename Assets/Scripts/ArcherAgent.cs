using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ArcherAgent : Agent {

    public Transform target;
    public float arrowVelocity = 70f;
    public float cd = 1.75f;
    float p = 1.225f;
    float a = 1.0e-4f;
    bool isFired = false;
    Rigidbody rb;
    Vector3 initialPos;
    Quaternion initialRot;
    Vector2 wind;

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

        float xOff = Academy.Instance.EnvironmentParameters.GetWithDefault("xpos", 0.0f);
        transform.Translate(Vector3.right * xOff);

        float windMag = Academy.Instance.EnvironmentParameters.GetWithDefault("windMag", 0.0f);
        float windDir = Academy.Instance.EnvironmentParameters.GetWithDefault("windDir", 0.0f);

        wind = new Vector2(windMag * Mathf.Cos(windDir), windMag * Mathf.Sin(windDir));

        RequestDecision();
    }

    public override void CollectObservations(VectorSensor sensor) {
        // Arrow location relative to target (2)
        sensor.AddObservation(transform.localPosition.x / 10f);
        // sensor.AddObservation(transform.localPosition.z - target.localPosition.z);

        // Wind vector (2)
        sensor.AddObservation(wind.x / 6f);
        sensor.AddObservation(wind.y / 6f);
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
        continuousActionsOut[0] = -0.22f;
        continuousActionsOut[1] = 0f;
    }

    // Update is called once per physics step
    void FixedUpdate() {
        // Projectile motion update
        if (isFired)
            transform.rotation = Quaternion.LookRotation(rb.velocity);

        // Stop if under floor
        if (transform.localPosition.y < 0f || transform.localPosition.z < -40f) {
            SetReward(-1f);
            EndEpisode();
        }

        if (transform.localPosition.x < -10f || transform.localPosition.x > 10f) {
            //SetReward(-0.75f);
            SetReward(-1f);
            EndEpisode();
        }

        if (transform.localPosition.z > 40f) {
            //SetReward(-0.5f);
            SetReward(-1f);
            EndEpisode();
        }

        // wind force!
        Vector2 arrowVel = new Vector2(rb.velocity.x, rb.velocity.z);
        Vector2 airRelVel = arrowVel - wind;
        // Debug.Log(airRelVel);
        float v = airRelVel.magnitude;

        float forceAmt = (p * v * v * cd * a) / 2;
        Vector2 airRes = -airRelVel.normalized * forceAmt;
        // Debug.Log(airRes);
        rb.AddForce(new Vector3(airRes.x, 0f, airRes.y));
    }
}
