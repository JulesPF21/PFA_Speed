using System;
using ECM2;
using ECM2.Examples.FirstPerson;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : FirstPersonCharacter
{
    public float Health = 100f;

    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(.15f, .05f, 0f);

    public Transform cameraTransform;
    private Vector3 originalCameraPos;

    private float originalHeight;
    private Vector3 originalCenter;
    private Vector3 originalCameraLocalPos;

    [SerializeField] private CapsuleCollider capsule;

    public float slideCameraHeight = -0.5f;
    public float slideColliderHeight = 1.0f;
    public Vector3 slideColliderCenter = new Vector3(0, 0.5f, 0);

    [Header("Sliding Parameters")] public KeyCode slideKey = KeyCode.LeftShift;

    [Header("Wall Run Settings")] public float wallRunDuration = 1.5f;
    public float wallRunGravity = 1f;
    public float wallRunSpeed = 10f;
    public float wallCheckDistance = 1f;
    public float jumpImpulseStrength = 12f;
    public float wallRaycastHeight = 2f;

    private float wallRunTimer;
    private bool isWallRunning;
    private Vector3 wallNormal;

    private Vector3 jointOriginalPos;
    private float timer = 0;

    [Header("Wall Run Camera Effects")] public float wallRunTilt = 15f;
    public float tiltSmoothSpeed = 5f;
    private float targetTilt = 0f;
    private float currentTilt = 0f;

    [Space(15.0f)] public float slideImpulse = 20.0f;
    public float slideDownAcceleration = 20.0f;

    enum ECustomMovementMode
    {
        Sliding = 1
    }

    public override float GetMaxSpeed()
    {
        return IsSliding() ? maxWalkSpeed : base.GetMaxSpeed();
    }

    public override float GetMaxAcceleration()
    {
        return IsSliding() ? maxAcceleration * 0.1f : base.GetMaxAcceleration();
    }

    public override bool IsWalking()
    {
        return IsSliding() || base.IsWalking();
    }

    public bool IsSliding()
    {
        return movementMode == MovementMode.Custom && customMovementMode == (int)ECustomMovementMode.Sliding;
    }

    protected virtual bool CanSlide()
    {
        if (!IsGrounded())
            return false;

        float sqrSpeed = velocity.sqrMagnitude;
        float slideSpeedThreshold = maxWalkSpeedCrouched * maxWalkSpeedCrouched;

        return sqrSpeed >= slideSpeedThreshold * 1.02f;
    }

    protected virtual Vector3 CalcSlideDirection()
    {
        Vector3 slideDirection = GetMovementDirection();
        if (slideDirection.isZero())
            slideDirection = GetVelocity();
        else if (slideDirection.isZero())
            slideDirection = GetForwardVector();

        slideDirection = ConstrainInputVector(slideDirection);

        return slideDirection.normalized;
    }

    protected virtual void CheckSlideInput()
    {
        bool isSliding = IsSliding();
        bool wantsToSlide = Input.GetKey(slideKey);

        if (!isSliding && wantsToSlide && CanSlide())
        {
            SetMovementMode(MovementMode.Custom, (int)ECustomMovementMode.Sliding);
        }
        else if (isSliding && (!wantsToSlide || !CanSlide()))
        {
            SetMovementMode(MovementMode.Walking);
        }
    }

    protected override void OnMovementModeChanged(MovementMode prevMovementMode, int prevCustomMode)
    {
        base.OnMovementModeChanged(prevMovementMode, prevCustomMode);

        if (IsSliding())
        {
            Vector3 slideDirection = CalcSlideDirection();
            characterMovement.velocity += slideDirection * slideImpulse;

            SetRotationMode(RotationMode.None);

            capsule.height = slideColliderHeight;
            capsule.center = slideColliderCenter;

            cameraTransform.localPosition = originalCameraPos + new Vector3(0, slideCameraHeight, 0);
        }

        bool wasSliding = prevMovementMode == MovementMode.Custom &&
                          prevCustomMode == (int)ECustomMovementMode.Sliding;

        if (wasSliding)
        {
            cameraTransform.localPosition = originalCameraPos;
            SetRotationMode(RotationMode.None);
            capsule.height = originalHeight;
            capsule.center = originalCenter;
            joint.localPosition = originalCameraLocalPos;

            if (IsFalling())
            {
                Vector3 worldUp = -GetGravityDirection();
                Vector3 verticalVelocity = Vector3.Project(velocity, worldUp);
                Vector3 lateralVelocity = Vector3.ClampMagnitude(velocity - verticalVelocity, maxWalkSpeed);

                characterMovement.velocity = lateralVelocity + verticalVelocity;
            }
        }
    }

    protected override void OnBeforeSimulationUpdate(float deltaTime)
    {
        base.OnBeforeSimulationUpdate(deltaTime);
        CheckSlideInput();
    }

    protected virtual void SlidingMovementMode(float deltaTime)
    {
        Vector3 desiredVelocity = Vector3.Project(GetDesiredVelocity(), GetRightVector());

        characterMovement.velocity =
            CalcVelocity(characterMovement.velocity, desiredVelocity, groundFriction * 0.2f, false, deltaTime);

        Vector3 slideDownDirection =
            Vector3.ProjectOnPlane(GetGravityDirection(), characterMovement.groundNormal).normalized;

        characterMovement.velocity += slideDownAcceleration * deltaTime * slideDownDirection;

        if (applyStandingDownwardForce)
            ApplyDownwardsForce();
    }

    protected override void CustomMovementMode(float deltaTime)
    {
        base.CustomMovementMode(deltaTime);

        if (customMovementMode == (int)ECustomMovementMode.Sliding)
        {
        }

        SlidingMovementMode(deltaTime);
    }

    private void Start()
    {
        capsule = GetComponent<CapsuleCollider>();
        useSeparateBrakingDeceleration = true;
        originalHeight = capsule.height;
        originalCenter = capsule.center;
        originalCameraPos = cameraTransform.localPosition;
        jointOriginalPos = joint.localPosition;

    }

    public void Update()
    {
        HandleWallRun();
        CheckForWall(out Vector3 hitNormal);
        if (enableHeadBob)
        {
            HeadBob();
        }


        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSmoothSpeed);
        cameraTransform.localRotation = Quaternion.Euler(0f, 0f, currentTilt);

    }

    public void FixedUpdate()
    {
        Health = GetSpeed() * 5f;
    }

    private void HeadBob()
    {
        if (GetSpeed() > 1 && IsGrounded() && !IsSliding())
        {
            timer += Time.deltaTime * (bobSpeed + GetSpeed());
            joint.localPosition = new Vector3(jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x,
                jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y,
                jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z);
        }
        else
        {
            timer = 0;
            joint.localPosition = new Vector3(
                Mathf.Lerp(joint.localPosition.x, jointOriginalPos.x, Time.deltaTime * bobSpeed),
                Mathf.Lerp(joint.localPosition.y, jointOriginalPos.y, Time.deltaTime * bobSpeed),
                Mathf.Lerp(joint.localPosition.z, jointOriginalPos.z, Time.deltaTime * bobSpeed));
        }
    }

    private void HandleWallRun()
    {
        if (IsGrounded())
        {
            if (isWallRunning)
                StopWallRun();
            return;
        }

        if (!isWallRunning && !IsGrounded() && CheckForWall(out wallNormal))
        {
            StartWallRun();
        }

        if (isWallRunning)
        {
            if (Input.GetKeyDown("space"))
            {
                WallRunJump();
            }

            wallRunTimer -= Time.deltaTime;
            if (wallRunTimer <= 0f || !CheckForWall(out wallNormal))
            {
                StopWallRun();
                IsJumping();
            }
        }

    }

    private void StartWallRun()
    {
        if (characterMovement.velocity.y > 0)
        {
            characterMovement.velocity = new Vector3(
                characterMovement.velocity.x,
                Mathf.Min(characterMovement.velocity.y, 1f), // empêche de voler trop haut
                characterMovement.velocity.z
            );
        }

        isWallRunning = true;
        wallRunTimer = wallRunDuration;
        gravityScale = wallRunGravity;

        Vector3 wallDirection = Vector3.Cross(Vector3.up, wallNormal);
        if (Vector3.Dot(wallDirection, GetForwardVector()) < 0)
        {
            wallDirection = -wallDirection;
        }

        float side = Vector3.Dot(wallNormal, transform.right);
        targetTilt = side > 0 ? -wallRunTilt : wallRunTilt;
    }

    private void StopWallRun()
    {
        isWallRunning = false;
        gravityScale = 1f;
        targetTilt = 0f;
    }

    private bool CheckForWall(out Vector3 hitNormal)
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * wallRaycastHeight;
        Vector3 right = transform.right;
        Vector3 left = -transform.right;

        Debug.DrawRay(origin, right * wallCheckDistance, Color.red);
        Debug.DrawRay(origin, left * wallCheckDistance, Color.blue);

        if (Physics.Raycast(origin, right, out hit, wallCheckDistance))
        {
            hitNormal = hit.normal;
            return true;
        }
        else if (Physics.Raycast(origin, left, out hit, wallCheckDistance))
        {
            hitNormal = hit.normal;
            return true;
        }

        hitNormal = Vector3.zero;
        return false;
    }

    private void WallRunJump()
    {
        // direction parallèle au mur
        Vector3 wallDirection = Vector3.Cross(Vector3.up, wallNormal);
        if (Vector3.Dot(wallDirection, GetForwardVector()) < 0)
            wallDirection = -wallDirection;

        // direction combinée : vers l'avant + haut + un peu de recul
        Vector3 jumpDirection = (wallDirection * 1.2f + Vector3.up * 1f + wallNormal * 0.2f).normalized;

        // appliquer l'impulsion
        characterMovement.velocity = jumpDirection * jumpImpulseStrength;

        StopWallRun();
    }
}
