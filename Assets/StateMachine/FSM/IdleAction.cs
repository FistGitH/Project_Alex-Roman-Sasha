public class IdleAction : FsmAction<Enemy>
{
	public override void Enter(Enemy actor)
	{
		actor.Movement.Stop();
		actor.UpdateIdleEndTime(actor.IdleDuration);
	}

	public override void Update(Enemy actor) { }
}
