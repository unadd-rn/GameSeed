using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickThrowTest : MonoBehaviour
{
    [SerializeField]
    Transform Target;
    
    [SerializeField]
    float initialAngle;

    [SerializeField] float velocityScale = 1f;
    [SerializeField] float spinScale = 15f;
    [SerializeField][Range(-0.5f, 0.5f)] float hitPoint = 0f;
    [SerializeField] float stickLength = 1f;

    IEnumerator Start()
    {
        Debug.Log("Game started. Waiting for 3 seconds...");

        // This line pauses the function for 3 seconds
        yield return new WaitForSeconds(5f);

        var rigid = GetComponent<Rigidbody>();

        Vector3 p = Target.position;

        float gravity = Physics.gravity.magnitude;

        float angle = initialAngle * Mathf.Deg2Rad;
        float height = transform.position.y - Target.position.y;

        Vector3 planarTarget = new Vector3(p.x, 0, p.z);
        Vector3 planarPosition = new Vector3(transform.position.x, 0, transform.position.z);

        float distance = Vector3.Distance(planarTarget, planarPosition);
        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * distance * distance) / (distance * Mathf.Tan(angle) - height));

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        float angleBetweenObjects = Vector3.SignedAngle(Vector3.forward, planarTarget - planarPosition, Vector3.up);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        Vector3 lateralForce = -transform.right * (hitPoint * velocityScale * initialVelocity);
        rigid.AddForce(lateralForce, ForceMode.VelocityChange);

        Vector3 localHitOffset = transform.right * (hitPoint * stickLength);
        Vector3 worldHitPoint = transform.position + localHitOffset;

        // rigid.velocity = finalVelocity;
        rigid.AddForceAtPosition(finalVelocity * velocityScale, worldHitPoint, ForceMode.VelocityChange);
        rigid.AddForce(-transform.right * (hitPoint * velocityScale * initialVelocity), ForceMode.VelocityChange);
        rigid.angularVelocity = -transform.up * (hitPoint * spinScale);

        Debug.Log("3 seconds have passed! Executing action.");
    }

}
