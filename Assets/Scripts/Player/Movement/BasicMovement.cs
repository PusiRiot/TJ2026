using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class BasicMovement : MonoBehaviour, IMovement
{
    #region Variables
    const float ROTATION_SPEED = 7f;
    float speed;
    Vector2 moveInput;
    bool movementDisabled = false;
    Rigidbody rb;

    // Dash
    float dashIncrement;
    float dashDuration;
    float dashCooldownDuration;

    bool dashExecuting = false;
    bool dashCoolingDown = false;
    TrailRenderer trailRenderer;

    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.emitting = false;

        dashDuration = GameManager.Instance.GetDashDuration();
        dashIncrement = GameManager.Instance.GetDashSpeedIncrement();
        dashCooldownDuration = GameManager.Instance.GetDashCooldownDuration();
    }

    void FixedUpdate()
    {
        Motion();
    }

    void Motion()
    {
        // Movement
        Vector3 motionVector = new Vector3(moveInput.x, 0, moveInput.y);

        // rotate to face the movement direction
        if (motionVector.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(motionVector);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * ROTATION_SPEED));
        }

        // move in the direction of the input
        if (!movementDisabled && !dashExecuting)
            rb.linearVelocity = motionVector.normalized * speed;
    }


    #region IMovement implementation

    void IMovement.Init(float _speed, float _dashIncrement, int _teamIndex)
    {
        speed = _speed;
        dashIncrement = _dashIncrement;

        // Add trailRenderer material, it is linked to the team index to change color accordingly
        List<Material> trailRendererMats = new();
        trailRendererMats.Add(GameManager.Instance.GetTeamEmissiveMaterial(_teamIndex));
        trailRenderer.SetMaterials(trailRendererMats);
    }

    void IMovement.Move(float horizontal, float vertical)
    {
        moveInput = new Vector2(horizontal, vertical);
    }

    /// <summary>
    /// Disable movement for a specific amount of time. Doesn't disable rotation
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    IEnumerator IMovement.DisableMovement(float duration)
    {
        movementDisabled = true; 
        yield return new WaitForSeconds(duration);
        movementDisabled = false;
    }

    IEnumerator IMovement.Dash(bool ignoreCooldown)
    {
        if (ignoreCooldown || (!movementDisabled && !dashExecuting && !dashCoolingDown))
        {
            dashExecuting = true;
            trailRenderer.emitting = true;

            // execute dash in movement direction or forward
            Vector3 motionVector = new Vector3(moveInput.x, 0, moveInput.y);
            Vector3 dashDir = motionVector.sqrMagnitude > 0.01f ? motionVector.normalized : transform.forward;
            rb.linearVelocity = dashDir * speed * dashIncrement;

            yield return new WaitForSeconds(dashDuration);
            trailRenderer.emitting = false;
            dashExecuting = false;

            if (!ignoreCooldown)
            {
                dashCoolingDown = true;

                yield return new WaitForSeconds(dashCooldownDuration); // NOTE: if stamina system were to be done, this implementation should change
                dashCoolingDown = false;
            }
        }
    }

    /// <summary>
    /// Disable or enable movement. Doesn't disable rotation
    /// </summary>
    /// <param name="movementDisabled"></param>
    void IMovement.DisableMovement(bool movementDisabled)
    {
        this.movementDisabled = movementDisabled; 
    }

    #endregion
}
