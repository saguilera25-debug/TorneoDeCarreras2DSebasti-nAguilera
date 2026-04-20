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
    }

    //Start se utiliza para llamar a la actualización antes del primer frame.
    void Start()
    {
        rotationAngle = transform.rotation.eulerAngles.z;
    }

    //Frame-rate independiente para calculaciones físicas.
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
        //No dejar que el jugador frene mientras está en el aire, pero todavia permitimos algo de arrastre cuando desacelera.
        if (isJumping && accelerationInput < 0)
            accelerationInput = 0;

        //Aplicar arrastre si no hay accelerationInput para que el auto se detenga cuando el jugador suelta el acelerador.
        if (accelerationInput == 0)
            carRigidbody2D.linearDamping = Mathf.Lerp(carRigidbody2D.linearDamping, 3.0f, Time.fixedDeltaTime * 3);
        else carRigidbody2D.linearDamping = Mathf.Lerp(carRigidbody2D.linearDamping, 0, Time.fixedDeltaTime * 10);

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
                accelerationInput = Mathf.Clamp(accelerationInput, 0, 1.0f);
                break;
        }

        //Calcular que tan adelante estamos yendo en términos de la dirección de nuestra velocidad.
        velocityVsUp = Vector2.Dot(transform.up, carRigidbody2D.linearVelocity);

        //Limita para que no podamos ir más rápido que la velocidad máxima en la dirección de "adelante".
        if (velocityVsUp < maxSpeed && accelerationInput > 0)
            return;

        //Limita para que no podamos ir más rápido que el 50% de la velocidad máxima en la dirección de "reversa".
        if (velocityVsUp < -maxSpeed * 0.5f && accelerationInput < 0)
            return;

        //Limita para que no podamos ir más rápido en alguna dirección mientras aceleramos.
        if (carRigidbody2D.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed && accelerationInput > 0 && !isJumping)
            return;

        //Crear una fuerza para el motor.
        Vector2 engineForceVector = transform.up * accelerationInput * accelerationFactor;

        //Aplicar fuerza y empuja el auto hacia adelante.
        carRigidbody2D.AddForce(engineForceVector, ForceMode2D.Force);
    }

    void ApplySteering()
    {
        //Limita la habilidad de los carros al girar cuando se mueven lentamente.
        float minSpeedBeforeAllowTurningFactor = (carRigidbody2D.linearVelocity.magnitude / 2);
        minSpeedBeforeAllowTurningFactor = Mathf.Clamp01(minSpeedBeforeAllowTurningFactor);

        //Actualiza el angulo de rotación basado en el input
        rotationAngle -= steeringInput * turnFactor * minSpeedBeforeAllowTurningFactor;

        //Aplicar manejo con rotar el objeto del auto.
        carRigidbody2D.MoveRotation(rotationAngle);
    }

    void KillOrthogonalVelocity()
    {
        //Consigue velocidad de hacia adelante y derecha del auto.
        Vector2 forwardVelocity = transform.up * Vector2.Dot(carRigidbody2D.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(carRigidbody2D.linearVelocity, transform.right);

        float currentDriftFactor = driftFactor;

        //Aplica más arrastre dependiendo de la superficie
        switch (GetSurface())
        {
            case Surface.SurfaceTypes.Sand:
                currentDriftFactor *= 1.05f;
                break;

            case Surface.SurfaceTypes.Oil:
                currentDriftFactor = 1.00f;
                break;

        }

        //Mata a la velocidad ortogonal basado en cúanto derrapa el auto.
        carRigidbody2D.linearVelocity = forwardVelocity + rightVelocity * currentDriftFactor;
    }

    float GetLateralVelocity()
    {
        //Regresa que tan rápido el auto se está moviendo de un lado a otro.
        return Vector2.Dot(transform.right, carRigidbody2D.linearVelocity);
    }

    public bool IsTireScreeching(out float lateralVelocity, out bool isBraking)
    {
        lateralVelocity = GetLateralVelocity();
        isBraking = false;

        if (accelerationInput < 0 && velocityVsUp > 0)
        {
            isBraking = true;
            return true;
        }

        if (isJumping)
            return false;

        //Revisa si nos estamos moviendo hacia adelante y si el jugador está aplicando los frenos. En este caso las llantas deben chirriar.
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

    public void Jump(float jumpHeightScale, float JumpPushScale, int carColliderLayerBeforeJump)
    {
        if (!isJumping)
            StartCoroutine(JumpCo(jumpHeightScale, JumpPushScale, carColliderLayerBeforeJump));
    }

    private IEnumerator JumpCo(float jumpHeightScale, float jumpPushScale, int carColliderLayerBeforeJump)
    {
        isJumping = true;

        float jumpStartTime = Time.time;
        float jumpDuration = Mathf.Max(carRigidbody2D.linearVelocity.magnitude * 0.05f, 0.1f);

        jumpHeightScale = jumpHeightScale * carRigidbody2D.linearVelocity.magnitude * 0.05f;
        jumpHeightScale = Mathf.Clamp(jumpHeightScale, 0.0f, 1.0f);

        //Cambiar la capa del auto, como hemos saltado, ahora estamos volando
        carCollider.gameObject.layer = LayerMask.NameToLayer("ObjectFlying");

        carSfxHandler.PlayJumpSFX();

        //Cambiar la capa de clasificación a flying
        carSpriteRenderer.sortingLayerName = "Flying";
        carShadowRenderer.sortingLayerName = "Flying";

        //Empuja el objeto hacia adelante cuando pasamos un salto
        carRigidbody2D.AddForce(carRigidbody2D.linearVelocity.normalized * jumpPushScale * 10, ForceMode2D.Impulse);

        while (isJumping)
        {
            //Porcentaje 0 - 1.0 de dónde estamos en el proceso de salto.
            float jumpCompletedPercentage = (Time.time - jumpStartTime) / jumpDuration;
            jumpCompletedPercentage = Mathf.Clamp01(jumpCompletedPercentage);

            //Toma la escala base de 1 y añade que tanto debemos incrementar la escala.
            carSpriteRenderer.transform.localScale = Vector3.one + Vector3.one * jumpCurve.Evaluate(jumpCompletedPercentage) * jumpHeightScale;

            //Cambia la escala de sombra pero hazla un poco pequeña.
            carShadowRenderer.transform.localScale = carSpriteRenderer.transform.localScale * 0.75f;

            //Desplaza un poco la sombra.
            carShadowRenderer.transform.localPosition = new Vector3(1, -1, 0.0f) * 3 * jumpCurve.Evaluate(jumpCompletedPercentage) * jumpHeightScale;

            //Cuando alcanzamos el 100%, estamos listos.
            if (jumpCompletedPercentage == 1.0f)
                break;

            yield return null;
        }

        //Desactiva el car collider para que podamos hacer una comprobación compuesta.
        carCollider.enabled = false;

        //No revisar colisiones con triggers
        ContactFilter2D contactFilter2D = new ContactFilter2D();
        contactFilter2D.useTriggers = false;

        Collider2D[] hitResults = new Collider2D[2];

        int numberOfHitObjects = Physics2D.OverlapCircle(transform.position, 1.5f, contactFilter2D, hitResults);

        //Activar el collider otra vez para detectar cosas con el trigger.
        carCollider.enabled = true;

        //Revisar si el aterrizaje es seguro o no, si golpeamos 0 objetos, estamos bien.
        if (numberOfHitObjects != 0)
        {
            //Hay algo debajo del auto asi que saltamos otra vez.
            isJumping = false;

            //Agrega un pequeño salto y empuja el auto hacia adelante un poco.
            Jump(0.2f, 0.6f, carColliderLayerBeforeJump);
        }
        else
        {
            //Maneja el aterrizaje, escala el objeto de vuelta.
            carSpriteRenderer.transform.localScale = Vector3.one;

            //Reinicia la posición y escala de la sombra.
            carShadowRenderer.transform.localPosition = Vector3.zero;
            carShadowRenderer.transform.localScale = carSpriteRenderer.transform.localScale;

            //Estamos seguros para aterrizar, asi que cambia la capa de colisión de vuelta a como estaba antes de que saltaramos.
            carCollider.gameObject.layer = carColliderLayerBeforeJump;

            //Cambia la capa de clasificación a capa regular.
            carSpriteRenderer.sortingLayerName = "Default";
            carShadowRenderer.sortingLayerName = "Default";

            //Reproduce el sistema de particulas de aterrizaje si es un salto más grande.
            if (jumpHeightScale > 0.2f)
            {
                landingParticleSystem.Play();

                carSfxHandler.PlayLandingSFX();
            }

            //Cambia estado
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
            //Consigue el data de salto a través del salto.
            JumpData jumpData = collider2d.GetComponent<JumpData>();
            Jump(jumpData.jumpHeightScale, jumpData.jumpPushScale, carCollider.gameObject.layer);
        }
    }
}