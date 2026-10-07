using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GravityShiftController : MonoBehaviour
{
    public Transform worldRoot;              // drag the empty "World" GameObject here
    public float wallCheckDistance = 0.9f;   // keep this small so only close walls count
    public float rotationDuration = 0.5f;    // how long the smooth rotation takes
    public bool invertRotationDirection = false; // flip this in Inspector if it spins the wrong way

    private TankStyleController movement;
    private CharacterController controller;
    private bool isRotating = false;

    void Start()
    {
        movement = GetComponent<TankStyleController>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (isRotating)
        {
            return; // ignore input while a rotation is already happening
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && movement.IsGrounded)
        {
            RaycastHit hit;
            bool foundWall = FindWallInFront(out hit);

            if (foundWall && worldRoot != null)
            {
                StartCoroutine(RotateWorldSmoothly(hit));
            }
            else
            {
                movement.Jump();
            }
        }
    }

    bool FindWallInFront(out RaycastHit hit)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f; // raise it a bit so it doesn't hit the floor
        float sphereRadius = 0.4f; // gives some tolerance if Robert isn't perfectly lined up

        if (Physics.SphereCast(origin, sphereRadius, transform.forward, out hit, wallCheckDistance))
        {
            // ignore anything that isn't roughly vertical (skip floors/ceilings)
            if (Mathf.Abs(hit.normal.y) < 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    IEnumerator RotateWorldSmoothly(RaycastHit wallHit)
    {
        isRotating = true;

        // use the wall's own flat surface direction, not which way Robert happens to face
        Vector3 flatNormal = new Vector3(wallHit.normal.x, 0f, wallHit.normal.z);
        flatNormal.Normalize();

        // the tilt axis runs along the wall, perpendicular to its normal
        Vector3 axis = Vector3.Cross(Vector3.up, flatNormal).normalized;
        float angle = 90f;

        if (invertRotationDirection)
        {
            angle = -angle;
        }

        Quaternion startRotation = worldRoot.rotation;
        Quaternion endRotation = Quaternion.AngleAxis(angle, axis) * startRotation;
        Vector3 pivot = transform.position;

        // remember the starting position and offset ONCE, before the loop begins
        Vector3 startPosition = worldRoot.position;
        Vector3 startOffset = startPosition - pivot;

        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationDuration;

            // rotate the world smoothly from start to end, around the pivot point
            Quaternion currentStep = Quaternion.Slerp(startRotation, endRotation, t);
            Quaternion deltaSoFar = currentStep * Quaternion.Inverse(startRotation);

            worldRoot.rotation = currentStep;
            worldRoot.position = pivot + deltaSoFar * startOffset;

            yield return null;
        }

        worldRoot.rotation = endRotation;
        worldRoot.position = pivot + (endRotation * Quaternion.Inverse(startRotation)) * startOffset;

        // force the CharacterController to re-check the ground after the room settles
        controller.enabled = false;
        controller.enabled = true;

        isRotating = false;
    }
}