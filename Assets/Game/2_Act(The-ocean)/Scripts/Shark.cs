using UnityEngine;

[AddComponentMenu("#NVJOB/Boids/Shark")]
[RequireComponent(typeof(Rigidbody))]
public class Shark : MonoBehaviour
{
    [Header("General Settings")]
    public float speed = 5f;
    public float walkZone = 100f;
    public Transform camRig;
    public bool debug;

    [Header("Hunting Settings")]
    public float huntingZone = 50f;
    public LayerMask layerFlock; // Сохранено для совместимости старого Inspector.
    public Material sharkMaterial;
    [Range(1f, 180f)] public float viewAngle = 90f;
    public LayerMask visionMask = ~0;
    public float attackSpeed = 20f;
    public float turnSpeed = 120f;
    public float retreatDuration = 2f;
    public float attackTimeout = 8f;

    private enum State { Patrol, Attack, Retreat }
    private State state;
    private Rigidbody body;
    private SubmarineController victim;
    private Vector3 patrolCenter, patrolTarget, retreatDirection;
    private float nextPatrolTime, nextScanTime, stateUntil;
    private Material materialInstance;
    private static readonly int ScriptControl = Shader.PropertyToID("_ScriptControl");

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        patrolCenter = body.position;
        PickPatrolTarget();
        // Не изменяем общий материал других акул.
        Renderer visual = GetComponentInChildren<Renderer>();
        if (sharkMaterial != null && visual != null)
        {
            materialInstance = new Material(sharkMaterial);
            visual.sharedMaterial = materialInstance;
        }
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        if (state == State.Attack &&
            (victim == null || victim.IsDead || !victim.isActiveAndEnabled || Time.time >= stateUntil))
            BeginRetreat();

        if (state == State.Retreat && Time.time >= stateUntil)
        {
            state = State.Patrol;
            victim = null;
            PickPatrolTarget();
        }

        if (state == State.Patrol)
        {
            if (Time.time >= nextPatrolTime || (patrolTarget - body.position).sqrMagnitude < 4f)
                PickPatrolTarget();
            if (Time.time >= nextScanTime)
            {
                nextScanTime = Time.time + 0.2f;
                FindVisibleSubmarine();
            }
        }

        Vector3 direction;
        float currentSpeed;
        if (state == State.Attack && victim != null)
        {
            direction = victim.transform.position - body.position;
            currentSpeed = attackSpeed;
        }
        else if (state == State.Retreat)
        {
            direction = retreatDirection;
            currentSpeed = attackSpeed;
        }
        else
        {
            direction = patrolTarget - body.position;
            currentSpeed = speed;
        }

        Quaternion rotation = body.rotation;
        if (direction.sqrMagnitude > 0.0001f)
            rotation = Quaternion.RotateTowards(rotation,
                Quaternion.LookRotation(direction.normalized), turnSpeed * dt);
        body.MoveRotation(rotation);
        // Отход начинается сразу, даже если визуально акула ещё разворачивается.
        Vector3 movement = state == State.Retreat ? retreatDirection : rotation * Vector3.forward;
        body.MovePosition(body.position + movement * Mathf.Max(0f, currentSpeed) * dt);
        if (materialInstance != null && materialInstance.HasProperty(ScriptControl))
            materialInstance.SetFloat(ScriptControl, Time.time * (state == State.Patrol ? 1f : 2.1f));
        if (debug) Debug.DrawRay(body.position, direction, state == State.Attack ? Color.red : Color.cyan);
    }

    private void FindVisibleSubmarine()
    {
        Collider[] nearby = Physics.OverlapSphere(body.position, huntingZone,
            visionMask, QueryTriggerInteraction.Ignore);
        float closestDistance = float.PositiveInfinity;
        SubmarineController closest = null;
        foreach (Collider candidate in nearby)
        {
            SubmarineController submarine = candidate.GetComponentInParent<SubmarineController>();
            if (submarine == null || !submarine.CompareTag("Submarine") ||
                submarine.IsDead || !submarine.isActiveAndEnabled) continue;
            Vector3 direction = candidate.bounds.center - body.position;
            float distance = direction.magnitude;
            if (distance > huntingZone || distance >= closestDistance ||
                Vector3.Angle(body.rotation * Vector3.forward, direction) > viewAngle * 0.5f) continue;
            bool blocked = false;
            if (distance > 0.001f)
            {
                RaycastHit[] hits = Physics.RaycastAll(body.position, direction / distance,
                    distance, visionMask, QueryTriggerInteraction.Ignore);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.GetComponentInParent<Shark>() == this) continue;
                    if (hit.collider.GetComponentInParent<SubmarineController>() == submarine) continue;
                    blocked = true;
                    break;
                }
            }
            if (blocked) continue;
            closest = submarine;
            closestDistance = distance;
        }
        if (closest != null)
        {
            victim = closest;
            state = State.Attack;
            stateUntil = Time.time + Mathf.Max(0.1f, attackTimeout);
        }
    }

    private void OnCollisionEnter(Collision collision) { TryRam(collision); }
    private void OnCollisionStay(Collision collision) { TryRam(collision); }

    private void TryRam(Collision collision)
    {
        if (state != State.Attack || victim == null) return;
        SubmarineController submarine = collision.collider.GetComponentInParent<SubmarineController>();
        if (submarine != victim) return;
        // Смена состояния ДО урона: несколько коллайдеров дают только один удар.
        BeginRetreat();
        submarine.TakeSharkHit();
    }

    private void BeginRetreat()
    {
        retreatDirection = victim != null
            ? body.position - victim.transform.position
            : -(body.rotation * Vector3.forward);
        if (retreatDirection.sqrMagnitude < 0.001f)
            retreatDirection = -(body.rotation * Vector3.forward);
        retreatDirection.Normalize();
        state = State.Retreat;
        stateUntil = Time.time + Mathf.Max(0.5f, retreatDuration);
    }

    private void PickPatrolTarget()
    {
        Vector2 random = Random.insideUnitCircle * walkZone;
        patrolTarget = patrolCenter + new Vector3(random.x, 0f, random.y);
        nextPatrolTime = Time.time + 8f;
    }

    private void LateUpdate()
    {
        if (camRig != null) camRig.position = transform.position;
    }

    private void OnDestroy()
    {
        if (materialInstance != null) Destroy(materialInstance);
    }
}
