public class PatrolAction : FsmAction<Enemy> 
{
	public override void Enter(Enemy actor)
	{
		if (actor.Patrol.HasPoints)
			actor.Movement.MoveTo(actor.Patrol.CurrentPoint, actor.Speed);

		// animation
	}

	public override void Update(Enemy actor)
	{
		// Скидаємо результат попереднього кадру
		actor.ReachedPatrolPointThisFrame = false;

		if (actor.Patrol.HasPoints && actor.Movement.IsDestinationReached) 
		{
			// Якщо цього кадру юніт дійшов до patrol-точки, позначимо це як true, щоб IdleDecision перевірявся лише один раз
			actor.ReachedPatrolPointThisFrame = true; 

			actor.Patrol.MoveToNextPoint();
			actor.Movement.MoveTo(actor.Patrol.CurrentPoint, actor.Speed);
		}
	}

	public override void Exit(Enemy actor)
	{
		actor.Movement.Stop();
		// cancel animation
	}
}
