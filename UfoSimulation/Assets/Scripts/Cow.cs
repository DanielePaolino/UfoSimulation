using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cow : MonoBehaviour
{
    #region Fields
    private Rigidbody rb;
    private bool isAttracted = false;
    private Transform ray;
    private int defaultMass;
    private int attractionMass = 50;
    [SerializeField] private float maxSpeed;

    #endregion

    #region MonoBehaviour
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        defaultMass = (int)rb.mass;
    }

    private void FixedUpdate()
    {
        Attraction();
    }

    #endregion


    #region Gameplay
    public void TriggerAttraction(Transform rayTransform)
    {
        ray = rayTransform;
        isAttracted = true;
        rb.useGravity = false;
        rb.mass = attractionMass;
    }

    private void Attraction()
    {
        if (!isAttracted)
            return;

        float distance = Vector3.Distance(transform.position, ray.position);
        Vector3 directionToRayPos = (ray.position - transform.position);

        //Attract slower on high distance
        //from physics formula --> GPower = GravityForce * ( (Mass1 * Mass2) / (distance * distance) )
        //float gravitationalPower = 9.8f * ((rb.mass * 10f) / (distance * distance));

        //Attract faster on high distance
        //Try different values or combination to obtain a different attraction
        float gravitationalPower = 9.8f * ((rb.mass * 10f) * (2f * distance));
        rb.AddForce(directionToRayPos * gravitationalPower * Time.fixedDeltaTime);

        // limit cow velocity
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        }
    }

    public void StopAttraction()
    {
        isAttracted = false;
        rb.mass = defaultMass;
        rb.useGravity = true;

    }

    #endregion
}
