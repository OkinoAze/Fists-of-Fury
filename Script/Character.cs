using Godot;
using System;
using System.Dynamic;

public partial class Character : CharacterBody2D
{
	[Export]
	public int Health = 1;
	[Export]
	public int Damage = 1;
	[Export]
	public float Speed = 35;

	public Vector2 Direction = Vector2.Zero;

	public Sprite2D CharacterSprite;
	public Sprite2D Shadow;
	public AnimationPlayer AnimationPlayer;

	public int StateID = 0;
	public bool EnterEnd = false;
	public IState[] States = new IState[1];

	public override void _Ready()
	{
		AccessingResources();
		_ = new StateDefault(this);
	}

	public override void _PhysicsProcess(double delta)
	{
		Direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		StateMachineUpdate(delta);
	}

	public void AccessingResources()
	{
		CharacterSprite = GetNode<Sprite2D>("CharacterSprite");
		Shadow = GetNode<Sprite2D>("Shadow");
		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}
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
		protected Character Character;
		public int GetId { get; } = 0;
		public StateDefault(Character character)
		{
			Character = character;
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
