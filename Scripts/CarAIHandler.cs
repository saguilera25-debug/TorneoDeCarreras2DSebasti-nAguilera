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

    // Variables
    Vector3 targetPosition;
    Transform targetTransform;
    float originalMaximumSpeed;

    // Stuck system
    bool isRunningStuckCheck;
    bool isFirstTemporaryWaypoint;
    int stuckCheckCounter;
    List<Vector2> temporaryWaypoints = new List<Vector2>();
    float angleToTarget;

    // Avoidance
    Vector2 avoidanceVectorLerped;

    // Waypoints
    WaypointNode currentWaypoint;
    WaypointNode previousWaypoint;
    WaypointNode[] allWayPoints;

    // Components
    PolygonCollider2D polygonCollider2D;
    TopDownCarController topDownCarController;
    AStarLite aStarLite;

    void Awake()
    {
        topDownCarController = GetComponent<TopDownCarController>();
        aStarLite = GetComponent<AStarLite>();
        polygonCollider2D = GetComponentInChildren<PolygonCollider2D>();

        allWayPoints = FindObjectsByType<WaypointNode>(FindObjectsSortMode.None);

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

        // Stuck detection
        if (topDownCarController.GetVelocityMagnitude() < 0.5f &&
            Mathf.Abs(inputVector.y) > 0.01f &&
            !isRunningStuckCheck)
        {
            StartCoroutine(StuckCheckCO());
        }

        if (stuckCheckCounter >= 4 && !isRunningStuckCheck)
        {
            StartCoroutine(StuckCheckCO());
        }

        topDownCarController.SetInputVector(inputVector);
    }

    void FollowPlayer()
    {
        if (targetTransform == null)
            targetTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (targetTransform != null)
            targetPosition = targetTransform.position;
    }

    void FollowWaypoints()
    {
        if (currentWaypoint == null)
        {
            currentWaypoint = FindClosestWayPoint();
            previousWaypoint = currentWaypoint;
        }

        if (currentWaypoint == null) return;

        targetPosition = currentWaypoint.transform.position;

        float distance = Vector2.Distance(transform.position, targetPosition);

        if (distance <= currentWaypoint.minDistanceToReachWaypoint)
        {
            float speed = currentWaypoint.maxSpeed > 0 ? currentWaypoint.maxSpeed : originalMaximumSpeed;
            SetMaxSpeedBasedOnSkillLevel(speed);

            previousWaypoint = currentWaypoint;

            if (currentWaypoint.nextWaypointNode.Length > 0)
                currentWaypoint = currentWaypoint.nextWaypointNode[Random.Range(0, currentWaypoint.nextWaypointNode.Length)];
        }
    }

    void FollowTemporaryWayPoints()
    {
        if (temporaryWaypoints.Count == 0) return;

        targetPosition = temporaryWaypoints[0];

        float distance = Vector2.Distance(transform.position, targetPosition);

        SetMaxSpeedBasedOnSkillLevel(5);

        float minDistance = isFirstTemporaryWaypoint ? 3f : 1.5f;

        if (distance <= minDistance)
        {
            temporaryWaypoints.RemoveAt(0);
            isFirstTemporaryWaypoint = false;
        }
    }

    void FollowMousePosition()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPosition = worldPosition;
    }

    WaypointNode FindClosestWayPoint()
    {
        return allWayPoints
            .OrderBy(t => Vector2.Distance(transform.position, t.transform.position))
            .FirstOrDefault();
    }

    float TurnTowardTarget()
    {
        Vector2 direction = (targetPosition - transform.position).normalized;

        if (isAvoidingCars && !topDownCarController.IsJumping())
            AvoidCars(direction, out direction);

        angleToTarget = -Vector2.SignedAngle(transform.up, direction);

        float steer = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);

        return steer;
    }

    float ApplyThrottleOrBrake(float inputX)
    {
        if (topDownCarController.GetVelocityMagnitude() > maxSpeed)
            return 0;

        float throttle = 1f;

        if (temporaryWaypoints.Count > 0)
        {
            if (Mathf.Abs(angleToTarget) > 70)
                throttle = -1f;
        }

        return throttle;
    }

    void SetMaxSpeedBasedOnSkillLevel(float newMaxSpeed)
    {
        float clamped = Mathf.Clamp(newMaxSpeed, 0, originalMaximumSpeed);
        float skill = Mathf.Clamp(skillLevel, 0.3f, 1f);

        maxSpeed = clamped * skill;
    }

    bool IsCarsInFrontOfAICar(out Vector3 position, out Vector3 rightVector)
    {
        polygonCollider2D.enabled = false;

        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position + transform.up * 0.5f,
            1.2f,
            transform.up,
            12,
            1 << LayerMask.NameToLayer("Car")
        );

        polygonCollider2D.enabled = true;

        if (hit.collider != null)
        {
            position = hit.collider.transform.position;
            rightVector = hit.collider.transform.right;
            return true;
        }

        position = Vector3.zero;
        rightVector = Vector3.zero;
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