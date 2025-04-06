using System;
using Godot;

public partial class Character : MoveObject
{

	public delegate void PickUpPropReceiver(Prop prop);
	public delegate void DropWeaponReceiver();

	public PickUpPropReceiver PickUpProp;
	public DropWeaponReceiver DropWeapon;

	public int MaxHealth = 1;
	public int Health = 1;
	public int Damage = 1;
	protected const float _AttackRange = 5;
	protected float Repel = 0;
	public int AttackID { get; protected set; } = 0;
	protected Sprite2D CharacterSprite;
	protected Sprite2D WeaponSprite;
	protected Sprite2D ShadowSprite;
	protected AnimatedSprite2D Particle;

	protected AnimationPlayer AnimationPlayer;
	public DamageEmitter _DamageEmitter;
	public DamageReceiver _DamageReceiver;

	protected AudioStreamPlayer AudioPlayer;
	protected Timer AttackBufferTimer;
	protected Prop Weapon;
	protected PropInstance CanPickUpProp;

	protected Area2D PickUpCheck;
	protected int[] InvincibleStates;
	public string AvatarName { get; protected set; }

	public void AccessingResources()
	{
		CharacterSprite = GetNode<Sprite2D>("CharacterSprite");
		ShadowSprite = GetNode<Sprite2D>("ShadowSprite");
		WeaponSprite = CharacterSprite.GetNode<Sprite2D>("WeaponSprite");
		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_DamageEmitter = CharacterSprite.GetNode<DamageEmitter>("DamageEmitter");
		_DamageReceiver = CharacterSprite.GetNode<DamageReceiver>("DamageReceiver");
		AudioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		AttackBufferTimer = GetNode<Timer>("AttackBufferTimer");
		PickUpCheck = GetNode<Area2D>("PickUpCheck");
		Particle = GetNode<AnimatedSprite2D>("Particle");

		PickUpCheck.AreaEntered += OnPickUpCheck_AreaEntered;
		PickUpCheck.AreaExited += OnPickUpCheck_AreaExited;

	}
	protected void OnPickUpCheck_AreaExited(Area2D area)
	{
		CanPickUpProp = null;
	}

	protected void OnPickUpCheck_AreaEntered(Area2D area)
	{
		if (area.Owner is PropInstance p)
		{
			CanPickUpProp = p;
		}
	}

	protected bool AttackRange(Vector2 position)
	{
		if (position.Y > Position.Y - _AttackRange && position.Y < Position.Y + _AttackRange)
		{
			return true;
		}
		return false;
	}
	protected string GetAvatarPath(string name)
	{
		return "res://Art/UI/Avatar/avatar_" + name + ".png";
	}
	protected void PlayAudio(string name)
	{
		AudioPlayer.Stream = ResourceLoader.Load<AudioStreamWav>("res://Music/SFX/" + name + ".wav", null, ResourceLoader.CacheMode.Reuse);
		AudioPlayer.Play();
	}

}
