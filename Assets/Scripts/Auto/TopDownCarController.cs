using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCarController : MonoBehaviour
{
    [Header("Ajustes del carro")]
    public float driftFactor = 0.95f;
    public float accelerationFactor = 30.0f;
    public float turnFactor = 3.5f;
    public float maxSpeed = 20;

    [Header("Sprites")]
    public SpriteRenderer carSpriteRenderer;
    public SpriteRenderer carShadowRenderer;

    [Header("Saltar")]
    public AnimationCurve jumpCurve;
    public ParticleSystem landingParticleSystem;

    // Variables locales
    float accelerationInput = 0;
    float steeringInput = 0;

    float rotationAngle = 0;
    float velocityVsUp = 0;

    bool isJumping = false;

    // Componentes
    Rigidbody2D carRigidbody2D;
    Collider2D carCollider;
    CarSFXHandler carSfxHandler;
    CarSurfaceHandler carSurfaceHandler;

    void Awake()
    {
        carRigidbody2D = GetComponent<Rigidbody2D>();
        carCollider = GetComponentInChildren<Collider2D>();
        carSfxHandler = GetComponent<CarSFXHandler>();
        carSurfaceHandler = GetComponent<CarSurfaceHandler>();
    }

    void Start()
    {
        rotationAngle = transform.rotation.eulerAngles.z;
    }

    // Frame-rate independiente para físicas
    void FixedUpdate()
    {
        if (GameManager.instance.GetGameState() == GameStates.countDown)
            return;

        ApplyEngineForce();

        KillOrthogonalVelocity();

        ApplySteering();
    }

    void ApplyEngineForce()
    {
        // No dejar frenar en el aire
        if (isJumping && accelerationInput < 0)
            accelerationInput = 0;

        // Frenado automático
        if (accelerationInput == 0)
            carRigidbody2D.linearDamping = Mathf.Lerp(carRigidbody2D.linearDamping, 3.0f, Time.fixedDeltaTime * 3);
        else
            carRigidbody2D.linearDamping = 0;

        // Ajustes según superficie
        switch (GetSurface())
        {
            case Surface.SurfaceTypes.Sand:
                carRigidbody2D.linearDamping = Mathf.Lerp(carRigidbody2D.linearDamping, 9.0f, Time.fixedDeltaTime * 3);
                break;

            case Surface.SurfaceTypes.Grass:
                carRigidbody2D.linearDamping = Mathf.Lerp(carRigidbody2D.linearDamping, 10.0f, Time.fixedDeltaTime * 3);
                break;

            case Surface.SurfaceTypes.Oil:
                carRigidbody2D.linearDamping = 0;
                accelerationInput = Mathf.Clamp(accelerationInput, 0, 1.0f);
                break;
        }

        // Velocidad hacia adelante
        velocityVsUp = Vector2.Dot(transform.up, carRigidbody2D.linearVelocity);

        // Limitar velocidad máxima adelante
        if (velocityVsUp > maxSpeed && accelerationInput > 0)
            return;

        // Limitar reversa
        if (velocityVsUp < -maxSpeed * 0.5f && accelerationInput < 0)
            return;

        // Limitar velocidad total
        if (carRigidbody2D.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed &&
            accelerationInput > 0 &&
            !isJumping)
            return;

        // Fuerza del motor
        Vector2 engineForceVector =
            transform.up * accelerationInput * accelerationFactor;

        carRigidbody2D.AddForce(engineForceVector, ForceMode2D.Force);
    }

    void ApplySteering()
    {
        // Limitar giro a baja velocidad
        float minSpeedBeforeAllowTurningFactor =
            carRigidbody2D.linearVelocity.magnitude / 2;

        minSpeedBeforeAllowTurningFactor =
            Mathf.Clamp01(minSpeedBeforeAllowTurningFactor);

        // Rotación estable con deltaTime
        rotationAngle -=
            steeringInput *
            turnFactor *
            minSpeedBeforeAllowTurningFactor *
            Time.fixedDeltaTime *
            100;

        carRigidbody2D.MoveRotation(rotationAngle);
    }

    void KillOrthogonalVelocity()
    {
        // Velocidad adelante
        Vector2 forwardVelocity =
            transform.up *
            Vector2.Dot(carRigidbody2D.linearVelocity, transform.up);

        // Velocidad lateral
        Vector2 rightVelocity =
            transform.right *
            Vector2.Dot(carRigidbody2D.linearVelocity, transform.right);

        float currentDriftFactor = driftFactor;

        // Ajustar derrape según superficie
        switch (GetSurface())
        {
            case Surface.SurfaceTypes.Sand:
                currentDriftFactor *= 1.05f;
                break;

            case Surface.SurfaceTypes.Oil:
                currentDriftFactor = 1.00f;
                break;
        }

        // Aplicar derrape correcto
        carRigidbody2D.linearVelocity =
            forwardVelocity + rightVelocity * currentDriftFactor;
    }

    float GetLateralVelocity()
    {
        return Vector2.Dot(transform.right, carRigidbody2D.linearVelocity);
    }

    public bool IsTireScreeching(out float lateralVelocity, out bool isBraking)
    {
        lateralVelocity = GetLateralVelocity();
        isBraking = false;

        if (isJumping)
            return false;

        // Frenado
        if (accelerationInput < 0 && velocityVsUp > 0)
        {
            isBraking = true;
            return true;
        }

        // Derrape lateral
        if (Mathf.Abs(GetLateralVelocity()) > 4.0f)
            return true;

        return false;
    }

    public void SetInputVector(Vector2 inputVector)
    {
        steeringInput = inputVector.x;
        accelerationInput = inputVector.y;
    }

    public float GetVelocityMagnitude()
    {
        return carRigidbody2D.linearVelocity.magnitude;
    }

    public Surface.SurfaceTypes GetSurface()
    {
        if (carSurfaceHandler == null)
            return Surface.SurfaceTypes.Road;

        return carSurfaceHandler.GetCurrentSurface();
    }

    public void Jump(float jumpHeightScale, float jumpPushScale, int carColliderLayerBeforeJump)
    {
        if (!isJumping)
            StartCoroutine(JumpCo(jumpHeightScale, jumpPushScale, carColliderLayerBeforeJump));
    }

    private IEnumerator JumpCo(float jumpHeightScale, float jumpPushScale, int carColliderLayerBeforeJump)
    {
        isJumping = true;

        float jumpStartTime = Time.time;

        // Evitar duración 0
        float jumpDuration =
            Mathf.Max(carRigidbody2D.linearVelocity.magnitude * 0.05f, 0.1f);

        jumpHeightScale =
            jumpHeightScale *
            carRigidbody2D.linearVelocity.magnitude *
            0.05f;

        jumpHeightScale =
            Mathf.Clamp(jumpHeightScale, 0.0f, 1.0f);

        // Cambiar capa
        carCollider.gameObject.layer =
            LayerMask.NameToLayer("ObjectFlying");

        carSfxHandler.PlayJumpSFX();

        // Sorting layer
        carSpriteRenderer.sortingLayerName = "Flying";
        carShadowRenderer.sortingLayerName = "Flying";

        // Impulso del salto
        carRigidbody2D.AddForce(
            carRigidbody2D.linearVelocity.normalized *
            jumpPushScale *
            10,
            ForceMode2D.Impulse
        );

        while (isJumping)
        {
            float jumpCompletedPercentage =
                (Time.time - jumpStartTime) / jumpDuration;

            jumpCompletedPercentage =
                Mathf.Clamp01(jumpCompletedPercentage);

            // Escala auto
            carSpriteRenderer.transform.localScale =
                Vector3.one +
                Vector3.one *
                jumpCurve.Evaluate(jumpCompletedPercentage) *
                jumpHeightScale;

            // Escala sombra
            carShadowRenderer.transform.localScale =
                carSpriteRenderer.transform.localScale * 0.75f;

            // Offset sombra
            carShadowRenderer.transform.localPosition =
                new Vector3(1, -1, 0.0f) *
                3 *
                jumpCurve.Evaluate(jumpCompletedPercentage) *
                jumpHeightScale;

            if (jumpCompletedPercentage >= 1.0f)
                break;

            yield return null;
        }

        // Desactivar collider
        carCollider.enabled = false;

        ContactFilter2D contactFilter2D = new ContactFilter2D();
        contactFilter2D.useTriggers = false;

        Collider2D[] hitResults = new Collider2D[2];

        int numberOfHitObjects =
            Physics2D.OverlapCircle(
                transform.position,
                1.5f,
                contactFilter2D,
                hitResults
            );

        // Reactivar collider
        carCollider.enabled = true;

        // Revisar aterrizaje
        if (numberOfHitObjects != 0)
        {
            isJumping = false;

            Jump(0.2f, 0.6f, carColliderLayerBeforeJump);
        }
        else
        {
            // Restaurar escalas
            carSpriteRenderer.transform.localScale = Vector3.one;

            carShadowRenderer.transform.localPosition = Vector3.zero;
            carShadowRenderer.transform.localScale =
                carSpriteRenderer.transform.localScale;

            // Restaurar capa
            carCollider.gameObject.layer =
                carColliderLayerBeforeJump;

            // Sorting layer normal
            carSpriteRenderer.sortingLayerName = "Default";
            carShadowRenderer.sortingLayerName = "Default";

            // Partículas aterrizaje
            if (jumpHeightScale > 0.2f)
            {
                landingParticleSystem.Play();

                carSfxHandler.PlayLandingSFX();
            }

            isJumping = false;
        }
    }

    public bool IsJumping()
    {
        return isJumping;
    }

    void OnTriggerEnter2D(Collider2D collider2d)
    {
        if (collider2d.CompareTag("Jump"))
        {
            JumpData jumpData = collider2d.GetComponent<JumpData>();

            Jump(
                jumpData.jumpHeightScale,
                jumpData.jumpPushScale,
                carCollider.gameObject.layer
            );
        }
    }
}