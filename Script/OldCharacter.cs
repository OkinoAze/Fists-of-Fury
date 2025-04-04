using System;
using Godot;

public partial class OldCharacter : CharacterBody2D
{
	public delegate void AttackBlockedReceived(Character character, AttackBlockedStates blockedState);
	public delegate void PickUpPropReceived(Prop prop);
	public AttackBlockedReceived AttackBlocked;
	public PickUpPropReceived PickUpProp;
	public enum AttackBlockedStates
	{
		Defense,
		Invincible,
		Miss,
	}
	[Export]
	public int MaxHealth = 1;
	[Export]
	public int Health = 1;
	[Export]
	public int Damage = 1;
	[Export]
	public float MoveSpeed = 35;
	[Export]
	public float JumpSpeed = 140;
	public float HeightSpeed = 0;
	public float Height = 0;
	public const float Gravity = 320;
	public const float _AttackRange = 5;
	public float Repel = 0;
	public Vector2 Direction = Vector2.Zero;
	public int AttackID = 0;
	public Sprite2D CharacterSprite;
	public Sprite2D WeaponSprite;
	public Sprite2D ShadowSprite;
	public AnimationPlayer AnimationPlayer;
	public Area2D _DamageEmitter;
	public DamageReceiver _DamageReceiver;

	public AudioStreamPlayer AudioPlayer;
	public Timer AttackBufferTimer;
	public int StateID = 0;
	public bool EnterEnd = false;
	public IState[] States = new IState[1];
	public Prop Weapon;
	public int[] InvincibleStates;
	public string AvatarName = null;

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
		ShadowSprite = GetNode<Sprite2D>("ShadowSprite");
		WeaponSprite = GetNode<Sprite2D>("WeaponSprite");
		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_DamageEmitter = CharacterSprite.GetNode<Area2D>("DamageEmitter");
		_DamageReceiver = CharacterSprite.GetNode<DamageReceiver>("DamageReceiver");
		AudioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		AttackBufferTimer = GetNode<Timer>("AttackBufferTimer");

	}
	public bool AttackRange(Vector2 position)
	{
		if (position.Y > Position.Y - _AttackRange && position.Y < Position.Y + _AttackRange)
		{
			return true;
		}
		return false;
	}

	public string GetAvatarPath(string name)
	{
		return "res://Art/UI/Avatar/avatar_" + name + ".png";
	}

	public void PlayAudio(string name)
	{
		AudioPlayer.Stream = ResourceLoader.Load<AudioStreamWav>("res://Music/SFX/" + name + ".wav");
		AudioPlayer.Play();
	}

	public void StateMachineUpdate(double delta)
	{
		if (Health >= MaxHealth)
		{
			Health = MaxHealth;
		}
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
		OldCharacter character;
		public int GetId { get; } = 0;
		public StateDefault(OldCharacter c)
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
