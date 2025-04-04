using System;
using Godot;

public partial class MoveObject : CharacterBody2D
{
	public float MoveSpeed = 35;
	public float JumpSpeed = 140;
	public float HeightSpeed = 0;
	public float Height = 0;
	public const float Gravity = 320;
	public Vector2 Direction = Vector2.Zero;
	public int StateID = 0;
	public bool EnterEnd = false;
	public IState[] States;

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

	public partial class StateDefault : Node, IState
	{
		MoveObject character;
		public int GetId { get; } = 0;
		public StateDefault(MoveObject c)
		{
			character = c;
			character.States[GetId] = this;
		}
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
