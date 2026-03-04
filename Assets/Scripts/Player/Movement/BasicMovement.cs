using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
    float maxStamina;
    float currentStamina;
    float staminaConsumption;
    float staminaRegenRate;

    bool dashExecuting = false;
    TrailRenderer trailRenderer;

    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.emitting = false;

        dashDuration = GameManager.Instance.GetDashDuration();
        dashIncrement = GameManager.Instance.GetDashSpeedIncrement();
        maxStamina = GameManager.Instance.GetMaxStamina();
        staminaConsumption = GameManager.Instance.GetStaminaConsumption();
        staminaRegenRate = GameManager.Instance.GetStaminaRegenRate();

        currentStamina = maxStamina;
    }

    void FixedUpdate()
    {
        Motion();

        // Stamina regeneration
        currentStamina += staminaRegenRate * Time.fixedDeltaTime;

        if(currentStamina > maxStamina) currentStamina = maxStamina;
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

    void IMovement.Init(float _speed, int _teamIndex)
    {
        speed = _speed;

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

    IEnumerator IMovement.Dash(bool ignoreStamina)
    {
        if (ignoreStamina || (!movementDisabled && !dashExecuting && currentStamina >= staminaConsumption))
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

            if (!ignoreStamina)
            {
                currentStamina -= staminaConsumption;
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
