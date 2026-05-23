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

    //Variables locales
    float accelerationInput = 0;
    float steeringInput = 0;

    float rotationAngle = 0;

    float velocityVsUp = 0;

    bool isJumping = false;

    //Componentes
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

        if (carRigidbody2D == null)
            Debug.LogError("No se encontró Rigidbody2D en el auto.");

        if (carCollider == null)
            Debug.LogError("No se encontró Collider2D en los hijos del auto.");

        if (carSpriteRenderer == null)
            Debug.LogError("No se asignó carSpriteRenderer.");

        if (carShadowRenderer == null)
            Debug.LogError("No se asignó carShadowRenderer.");
    }

    //Start se utiliza para llamar a la actualización antes del primer frame.
    void Start()
    {
        rotationAngle = transform.rotation.eulerAngles.z;
    }

    //Frame-rate independiente para calculaciones físicas.
    void FixedUpdate()
    {
        //Protección por si no existe el GameManager todavía.
        if (GameManager.instance == null)
            return;

        if (GameManager.instance.GetGameState() == GameStates.countDown)
            return;

        ApplyEngineForce();

        KillOrthogonalVelocity();

        ApplySteering();
    }

    void ApplyEngineForce()
    {
        //Variable temporal para no modificar permanentemente el input original.
        float currentAccelerationInput = accelerationInput;

        //No dejar que el jugador frene mientras está en el aire,
        //pero todavía permitimos algo de arrastre cuando desacelera.
        if (isJumping && currentAccelerationInput < 0)
            currentAccelerationInput = 0;

        //Aplicar arrastre si no hay accelerationInput
        //para que el auto se detenga cuando el jugador suelta el acelerador.
        if (currentAccelerationInput == 0)
            carRigidbody2D.linearDamping = Mathf.Lerp(carRigidbody2D.linearDamping, 3.0f, Time.fixedDeltaTime * 3);
        else
            carRigidbody2D.linearDamping = Mathf.Lerp(carRigidbody2D.linearDamping, 0, Time.fixedDeltaTime * 10);

        //Aplicar más arrastre dependiendo de la superficie.
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

                //Evita que el jugador acelere en reversa sobre aceite.
                currentAccelerationInput = Mathf.Clamp(currentAccelerationInput, 0, 1.0f);
                break;
        }

        //Evita damping exagerado.
        carRigidbody2D.linearDamping = Mathf.Clamp(carRigidbody2D.linearDamping, 0, 5f);

        //Calcular qué tan adelante estamos yendo
        //en términos de la dirección de nuestra velocidad.
        velocityVsUp = Vector2.Dot(transform.up, carRigidbody2D.linearVelocity);

        //Limita para que no podamos ir más rápido
        //que la velocidad máxima en la dirección de "adelante".
        if (velocityVsUp > maxSpeed && currentAccelerationInput > 0)
            return;

        //Limita para que no podamos ir más rápido
        //que el 50% de la velocidad máxima en reversa.
        if (velocityVsUp < -maxSpeed * 0.5f && currentAccelerationInput < 0)
            return;

        //Limita velocidad total mientras aceleramos.
        if (carRigidbody2D.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed &&
            currentAccelerationInput > 0 &&
            !isJumping)
            return;

        //Crear una fuerza para el motor.
        Vector2 engineForceVector = transform.up * currentAccelerationInput * accelerationFactor;

        //Aplicar fuerza y empuja el auto hacia adelante.
        carRigidbody2D.AddForce(engineForceVector, ForceMode2D.Force);
    }

    void ApplySteering()
    {
        //No permitir girar si el auto está casi detenido.
        if (carRigidbody2D.linearVelocity.magnitude < 0.5f)
            return;

        //Limita la habilidad de girar cuando el auto se mueve lentamente.
        float minSpeedBeforeAllowTurningFactor = (carRigidbody2D.linearVelocity.magnitude / 2);
        minSpeedBeforeAllowTurningFactor = Mathf.Clamp01(minSpeedBeforeAllowTurningFactor);

        //Sincroniza el ángulo con el Rigidbody2D.
        rotationAngle = carRigidbody2D.rotation;

        //Actualiza el ángulo de rotación basado en el input.
        rotationAngle -= steeringInput *
                         turnFactor *
                         minSpeedBeforeAllowTurningFactor *
                         Time.fixedDeltaTime * 100;

        //Aplicar manejo rotando el objeto del auto.
        carRigidbody2D.MoveRotation(rotationAngle);
    }

    void KillOrthogonalVelocity()
    {
        //Consigue velocidad hacia adelante y derecha del auto.
        Vector2 forwardVelocity =
            transform.up * Vector2.Dot(carRigidbody2D.linearVelocity, transform.up);

        Vector2 rightVelocity =
            transform.right * Vector2.Dot(carRigidbody2D.linearVelocity, transform.right);

        float currentDriftFactor = driftFactor;

        //Aplica más arrastre dependiendo de la superficie.
        switch (GetSurface())
        {
            case Surface.SurfaceTypes.Sand:
                currentDriftFactor *= 1.05f;
                break;

            case Surface.SurfaceTypes.Oil:
                currentDriftFactor = 1.00f;
                break;
        }

        //Mata la velocidad ortogonal basado en cuánto derrapa el auto.
        carRigidbody2D.linearVelocity =
            forwardVelocity + rightVelocity * currentDriftFactor;
    }

    float GetLateralVelocity()
    {
        //Regresa qué tan rápido el auto se está moviendo de lado a lado.
        return Vector2.Dot(transform.right, carRigidbody2D.linearVelocity);
    }

    public bool IsTireScreeching(out float lateralVelocity, out bool isBraking)
    {
        lateralVelocity = GetLateralVelocity();
        isBraking = false;

        //Revisar frenado.
        if (accelerationInput < 0 && velocityVsUp > 0)
        {
            isBraking = true;
            return true;
        }

        if (isJumping)
            return false;

        //Revisa si el auto está derrapando.
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
        //Si no existe handler de superficies, usar carretera por defecto.
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

        float jumpDuration =
            Mathf.Max(carRigidbody2D.linearVelocity.magnitude * 0.05f, 0.1f);

        jumpHeightScale =
            jumpHeightScale * carRigidbody2D.linearVelocity.magnitude * 0.05f;

        jumpHeightScale = Mathf.Clamp(jumpHeightScale, 0.0f, 1.0f);

        //Cambiar la capa del auto.
        carCollider.gameObject.layer = LayerMask.NameToLayer("ObjectFlying");

        if (carSfxHandler != null)
            carSfxHandler.PlayJumpSFX();

        //Cambiar sorting layer a Flying.
        carSpriteRenderer.sortingLayerName = "Flying";
        carShadowRenderer.sortingLayerName = "Flying";

        //Empuja el auto hacia adelante.
        carRigidbody2D.AddForce(
            carRigidbody2D.linearVelocity.normalized * jumpPushScale * 10,
            ForceMode2D.Impulse);

        while (isJumping)
        {
            //Porcentaje 0 - 1.0 del proceso de salto.
            float jumpCompletedPercentage =
                (Time.time - jumpStartTime) / jumpDuration;

            jumpCompletedPercentage = Mathf.Clamp01(jumpCompletedPercentage);

            //Escala del auto.
            carSpriteRenderer.transform.localScale =
                Vector3.one +
                Vector3.one *
                jumpCurve.Evaluate(jumpCompletedPercentage) *
                jumpHeightScale;

            //Escala de la sombra.
            carShadowRenderer.transform.localScale =
                carSpriteRenderer.transform.localScale * 0.75f;

            //Desplazamiento de sombra.
            carShadowRenderer.transform.localPosition =
                new Vector3(1, -1, 0.0f) *
                3 *
                jumpCurve.Evaluate(jumpCompletedPercentage) *
                jumpHeightScale;

            //Finaliza salto.
            if (jumpCompletedPercentage >= 1.0f)
                break;

            yield return null;
        }

        //Desactivar collider temporalmente.
        carCollider.enabled = false;

        //No revisar triggers.
        ContactFilter2D contactFilter2D = new ContactFilter2D();
        contactFilter2D.useTriggers = false;

        Collider2D[] hitResults = new Collider2D[10];

        int numberOfHitObjects =
            Physics2D.OverlapCircle(
                transform.position,
                1.5f,
                contactFilter2D,
                hitResults);

        //Reactivar collider.
        carCollider.enabled = true;

        bool safeToLand = true;

        //Revisar si golpeamos algo distinto a nosotros mismos.
        for (int i = 0; i < numberOfHitObjects; i++)
        {
            if (hitResults[i] == null)
                continue;

            if (hitResults[i] == carCollider)
                continue;

            safeToLand = false;
            break;
        }

        //Si no es seguro aterrizar, saltar otra vez.
        if (!safeToLand)
        {
            isJumping = false;

            Jump(0.2f, 0.6f, carColliderLayerBeforeJump);
        }
        else
        {
            //Reinicia escala.
            carSpriteRenderer.transform.localScale = Vector3.one;

            //Reinicia sombra.
            carShadowRenderer.transform.localPosition = Vector3.zero;
            carShadowRenderer.transform.localScale =
                carSpriteRenderer.transform.localScale;

            //Restaurar layer original.
            carCollider.gameObject.layer = carColliderLayerBeforeJump;

            //Restaurar sorting layer.
            carSpriteRenderer.sortingLayerName = "Default";
            carShadowRenderer.sortingLayerName = "Default";

            //Partículas y sonido de aterrizaje.
            if (jumpHeightScale > 0.2f)
            {
                if (landingParticleSystem != null)
                    landingParticleSystem.Play();

                if (carSfxHandler != null)
                    carSfxHandler.PlayLandingSFX();
            }

            //Finaliza estado de salto.
            isJumping = false;
        }
    }

    public bool IsJumping()
    {
        return isJumping;
    }

    //Consigue el trigger del salto.
    void OnTriggerEnter2D(Collider2D collider2d)
    {
        if (collider2d.CompareTag("Jump"))
        {
            //Consigue datos del salto.
            JumpData jumpData = collider2d.GetComponent<JumpData>();

            if (jumpData != null)
            {
                Jump(
                    jumpData.jumpHeightScale,
                    jumpData.jumpPushScale,
                    carCollider.gameObject.layer);
            }
        }
    }
}