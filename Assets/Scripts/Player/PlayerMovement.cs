using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : Subject<PlayerMovementEvent>
{
    #region Variables
    float _rotationSpeed;
    float speed;
    Vector2 moveInput;
    bool movementDisabled = false;
    Rigidbody rb;
    int _teamIndex = -1;

    // Dash
    float _dashSpeedIncrement;
    float _dashDuration;
    float _maxStamina;
    float currentStamina;
    float _staminaConsumption;
    float _staminaRegenRate;

    bool dashExecuting = false;
    TrailRenderer trailRenderer;

    #endregion

    #region MonoBehaviour

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.emitting = false;

        _rotationSpeed = GameManager.Instance.GetPlayerRotationSpeed();
        _dashDuration = GameManager.Instance.GetDashDuration();
        _dashSpeedIncrement = GameManager.Instance.GetDashSpeedIncrement();
        _maxStamina = GameManager.Instance.GetMaxStamina();
        _staminaConsumption = GameManager.Instance.GetStaminaConsumption();
        _staminaRegenRate = GameManager.Instance.GetStaminaRegenRate();

        currentStamina = _maxStamina;

        AddObserversOnScene();
    }

    void FixedUpdate()
    {
        Motion();

        // Stamina regeneration
        if (currentStamina < _maxStamina)
            RegenerateStamina();
    }

    #endregion

    void Motion()
    {
        // Movement
        Vector3 motionVector = new Vector3(moveInput.x, 0, moveInput.y);

        // Stop them from pushing each other when they are not moving, but allow them to push each other when they are moving
        if (motionVector.magnitude == 0.0f)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY
                | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }

        else
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }

        // rotate to face the movement direction
        if (motionVector.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(motionVector);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * _rotationSpeed));
        }

        // move in the direction of the input
        if (!movementDisabled && !dashExecuting)
            rb.linearVelocity = motionVector.normalized * speed;
    }

    void RegenerateStamina()
    {
        currentStamina += _staminaRegenRate * Time.fixedDeltaTime;

        if (currentStamina >= _maxStamina)
        {
            currentStamina = _maxStamina;
            Notify(PlayerMovementEvent.DashEnabled, _teamIndex);
        }
    }


    #region Public methods

    public void Initialize(float speed, int teamIndex)
    {
        this.speed = speed;

        // Add trailRenderer material, it is linked to the team index to change color accordingly
        List<Material> trailRendererMats = new();
        _teamIndex = teamIndex;
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

    // To be called by Dash Action
    public IEnumerator Dash()
    {
        if (!movementDisabled && !dashExecuting && currentStamina >= _staminaConsumption)
        {
            dashExecuting = true;
            trailRenderer.emitting = true;

            // execute dash in movement direction or forward
            Vector3 motionVector = new Vector3(moveInput.x, 0, moveInput.y);
            Vector3 dashDir = motionVector.sqrMagnitude > 0.01f ? motionVector.normalized : transform.forward;
            rb.linearVelocity = dashDir * speed * _dashSpeedIncrement;

            yield return new WaitForSeconds(_dashDuration);
            trailRenderer.emitting = false;
            dashExecuting = false;

            currentStamina -= _staminaConsumption;
            Notify(PlayerMovementEvent.DashConsumed, _teamIndex);
        }
    }

    // To be called by Heavy Melee
    public IEnumerator Dash(float dashDuration, float dashSpeedIncrement)
    {
        dashExecuting = true;
        trailRenderer.emitting = true;

        // execute dash in movement direction or forward
        Vector3 motionVector = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 dashDir = motionVector.sqrMagnitude > 0.01f ? motionVector.normalized : transform.forward;
        rb.linearVelocity = dashDir * speed * dashSpeedIncrement;

        yield return new WaitForSeconds(dashDuration);
        trailRenderer.emitting = false;
        dashExecuting = false;
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
