using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickThrowTest : MonoBehaviour
{
    [SerializeField]
    Transform Target;
    
    [SerializeField]
    float initialAngle;

    void Start()
    {
        var rigid = GetComponent<Rigidbody>();

        Vector3 p = Target.position;

        float gravity = Physics.gravity.magnitude;

        float angle = initialAngle * Mathf.Deg2Rad;

        Vector3 planarTarget = new Vector3(p.x, 0, p.z);
        Vector3 planarPosition = new Vector3(transform.position.x, 0, transform.position.y);

        float distance = Vector3.Distance(planarTarget, planarPosition);
        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt(0.5f * gravity * Mathf.Pow(distance, 2));

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPosition);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        rigid.velocity = finalVelocity;

    }

}
