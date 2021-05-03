using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour {
    public ArcherAgent arrow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnCollisionEnter(Collision collision) {
        Vector3 contactPt = collision.GetContact(0).point;
        Vector3 localContact = transform.InverseTransformPoint(contactPt);
        float centerDist = Mathf.Sqrt(localContact.x * localContact.x + localContact.y * localContact.y);

        arrow.SetReward(1f / (1f + (centerDist / 5f)));
        arrow.EndEpisode();

        Debug.Log("hit at distance!");
        Debug.Log(centerDist);
        Debug.DrawRay(collision.GetContact(0).point, -collision.GetContact(0).normal, Color.white, 30);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
