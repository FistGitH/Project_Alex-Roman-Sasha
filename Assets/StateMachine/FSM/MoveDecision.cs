public class MoveDecision : FsmDecision<Enemy>
{
	public override bool Decide(Enemy actor)
	{
		return actor.IsIdle == false;
	}
}
