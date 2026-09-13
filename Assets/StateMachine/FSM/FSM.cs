using System.Collections.Generic;

public abstract class FsmAction<TActor>
{
	public virtual void Enter(TActor actor) { }

	public abstract void Update(TActor actor);

	public virtual void Exit(TActor actor) { }
}

public abstract class FsmDecision<TActor>
{
	public abstract bool Decide(TActor actor);
}

public class FsmTransition<TActor>
{
	public FsmDecision<TActor> Decision;
	public FsmState<TActor> TargetState;

	public FsmTransition(FsmDecision<TActor> decision, FsmState<TActor> targetState)
	{
		Decision = decision;
		TargetState = targetState;
	}

	public bool CanTransition(TActor actor)
	{
		return Decision.Decide(actor);
	}
}

public class FsmState<TActor>
{
	private FsmAction<TActor> _action;
	private List<FsmTransition<TActor>> _transitions = new();

	public FsmState(FsmAction<TActor> action)
	{
		_action = action;
	}

	public void AddTransition(FsmTransition<TActor> transition)
	{
		_transitions.Add(transition);
	}

	public void Enter(TActor actor)
	{
		_action.Enter(actor);
	}

	public void Update(TActor actor, FsmStateMachine<TActor> stateMachina)
	{
		// Тут змінили порядок. Спершу оновлюємо поточний стан (Update), а вже потім перевіряємо переходи (Transitions).
		_action.Update(actor);

		foreach(var transition in _transitions)
		{
			if (!transition.CanTransition(actor))
			{
				continue;
			}

			stateMachina.ChangeState(transition.TargetState);
			return;
		}
	}

	public void Exit(TActor actor)
	{
		_action.Exit(actor);
	}
}

public class FsmStateMachine<TActor>
{
	public FsmState<TActor> CurrentState { get; private set; }

	private TActor _actor;

	public FsmStateMachine(TActor actor, FsmState<TActor> initialState)
	{
		_actor = actor;
		CurrentState = initialState;
		CurrentState?.Enter(_actor);
	}

	public void Update()
	{
		CurrentState?.Update(_actor, this);
	}

	public void ChangeState(FsmState<TActor> newState)
	{
		if(ReferenceEquals(CurrentState, newState))
		{
			return;
		}

		CurrentState?.Exit(_actor);
		newState?.Enter(_actor);
		CurrentState = newState;
	}
}
