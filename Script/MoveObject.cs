using System;
using Godot;

public partial class MoveObject : CharacterBody2D
{
	[Export]
	public float MoveSpeed = 35;
	[Export]
	public float JumpSpeed = 100;
	public float HeightSpeed = 0;
	public float Height = 0;
	public const float Gravity = 320;
	public Vector2 Direction = Vector2.Zero;
	public int StateID = 0;
	protected bool EnterEnd = false;
	protected IState[] States;

	public void StateMachineUpdate(double delta)
	{
		if (EnterEnd == false)
		{
			EnterEnd = States[StateID].Enter();
		}
		var id = States[StateID].Update(delta);
		if (id != StateID)
		{
			EnterEnd = false;
			StateID = id;
		}
	}
	public void SwitchState(int id, bool enter = false)
	{
		EnterEnd = enter;
		StateID = id;
	}

	public interface IState
	{
		public int GetId { get; }

		public bool Enter()
		{
			return true;
		}
		public int Update(double delta)
		{
			return Exit();
		}
		public int Exit()
		{
			return GetId;
		}

	}



}
