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
    bool dashExecuting = false;
    TrailRenderer trailRenderer;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.emitting = false;
    }

    private void FixedUpdate()
    {
        if (!movementDisabled)
            Motion();
    }

    private void Motion()
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
        if (!dashExecuting)
            rb.linearVelocity = motionVector.normalized * speed;
    }

    private IEnumerator Dash()
    {
        if (movementDisabled || dashExecuting) yield return null;

        dashExecuting = true;
        trailRenderer.emitting = true;

        // execute dash in movement direction or forward
        Vector3 motionVector = new Vector3(moveInput.x, 0, moveInput.y); 
        Vector3 dashDir = motionVector.sqrMagnitude > 0.01f ? motionVector.normalized : transform.forward;
        rb.linearVelocity = dashDir * speed * dashIncrement;

        yield return new WaitForSeconds(0.2f);
        trailRenderer.emitting = false;
        dashExecuting = false;
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

    void IMovement.ExecuteAbility(MovementAbilityType abilityType)
    {
        switch (abilityType)
        {
            case MovementAbilityType.Dash:
                Debug.Log("dashing");
                StartCoroutine(Dash()); 
                break;
        }
    }

    IEnumerator IMovement.DisableMovement(float duration)
    {
        movementDisabled = true; 
        yield return new WaitForSeconds(duration);
        movementDisabled = false;
    }

    void IMovement.DisableMovement(bool movementDisabled)
    {
        this.movementDisabled = movementDisabled; 
    }

    #endregion
}
