using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public sealed class EnemyMovement : MonoBehaviour
{
	private NavMeshAgent _agent;

	public bool IsDestinationReached => _agent.isOnNavMesh && !_agent.pathPending && _agent.hasPath && _agent.remainingDistance <= _agent.stoppingDistance + 0.05f;

	private void Awake()
	{
		_agent = GetComponent<NavMeshAgent>();
	}

	public void MoveTo(Vector3 destination, float speed)
	{
		if (!_agent.isOnNavMesh)
			return;

		_agent.speed = speed;
		_agent.isStopped = false;
		_agent.SetDestination(destination);
	}

	public void Stop()
	{
		_agent.isStopped = true;
	}
}
