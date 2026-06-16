using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StickThrowTest : MonoBehaviour
{
    [SerializeField]
    Transform Target;

    [SerializeField] private Slider hitPointSlider;
    
    [SerializeField]
    float initialAngle;

    public float velocityScale = 1f;
    [SerializeField] float spinScale = 15f;
    private float hitPoint = 0f;
    [SerializeField] float stickLength = 1f;
    private Rigidbody rigid;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();

        if (hitPointSlider == null)
        {
            hitPointSlider = FindObjectOfType<Slider>();
        }
    }

    public void Throw()
    {
        if (hitPointSlider != null)
        {
            hitPoint = hitPointSlider.value;
        }

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

        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        // rigid.velocity = finalVelocity;
        rigid.AddForceAtPosition(finalVelocity * velocityScale, worldHitPoint, ForceMode.VelocityChange);
        // rigid.AddForce(-transform.right * (hitPoint * velocityScale * initialVelocity), ForceMode.VelocityChange);
        rigid.angularVelocity = transform.right * (hitPoint * spinScale);
    }

}
