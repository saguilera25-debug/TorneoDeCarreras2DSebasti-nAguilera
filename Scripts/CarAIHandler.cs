using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CarAIHandler : MonoBehaviour
{
    public enum AIMode { followPlayer, followWaypoints, followMouse };

    [Header("Ajustes de la Inteligencia Artificial")]
    public AIMode aiMode;
    public float maxSpeed = 16;
    public bool isAvoidingCars = true;
    [Range(0.0f, 1.0f)]
    public float skillLevel = 1.0f;

    //Variables locales
    Vector3 targetPosition = Vector3.zero;
    Transform targetTransform = null;
    float originalMaximumSpeed = 0;

    //Manejo atascado
    bool isRunningStuckCheck = false;
    bool isFirstTemporaryWaypoint = false;
    int stuckCheckCounter = 0;
    List<Vector2> temporaryWaypoints = new List<Vector2>();
    float angleToTarget = 0;

    //Evitación
    Vector2 avoidanceVectorLerped = Vector3.zero;

    //Waypoints
    WaypointNode currentWaypoint = null;
    WaypointNode previousWaypoint = null;
    WaypointNode[] allWayPoints;

    //Colliders
    PolygonCollider2D polygonCollider2D;

    //Componentes
    TopDownCarController topDownCarController;
    AStarLite aStarLite;

    void Awake()
    {
        topDownCarController = GetComponent<TopDownCarController>();
        allWayPoints = FindObjectsByType<WaypointNode>(FindObjectsSortMode.None);

        aStarLite = GetComponent<AStarLite>();

        polygonCollider2D = GetComponentInChildren<PolygonCollider2D>();

        originalMaximumSpeed = maxSpeed;
    }

    void Start()
    {
        SetMaxSpeedBasedOnSkillLevel(maxSpeed);
    }

    void FixedUpdate()
    {
        Vector2 inputVector = Vector2.zero;

        switch (aiMode)
        {
            case AIMode.followPlayer:
                FollowPlayer();
                break;

            case AIMode.followWaypoints:
                FollowWaypoints();
                break;

            case AIMode.followMouse:
                FollowMousePosition();
                break;
        }

        inputVector.x = TurnTowardTarget();
        inputVector.y = ApplyThrottleOrBrake(inputVector.x);

        //Si la IA está aplicando el acelerador pero no logra alcanzar velocidad, entonces ejecutemos nuestra comprobación de bloqueo.
        if (topDownCarController.GetVelocityMagnitude() < 0.5f && Mathf.Abs(inputVector.y) > 0.01f && !isRunningStuckCheck)
            StartCoroutine(StuckCheckCO());

        //Gestiona el caso especial en el que el coche haya retrocedido durante un rato; entonces comprobará si sigue atascado. Si no lo está, volverá a avanzar.
        if (stuckCheckCounter >= 4 && !isRunningStuckCheck)
            StartCoroutine(StuckCheckCO());

        //Enviar el input al controlador de autos.
        topDownCarController.SetInputVector(inputVector);
    }

    //IA sigue al jugador.

    void FollowPlayer()
    {
        if (targetTransform == null)
            targetTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (targetTransform != null)
            targetPosition = targetTransform.position;
    }

    //IA sigue Waypoints.
    void FollowWaypoints()
    {
        //Escoge el punto de referencia más cercano si no tenemos uno listo.
        if (currentWaypoint == null)
        {
            currentWaypoint = FindClosestWayPoint();
            previousWaypoint = currentWaypoint;
        }

        if (currentWaypoint == null) 
            return;

        targetPosition = currentWaypoint.transform.position;

        float distanceToWayPoint = Vector2.Distance(transform.position, targetPosition);

        if (distanceToWayPoint <= currentWaypoint.minDistanceToReachWaypoint)
        {
            float speed = currentWaypoint.maxSpeed > 0 ? currentWaypoint.maxSpeed : originalMaximumSpeed;
            SetMaxSpeedBasedOnSkillLevel(speed);

            previousWaypoint = currentWaypoint;

            if (currentWaypoint.nextWaypointNode.Length > 0)
                currentWaypoint = currentWaypoint.nextWaypointNode[Random.Range(0, currentWaypoint.nextWaypointNode.Length)];
        }
    }

    //IA sigue Waypoints.
    void FollowTemporaryWayPoints()
    {
        //Establece la posición objetiva para la IA.
        if (temporaryWaypoints.Count == 0) 
            return;

        //Guarda que tan cerca estamos al objetivo.
        targetPosition = temporaryWaypoints[0];

        //Guarda que tan cerca estamos al objetivo.
        float distanceToWayPoint = (targetPosition - transform.position).magnitude;

        //Conduce un poco más lento de lo normal.
        SetMaxSpeedBasedOnSkillLevel(5);

        //Revisa si estamos cerca lo suficiente al objetivo.
        float minDistanceToReachWaypoint = 1.5f;

        if (!isFirstTemporaryWaypoint)
            minDistanceToReachWaypoint = 3.0f;

        if (distanceToWayPoint <= minDistanceToReachWaypoint)
        {
            temporaryWaypoints.RemoveAt(0);
            isFirstTemporaryWaypoint = false;
        }
    }

    //IA sigue el movimiento del ratón
    void FollowMousePosition()
    {
        //Toma la posición del mouse en el espacio de la pantalla y conviertelo en espacio mundial.
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Establece la posición objetivo para la IA.
        targetPosition = worldPosition;
    }

    //Encuentra el waypoint cercano a la IA.

    WaypointNode FindClosestWayPoint()
    {
        return allWayPoints.OrderBy(t => Vector3.Distance(transform.position, t.transform.position)).FirstOrDefault();
    }

    float TurnTowardTarget()
    {
        Vector2 vectorToTarget = targetPosition - transform.position;
        vectorToTarget.Normalize();

        //Aplicar evitación a conducir.
        if (isAvoidingCars && !topDownCarController.IsJumping())
            AvoidCars(vectorToTarget, out vectorToTarget);

        //Calcular un ángulo hacia el objetivo.
        angleToTarget = -Vector2.SignedAngle(transform.up, vectorToTarget);
        angleToTarget *= -1;

        //Queremos que el coche gire lo máximo posible si el ángulo es mayor de 45 grados y que se suavice si el ángulo es pequeño.
        float steerAmount = angleToTarget / 45.0f;

        //Sujetar la dirección entrer -1 y 1.
        steerAmount = Mathf.Clamp(steerAmount, -1.0f, 1.0f);

        return steerAmount;
    }

    float ApplyThrottleOrBrake(float inputX)
    {
        //Si estamos yendo demasiado rápido, no aceleramos más.
        if (topDownCarController.GetVelocityMagnitude() > maxSpeed)
            return 0;

        //Aplicamos el acelerador dependiendo de como queremos que el auto gire.
        float reduceSpeedDueToCornering = Mathf.Abs(inputX) / 1.0f;

        //Aplicamos el acelerador basado en tomar curvas y habilidades.
        float throttle = 1.05f - reduceSpeedDueToCornering * skillLevel;

        //Aplicar el acelerador de forma diferente cuando seguimos waypoints temporales.
        if (temporaryWaypoints.Count() != 0)
        {
            //Si el ángulo es más largo para alcanzar el objetivo, lo mejor es ir en reversa.
            if (angleToTarget > 70)
                throttle = throttle * -1;
            else if (angleToTarget < -70)
                throttle = throttle * -1;
            //Si estamos aún atascados después de varios intentos, entonces nos vamos en reversa.
            else if (stuckCheckCounter > 3)
                throttle = throttle * -1;
        }

        //Aplicar el acelerador basado en girar en curvas y habilidades.
        return throttle;
    }

    void SetMaxSpeedBasedOnSkillLevel(float newSpeed)
    {
        maxSpeed = Mathf.Clamp(newSpeed, 0, originalMaximumSpeed);

        float skillbasedMaximumSpeed = Mathf.Clamp(skillLevel, 0.3f, 1.0f);
        maxSpeed = maxSpeed * skillbasedMaximumSpeed;
    }

    //Encuentra el punto más cercano en la linea.

    Vector2 FindNearestPointOnLine(Vector2 lineStartPosition, Vector2 lineEndPosition, Vector2 point)
    {
        //Obtener la dirección como vector.
        Vector2 lineHeadingVector = (lineEndPosition - lineStartPosition);

        //Guarda la distancia máxima
        float maxDistance = lineHeadingVector.magnitude;
        lineHeadingVector.Normalize();

        //Realizar una proyección desde la posición inicial hasta el punto
        Vector2 lineVectorStartToPoint = point - lineStartPosition;
        float dotProduct = Vector2.Dot(lineVectorStartToPoint, lineHeadingVector);

        //Limitar el producto escalar a maxDistance
        dotProduct = Mathf.Clamp(dotProduct, 0f, maxDistance);

        return lineStartPosition + lineHeadingVector * dotProduct;
    }

    //Revisa si hay autos adelante del vehiculo.
    bool IsCarsInFrontOfAICar(out Vector3 position, out Vector3 otherCarRightVector)
    {
        //Desactiva los colliders del auto propio para evitar que el auto IA se detecte solo.
        polygonCollider2D.enabled = false;

        //Realiza el lanzamiento circular delante del coche con un ligero desplazamiento hacia adelante y solo en la capa del auto.
        RaycastHit2D raycastHit2d = Physics2D.CircleCast(transform.position + transform.up * 0.5f, 1.2f,transform.up, 12, 1 << LayerMask.NameToLayer("Car"));

        //Activa los colliders otra vez para que el auto pueda chocar y otros autos puedan detectarlo.
        polygonCollider2D.enabled = true;

        if (raycastHit2d.collider != null)
        {
            //Dibuja una linea roja mostrando que tan largo es la detección, hazla roja si hemos detectado otro auto.
            Debug.DrawRay(transform.position, transform.up * 12, Color.red);

            position = raycastHit2d.collider.transform.position;
            otherCarRightVector = raycastHit2d.collider.transform.right;
            return true;
        }
        else
        {
            //No hemos detectado otro auto asi que dibujamos una linea negra para que la usamos al revisar los otros autos.
            Debug.DrawRay(transform.position, transform.up * 12, Color.black);
        }

        //No hubo un auto detectado pero necesitamos asignar valores asi que regresamos a 0.
        position = Vector3.zero;
        otherCarRightVector = Vector3.zero;

        return false;
    }

    void AvoidCars(Vector2 vectorToTarget, out Vector2 newVectorToTarget)
    {
        if (IsCarsInFrontOfAICar(out Vector3 otherCarPosition, out Vector3 otherCarRightVector))
        {
            Vector2 avoidanceVector = Vector2.zero;

            //Calcula el vector de reflexión si chocáramos con el otro auto.
            avoidanceVector = Vector2.Reflect((otherCarPosition - transform.position).normalized, otherCarRightVector);
               
            float distanceToTarget = (targetPosition - transform.position).magnitude;

            //Queremos poder controlar cuánto deseo tiene la IA de conducir hacia el punto de referencia en lugar de evitar a los demás coches.
            //Cuando más nos acerquemos al Waypoint el deseo para alcanzarlo se incrementa.
            float driveToTargetInfluence = 6.0f / distanceToTarget;

            //Asegurarse que limitemos el valor entre 30% a 100% ya que siempre queremos que la IA quiera alcanzar el waypoint.
            driveToTargetInfluence = Mathf.Clamp(driveToTargetInfluence, 0.30f, 1.0f);

            //El deseo de esquivar el auto es simplemente la inversa de alcanzar el waypoint.
            float avoidanceInfluence = 1.0f - driveToTargetInfluence;

            //Reduce un poco la fluctuación utilizando un lerp.
            avoidanceVectorLerped = Vector2.Lerp(avoidanceVectorLerped, avoidanceVector, Time.fixedDeltaTime * 4);

            //Calcula un nuevo vector hacia el objetivo basándote en el vector de evasión y el deseo de alcanzar el punto de referencia.
            newVectorToTarget = vectorToTarget * driveToTargetInfluence + avoidanceVector * avoidanceInfluence;
            newVectorToTarget.Normalize();

            //Dibuja en verde el vector que indica el vector de evitación.
            Debug.DrawRay(transform.position, avoidanceVector * 10, Color.green);

            //Dibuja en amarillo el vector que seguirá realmente el coche.
            Debug.DrawRay(transform.position, newVectorToTarget * 10, Color.yellow);

            //Terminamos, asi que podemos regresar.
            return;
        }

        //Necesitamos asignar un valor predeterminado si no chocamos con ningún coche antes de salir de la función.
        newVectorToTarget = vectorToTarget;
    }

    IEnumerator StuckCheckCO()
    {
        Vector3 initialStuckPosition = transform.position;

        isRunningStuckCheck = true;

        yield return new WaitForSeconds(0.7f);

        if ((transform.position - initialStuckPosition).sqrMagnitude < 3)
        {
            if (currentWaypoint != null && aStarLite != null)
            {
                temporaryWaypoints = aStarLite.FindPath(currentWaypoint.transform.position);

                if (temporaryWaypoints == null)
                    temporaryWaypoints = new List<Vector2>();
            }

            stuckCheckCounter++;
            isFirstTemporaryWaypoint = true;
        }
        else
        {
            stuckCheckCounter = 0;
        }

        isRunningStuckCheck = false;
    }
}