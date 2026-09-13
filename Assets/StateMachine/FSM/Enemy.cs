using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyPatrol))]
public class Enemy : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField] private float _speed = 3.5f;
	[SerializeField] private float _chaseSpeed = 6f;

	[Header("Patrol stop")]
	[SerializeField] private float _idleDuration = 2f;
	[SerializeField, Range(0f, 1f)] private float _idleProbability = 0.35f;

	[Header("Player detection")]
	[SerializeField] private Transform _player;
	[SerializeField] private string _playerTag = "Player";
	[SerializeField] private float _viewDistance = 12f;
	[SerializeField, Range(1f, 360f)] private float _viewAngle = 100f;
	[SerializeField] private float _closeDetectionRadius = 2f;
	[SerializeField] private float _eyeHeight = 1.4f;
	[SerializeField] private LayerMask _visionMask = ~0;
	[SerializeField] private float _lostSightDelay = 0.2f;

	public float Speed => _speed;
	public float ChaseSpeed => _chaseSpeed;
	public float IdleDuration => _idleDuration;
	public float IdleProbability => _idleProbability;

	public EnemyMovement Movement => _movement;
	public EnemyPatrol Patrol => _patrol;
	public bool IsIdle => Time.time < _idleEndTime;
	public bool ReachedPatrolPointThisFrame { get; set; }
	public bool CanSeePlayer { get; private set; }
	public bool HasLostPlayer => Time.time > _lastTimePlayerSeen + _lostSightDelay;
	public Transform Player => _player;

	private EnemyMovement _movement;
	private EnemyPatrol _patrol;
	private FsmStateMachine<Enemy> _stateMachine;

	private float _idleEndTime;
	private float _lastTimePlayerSeen = float.NegativeInfinity;
	private float _triggerDetectedUntil;

	private void Awake()
	{
		_movement = GetComponent<EnemyMovement>();
		_patrol = GetComponent<EnemyPatrol>();
	}

	private void Start()
	{
		FindPlayerIfNeeded();

		FsmState<Enemy> idleState = new FsmState<Enemy>(new IdleAction());
		FsmState<Enemy> patrolState = new FsmState<Enemy>(new PatrolAction());
		FsmState<Enemy> chaseState = new FsmState<Enemy>(new ChaseAction());

		patrolState.AddTransition(new FsmTransition<Enemy>(new SeePlayerDecision(), chaseState));
		patrolState.AddTransition(new FsmTransition<Enemy>(new IdleDecision(), idleState));
		idleState.AddTransition(new FsmTransition<Enemy>(new SeePlayerDecision(), chaseState));
		idleState.AddTransition(new FsmTransition<Enemy>(new MoveDecision(), patrolState));
		chaseState.AddTransition(new FsmTransition<Enemy>(new LostPlayerDecision(), patrolState));

		_stateMachine = new FsmStateMachine<Enemy>(this, patrolState);
	}

	private void Update()
	{
		UpdatePlayerVisibility();
		_stateMachine.Update();
	}

	public void UpdateIdleEndTime(float idleDuration)
	{
		_idleEndTime = Time.time + idleDuration;
	}

	private void UpdatePlayerVisibility()
	{
		FindPlayerIfNeeded();
		CanSeePlayer = false;
		if (_player == null)
			return;

		Vector3 targetPosition = _player.position;
		Vector3 flatDirection = targetPosition - transform.position;
		flatDirection.y = 0f;
		float distance = flatDirection.magnitude;

		if (Time.time < _triggerDetectedUntil || distance <= _closeDetectionRadius)
		{
			CanSeePlayer = true;
		}
		else if (distance <= _viewDistance && Vector3.Angle(transform.forward, flatDirection) <= _viewAngle * 0.5f)
		{
			Vector3 origin = transform.position + Vector3.up * _eyeHeight;
			Vector3 target = targetPosition + Vector3.up;
			Vector3 ray = target - origin;
			if (!Physics.Raycast(origin, ray.normalized, out RaycastHit hit, ray.magnitude, _visionMask, QueryTriggerInteraction.Ignore)
				|| hit.transform == _player || hit.transform.IsChildOf(_player))
			{
				CanSeePlayer = true;
			}
		}

		if (CanSeePlayer)
			_lastTimePlayerSeen = Time.time;
	}

	private void FindPlayerIfNeeded()
	{
		if (_player != null)
			return;

		GameObject playerObject = GameObject.FindGameObjectWithTag(_playerTag);
		if (playerObject != null)
			_player = playerObject.transform;
	}

	private void OnTriggerStay(Collider other)
	{
		Transform candidate = other.transform.root;
		if (other.CompareTag(_playerTag) || candidate.CompareTag(_playerTag))
		{
			_player = candidate;
			_triggerDetectedUntil = Time.time + 0.15f;
		}
	}
}

public class SeePlayerDecision : FsmDecision<Enemy>
{
	public override bool Decide(Enemy actor) => actor.CanSeePlayer;
}

public class LostPlayerDecision : FsmDecision<Enemy>
{
	public override bool Decide(Enemy actor) => actor.HasLostPlayer;
}

public class ChaseAction : FsmAction<Enemy>
{
	public override void Update(Enemy actor)
	{
		if (actor.Player != null)
			actor.Movement.MoveTo(actor.Player.position, actor.ChaseSpeed);
	}

	public override void Exit(Enemy actor)
	{
		actor.Movement.Stop();
	}
}
