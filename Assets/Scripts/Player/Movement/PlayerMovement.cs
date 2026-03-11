using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : Subject<PlayerMovementEvent>
{
    #region Variables
    const float ROTATION_SPEED = 7f;
    float speed;
    Vector2 moveInput;
    bool movementDisabled = false;
    Rigidbody rb;
    int teamIndex = -1;

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

        AddObserversOnScene();
    }

    void FixedUpdate()
    {
        Motion();

        // Stamina regeneration
        if (currentStamina < maxStamina)
            RegenerateStamina();
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

    void RegenerateStamina()
    {
        currentStamina += staminaRegenRate * Time.fixedDeltaTime;

        if (currentStamina >= maxStamina)
        {
            currentStamina = maxStamina;
            Notify(PlayerMovementEvent.DashEnabled, teamIndex);
        }
    }


    #region Public methods

    public void Init(float _speed, int _teamIndex)
    {
        speed = _speed;

        // Add trailRenderer material, it is linked to the team index to change color accordingly
        List<Material> trailRendererMats = new();
        teamIndex = _teamIndex;
        trailRendererMats.Add(GameManager.Instance.GetTeamEmissiveMaterial(_teamIndex));
        trailRenderer.SetMaterials(trailRendererMats);
    }

    public void Move(float horizontal, float vertical)
    {
        moveInput = new Vector2(horizontal, vertical);
    }

    /// <summary>
    /// Disable movement for a specific amount of time. Doesn't disable rotation
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    public IEnumerator DisableMovement(float duration)
    {
        movementDisabled = true; 
        yield return new WaitForSeconds(duration);
        movementDisabled = false;
    }

    public IEnumerator Dash(bool ignoreStamina)
    {
        if (ignoreStamina || (!movementDisabled && !dashExecuting && currentStamina >= staminaConsumption))
        {
            Notify(PlayerMovementEvent.DashConsumed, teamIndex);
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
    public void DisableMovement(bool movementDisabled)
    {
        this.movementDisabled = movementDisabled;
    }

    #endregion
}
