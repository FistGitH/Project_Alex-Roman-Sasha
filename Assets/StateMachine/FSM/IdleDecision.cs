using UnityEngine;

public class IdleDecision : FsmDecision<Enemy>
{
	public override bool Decide(Enemy actor)
	{
		return actor.ReachedPatrolPointThisFrame && Random.value < actor.IdleProbability;
	}
}
