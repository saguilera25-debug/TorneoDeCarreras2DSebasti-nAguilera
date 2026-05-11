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

    // Components
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
        if (GameManager.instance.GetGameState() == GameStates.countDown)
            return;

        Vector2 inputVector = Vector2.zero;

        switch (aiMode)
        {
            case AIMode.followPlayer:
                FollowPlayer();
                break;

            case AIMode.followWaypoints:
                if (temporaryWaypoints.Count == 0)
                    FollowWaypoints();
                else FollowTemporaryWayPoints();

                break;

            case AIMode.followMouse:
                FollowMousePosition();
                break;
        }

        inputVector.x = TurnTowardTarget();
        inputVector.y = ApplyThrottleOrBrake(inputVector.x);

        //Si la IA está acelerando pero no logra alcanzar velocidad, entonces vamos a comprobar si se ha quedado atascada.
        if (topDownCarController.GetVelocityMagnitude() < 0.5f && Mathf.Abs(inputVector.y) > 0.01f && !isRunningStuckCheck)
            StartCoroutine(StuckCheckCO());

        //Gestiona el caso especial en el que el coche haya retrocedido durante un rato; entonces comprobará si sigue atascado.Si no lo está, volverá a avanzar.
        if (stuckCheckCounter >= 4 && !isRunningStuckCheck)
            StartCoroutine(StuckCheckCO());

        //Envia el input al controlador de autos.
        topDownCarController.SetInputVector(inputVector);
    }

    //Inteligencia Artificial sigue al jugador.
    void FollowPlayer()
    {
        if (targetTransform == null)
            targetTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (targetTransform != null)
            targetPosition = targetTransform.position;
    }

    //Inteligencia Artificial sigue los waypoints.
    void FollowWaypoints()
    {
        //Escoge el waypoint más cercano si no tenemos listo un waypoint.
        if (currentWaypoint == null)
        {
            currentWaypoint = FindClosestWayPoint();
            previousWaypoint = currentWaypoint;
        }

        //Establece el objetivo en la posición de los waypoints.
        if (currentWaypoint != null)
        {
            //Establece la posición objetivo para la IA.
            targetPosition = currentWaypoint.transform.position;

            //Almacenar qué tan cerca estamos del objetivo.
            float distanceToWayPoint = (targetPosition - transform.position).magnitude;

            Vector3 nearestPointOnTheWayPointLine = FindNearestPointOnLine(previousWaypoint.transform.position, currentWaypoint.transform.position, transform.position);

            targetPosition = nearestPointOnTheWayPointLine;

            Debug.DrawLine(transform.position, targetPosition, Color.cyan);

            //Revisa si estamos lo suficientemente cerca para considerar que llegamos al waypoint.
            if (distanceToWayPoint <= currentWaypoint.minDistanceToReachWaypoint)
            {
                if (currentWaypoint.maxSpeed > 0)
                    maxSpeed = currentWaypoint.maxSpeed;
                else maxSpeed = 1000;

                //Guarda el waypoint reciente como el anterior antes de asignar un nuevo waypoint.
                previousWaypoint = currentWaypoint;

                //Si estamos cerca lo suficiente entonces seguimos al siguiente waypoint, si hay multiples waypoints entonces uno random.
                currentWaypoint = currentWaypoint.nextWaypointNode[Random.Range(0, currentWaypoint.nextWaypointNode.Length)];
            }
        }
    }

    //Inteligencia Artificial sigue los Waypoints.
    void FollowTemporaryWayPoints()
    {
        //Establezca la posición objetivo para la IA.
        targetPosition = temporaryWaypoints[0];

        //Almacenar qué tan cerca estamos del objetivo.
        float distanceToWayPoint = (targetPosition - transform.position).magnitude;

        //Maneja más lento de lo normal.
        SetMaxSpeedBasedOnSkillLevel(5);

        //Revisar si estamos cerca lo suficiente para considerar que hemos llegado al waypoint.
        float minDistanceToReachWaypoint = 1.5f;

        if (!isFirstTemporaryWaypoint)
            minDistanceToReachWaypoint = 3.0f;
        
        if (distanceToWayPoint <= minDistanceToReachWaypoint)
        {
            temporaryWaypoints.RemoveAt(0);
            isFirstTemporaryWaypoint = false;
        }
    }

    //Inteligencia Artificial sigue la posición del ratón.
    void FollowMousePosition()
    {
        //Toma la posición del ratón en el espacio de la pantalla y convertirlo en el espacio mundial.
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Establece la posición objetivo para la IA.
        targetPosition = worldPosition;
    }

    //Encuentra el waypoint cercano para la IA.

    WaypointNode FindClosestWayPoint()
    {
        return allWayPoints
            .OrderBy(t => Vector2.Distance(transform.position, t.transform.position))
            .FirstOrDefault();
    }

    float TurnTowardTarget()
    {
        Vector2 vectorToTarget = targetPosition - transform.position;
        vectorToTarget.Normalize();

        //Aplicar la evitación a la dirección.
        if (isAvoidingCars)
            AvoidCars(vectorToTarget, out vectorToTarget);

        //Calcular un ángulo directo hacia el objetivo.
        angleToTarget = Vector2.SignedAngle(transform.up, vectorToTarget);
        angleToTarget *= -1;

        //Queremos que el auto gire lo antes posible el ángulo es mayor que 45 grados, y queremos que se suavize cuando el ángulo es pequeño.
        float steerAmount = angleToTarget / 45.0f;

        //Sujete la dirección a un valor entre -1 y 1.
        steerAmount = Mathf.Clamp(steerAmount, -1.0f, 1.0f);

        return steerAmount;
    }

    float ApplyThrottleOrBrake(float inputX)
    {
        //Si estamos yendo demasiado rápido, entonces no aceleramos más.
        if (topDownCarController.GetVelocityMagnitude() > maxSpeed)
            return 0;

        //Acelere hacia adelante según la tendencia del coche a girar.
        float reduceSpeedDueToCornering = Mathf.Abs(inputX);

        //Aplicar el acelerador basado en curvas y habilidad
        float throttle = 1.05f - reduceSpeedDueToCornering * skillLevel;

        //Maneja el acelerador de forma diferente cuando seguimos puntos temporales.
        if (temporaryWaypoints.Count != 0)
        {
            //Si el ángulo para alcanzar el objetivo es mayor, es mejor dar marcha atrás.
            if (angleToTarget > 70 || angleToTarget < -70)
            {
                throttle *= -1;
            }
        }

        return throttle;
    }

    void SetMaxSpeedBasedOnSkillLevel(float newSpeed)
    {
        maxSpeed = Mathf.Clamp(newSpeed, 0, originalMaximumSpeed);

        float skillbasedMaximumSpeed = Mathf.Clamp(skillLevel, 0.3f, 1.0f);
        maxSpeed = maxSpeed * skillbasedMaximumSpeed;
    }

    //Encuentra el punto más cercano en una línea.
    Vector2 FindNearestPointOnLine(Vector2 lineStartPosition, Vector2 lineEndPosition, Vector2 point)
    {
        //Obtener la dirección como vector
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
    //Revisa para autos adelante del auto.
    bool IsCarsInFrontOfAICar(out Vector3 position, out Vector3 otherCarRightVector)
    {
        //Desactiva el collider de los autos para evitar que el Auto IA se detecte por si solo.
        polygonCollider2D.enabled = false;

        //Realiza el lanzamiento circular delante del coche con un ligero desplazamiento hacia adelante y solo en la capa del coche.
        RaycastHit2D raycastHit2d = Physics2D.CircleCast(transform.position + transform.up * 0.5f, 1.2f, transform.up, 12, 1 << LayerMask.NameToLayer("Car"));

        //Activa los colliders otra vez para que el auto colisione y otros autos puedan detectarlo.
        polygonCollider2D.enabled = true;

        if (raycastHit2d.collider != null)
        {
            //Dibuja una linea roja que muestra que tan largo es la detección, la hacemos roja si hemos detectado otro auto.
            Debug.DrawRay(transform.position, transform.up * 12, Color.red);

            position = raycastHit2d.collider.transform.position;
            otherCarRightVector = raycastHit2d.collider.transform.right;
            return true;
        }
        else
        {
            //No detectamos otro auto asi que dibujamos una linea negra con la distancia que utilizamos para revisar otros autos.
            Debug.DrawRay(transform.position, transform.up * 12, Color.black);
        }
        position = Vector3.zero;
        otherCarRightVector = Vector3.zero;
        return false;
    }

    void AvoidCars(Vector2 currentDir, out Vector2 newDir)
    {
        if (IsCarsInFrontOfAICar(out Vector3 pos, out Vector3 right))
        {
            Vector2 avoid = Vector2.Reflect((pos - transform.position).normalized, right);

            float dist = Vector2.Distance(transform.position, targetPosition);

            float targetInfluence = Mathf.Clamp(6f / dist, 0.3f, 1f);
            float avoidInfluence = 1f - targetInfluence;

            avoidanceVectorLerped = Vector2.Lerp(avoidanceVectorLerped, avoid, Time.fixedDeltaTime * 4);

            newDir = (currentDir * targetInfluence + avoidanceVectorLerped * avoidInfluence).normalized;
            return;
        }

        newDir = currentDir;
    }

    IEnumerator StuckCheckCO()
    {
        Vector3 startPos = transform.position;

        isRunningStuckCheck = true;

        yield return new WaitForSeconds(0.7f);

        if ((transform.position - startPos).sqrMagnitude < 3f)
        {
            if (currentWaypoint != null)
                temporaryWaypoints = aStarLite?.FindPath(currentWaypoint.transform.position) ?? new List<Vector2>();

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