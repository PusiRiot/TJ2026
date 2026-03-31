using System;
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

    // Animator
    PlayerAnimator playerAnimator;

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
        playerAnimator = GetComponent<PlayerAnimator>();

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

        // rotate to face the movement direction
        if (motionVector.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(motionVector);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * _rotationSpeed));
        }

        // move in the direction of the input
        if (!movementDisabled && !dashExecuting)
        {
            rb.linearVelocity = motionVector.normalized * speed;

            // Animation walk forward
            playerAnimator.Motion = motionVector.magnitude;
        }
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

    IEnumerator DashMovement(float dashDuration, float dashSpeedIncrement)
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
    /// To stop players pushing each other on collision
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            rb.linearVelocity = Vector3.zero;
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

    /// <summary>
    /// To be called by Player input, it executes a dash with dash parameters if the player has enough stamina, has movement enabled and the dash is not on cooldown
    /// </summary>
    public void Dash()
    {
        if (!movementDisabled && !dashExecuting && currentStamina >= _staminaConsumption)
        {
            StartCoroutine(DashMovement(_dashDuration, _dashSpeedIncrement));
        }
    }

    /// <summary>
    /// To be called by Heavy Melee, it executes a dash with the given parameters and it always executes
    /// </summary>
    /// <param name="dashDuration"></param>
    /// <param name="dashSpeedIncrement"></param>
    public void Dash(float dashDuration, float dashSpeedIncrement)
    {
        StartCoroutine(DashMovement(dashDuration, dashSpeedIncrement));
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
