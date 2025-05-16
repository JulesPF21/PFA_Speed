using System;
using ECM2;
using ECM2.Examples.FirstPerson;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerController : FirstPersonCharacter
{
    public float Health = 100f;
    
    public VisualEffect speedEffect;
    
    public AudioSource jumpSound;
    public AudioSource landingSound;
    private bool wasGroundedLastFrame = true;
    public AudioSource stepSound;
    private float stepTimer = 0f;
    public float stepInterval = 0.4f;
    public float baseStepSpeed = 5f;
    
    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(.15f, .05f, 0f);

    public Transform cameraTransform;
    private Vector3 originalCameraPos;
    public Camera playerCamera;
    public float fov = 60f;
    
    private float originalHeight;
    private Vector3 originalCenter;
    private Vector3 originalCameraLocalPos;

    [SerializeField] private CapsuleCollider capsule;

    public float slideCameraHeight = -0.5f;
    public float slideColliderHeight = 1.0f;
    public Vector3 slideColliderCenter = new Vector3(0, 0.5f, 0);

    [Header("Sliding Parameters")] 
    [SerializeField] private Animator anim;
    
    [Header("Sliding Parameters")] 
    public KeyCode slideKey = KeyCode.LeftShift;
    public float slideImpulse = 20.0f;
    public float slideDownAcceleration = 20.0f;
    public float slideGravity = 3f;
    
    [Header("Wall Run Settings")] 
    public float wallRunDuration = 1.5f;
    public float wallRunGravity = 1f;
    public float wallRunSpeed = 10f;
    public float wallCheckDistance = 1f;
    public float jumpImpulseStrength = 12f;
    private bool applyWallJumpNextFrame = false;
    private Vector3 pendingWallJumpVelocity;
    public float wallRaycastHeight = 2f;

    private float wallRunTimer;
    private bool isWallRunning;
    private Vector3 wallNormal;

    private Vector3 jointOriginalPos;
    private float timer = 0;

    [Header("Wall Run Camera Effects")] 
    public float wallRunTilt = 15f;
    public float tiltSmoothSpeed = 5f;
    private float targetTilt = 0f;
    private float currentTilt = 0f;
    public Transform playerBody;
    public Transform tiltTransform;
    
    [Header("Ledge Propulsion")]
    [SerializeField] private float ledgeCheckDistance = 0.6f;
    [SerializeField] private float propulsionSpeed = 10f;
    [SerializeField] private float propulsionDuration = 0.2f;
    [SerializeField] private LayerMask wallLayer;

    private bool isPropelling = false;
    private float propulsionTimer;
    private Vector3 propulsionDirection;
    private bool canPropelFromLedge = true;
   
    private void Start()
    {
        capsule = GetComponent<CapsuleCollider>();
        useSeparateBrakingDeceleration = true;
        originalHeight = capsule.height;
        originalCenter = capsule.center;
        originalCameraPos = cameraTransform.localPosition;
        jointOriginalPos = joint.localPosition;

    }
    private float autoWalkDuration = 2f;

    public void Update()
    {
        
        speedEffect.SetFloat("SpawnRate", GetSpeed() * 2f);
        playerCamera.fieldOfView = fov + GetSpeed();
        if (!wasGroundedLastFrame && IsGrounded())
        {
            if (landingSound && !landingSound.isPlaying)
                landingSound.Play();
        }

        if (_isJumping)
        {
            jumpSound.Play();
            anim.SetBool ("Jump", true);
        }
        else
        {
            anim.SetBool ("Jump", false);
        }    
        wasGroundedLastFrame = IsGrounded();
        HandleWallRun();
        HandleWallRunFootsteps();
        if (!isPropelling)
            CheckLedgePropulsion();

        if (isPropelling)
        {
            propulsionTimer -= Time.deltaTime;
            if (propulsionTimer <= 0f)
                isPropelling = false;
            else
                characterMovement.velocity = propulsionDirection * propulsionSpeed;
        }
        CheckForWall(out Vector3 hitNormal);
        if (enableHeadBob && !IsSliding())
        {
            if (IsGrounded())
            {
                HeadBob();
            }
            else if (isWallRunning)
            {
                WallRunBob();
            }
            else
            {
                ResetHeadBob();
            }
            if (IsGrounded() && GetSpeed() > 0.1f)
            {
                stepSound.pitch = Mathf.Lerp(0.8f, 1.4f, GetSpeed() / maxWalkSpeed);
            }
            else
            {
                stepSound.pitch = 1f;
            }
        }

        if (IsSliding())
        {
            anim.SetBool ("Slide", true);
        }
        else
        {
            anim.SetBool ("Slide", false);
        }
        
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSmoothSpeed);
        Quaternion tiltRot = Quaternion.Euler(0f, 0f, currentTilt);
        tiltTransform.localRotation = tiltRot;
        

    }
    
    public void FixedUpdate()
    {
        if (applyWallJumpNextFrame)
        {
            characterMovement.velocity = pendingWallJumpVelocity;
            applyWallJumpNextFrame = false;
        }

        if (autoWalkDuration > 0f)
        {
            autoWalkDuration -= Time.fixedDeltaTime;
            characterMovement.velocity = transform.forward * maxWalkSpeed;
        }

        Health = GetSpeed() * 5f;
    }
    
    private void CheckLedgePropulsion()
    {
        // Check mur devant à hauteur torse
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        bool wallInFront = Physics.Raycast(origin, transform.forward, ledgeCheckDistance, wallLayer);

        // Check espace au-dessus
        bool spaceAbove = !Physics.Raycast(origin + Vector3.up * 1f, transform.forward, ledgeCheckDistance, wallLayer);

        bool isNearLedge = wallInFront && spaceAbove;

        if (isNearLedge && Input.GetKeyDown(KeyCode.Space) && canPropelFromLedge)
        {
            isPropelling = true;
            propulsionTimer = propulsionDuration;
            propulsionDirection = (transform.forward + Vector3.up * 0.75f).normalized;

            canPropelFromLedge = false;
            Invoke(nameof(ResetLedgePropulsion), 0.5f);
        }
    }
    
    private void ResetLedgePropulsion()
    {
        canPropelFromLedge = true;
    }
    
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
        float sqrSpeed = velocity.sqrMagnitude;
        float slideSpeedThreshold = GetSpeed() * maxWalkSpeedCrouched;

        if (IsGrounded())
        {
            return sqrSpeed >= slideSpeedThreshold * 1.02f;
        }
        else
            
        {return true;
        }
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

        if (wantsToSlide && !isSliding && CanSlide())
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
            if (!IsGrounded())
                gravityScale = slideGravity;
            SetRotationMode(RotationMode.None);

            capsule.height = slideColliderHeight;
            capsule.center = slideColliderCenter;

            cameraTransform.localPosition = originalCameraPos + new Vector3(0, slideCameraHeight, 0);
        }

        bool wasSliding = prevMovementMode == MovementMode.Custom &&
                          prevCustomMode == (int)ECustomMovementMode.Sliding;

        if (wasSliding)
        {
            gravityScale = 1f;
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

        if (IsGrounded())
        {
            Vector3 slideDownDirection =
                Vector3.ProjectOnPlane(GetGravityDirection(), characterMovement.groundNormal).normalized;
            characterMovement.velocity += gravityScale * deltaTime * slideDownDirection;
        }
        else
        {
            characterMovement.velocity += gravityScale * deltaTime * GetGravityDirection();
        }

        if (applyStandingDownwardForce)
            ApplyDownwardsForce();
    }

    protected override void CustomMovementMode(float deltaTime)
    {
        base.CustomMovementMode(deltaTime);

        if (customMovementMode == (int)ECustomMovementMode.Sliding)
        {
            SlidingMovementMode(deltaTime);
        }
    }

    private void HeadBob()
    {
        float speed = GetSpeed();

        if (speed > 1f && IsGrounded() && !IsSliding())
        {
            timer += Time.deltaTime * (bobSpeed + speed);
            joint.localPosition = new Vector3(
                jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x,
                jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y,
                jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z
            );
            
            stepTimer += Time.deltaTime * speed;
            
            float dynamicStepInterval = Mathf.Clamp(baseStepSpeed / speed, 0.1f, 1.2f);

            if (stepTimer >= dynamicStepInterval && !stepSound.isPlaying)
            {
                stepSound.Play();
                stepTimer = 0f;
            }
        }
        else
        {
            timer = 0f;
            stepTimer = 0f;
            joint.localPosition = Vector3.Lerp(joint.localPosition, jointOriginalPos, Time.deltaTime * bobSpeed);
        }
    }
    private void ResetHeadBob()
    {
        timer = 0;
        joint.localPosition = Vector3.Lerp(joint.localPosition, jointOriginalPos, Time.deltaTime * bobSpeed);
    }
    private float wallStepTimer = 0f;

    private void HandleWallRunFootsteps()
    {
        if (!isWallRunning || GetSpeed() < 1f)
            return;

        wallStepTimer -= Time.deltaTime;

        float interval = Mathf.Lerp(0.5f, 0.15f, GetSpeed() / wallRunSpeed); // dépend de la vitesse

        if (wallStepTimer <= 0f)
        {
            stepSound.pitch = Mathf.Lerp(0.9f, 1.3f, GetSpeed() / wallRunSpeed);
            stepSound.Play();
            wallStepTimer = interval;
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

        if (Physics.Raycast(origin, right, out hit, wallCheckDistance, wallLayer))
        {
            hitNormal = hit.normal;
            return true;
        }
        else if (Physics.Raycast(origin, left, out hit, wallCheckDistance, wallLayer))
        {
            hitNormal = hit.normal;
            return true;
        }

        hitNormal = Vector3.zero;
        return false;
    }

    private void WallRunJump()
    {
        if (jumpSound && !jumpSound.isPlaying)
            jumpSound.Play();
        Vector3 wallDirection = Vector3.Cross(Vector3.up, wallNormal);
        if (Vector3.Dot(wallDirection, GetForwardVector()) < 0)
            wallDirection = -wallDirection;

        Vector3 jumpDirection = (wallDirection * 2f + Vector3.up * 2f + wallNormal * 0.3f).normalized;
        
        pendingWallJumpVelocity = jumpDirection * (jumpImpulseStrength * 1.5f);
        applyWallJumpNextFrame = true;

        StopWallRun();
    }
    private void WallRunBob()
    {
        timer += Time.deltaTime * (bobSpeed + GetSpeed()); // un bob plus smooth
        joint.localPosition = new Vector3(
            jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x * 0.5f,
            jointOriginalPos.y + Mathf.Cos(timer * 0.5f) * bobAmount.y * 0.3f,
            jointOriginalPos.z
        );
    }
}
