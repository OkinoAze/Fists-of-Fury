using System;
using Godot;

public partial class Character : MoveObject
{

	public delegate void PickUpPropReceiver(Prop prop);
	public delegate void DropWeaponReceiver();

	public PickUpPropReceiver PickUpProp;
	public DropWeaponReceiver DropWeapon;

	[Export]
	public new float MoveSpeed = 35;
	[Export]
	public new float JumpSpeed = 140;
	[Export]
	public int MaxHealth = 1;
	[Export]
	public int Health = 1;
	[Export]
	public int Damage = 1;
	public const float _AttackRange = 5;
	public float Repel = 0;
	public int AttackID = 0;
	public Sprite2D CharacterSprite;
	public Sprite2D WeaponSprite;
	public Sprite2D ShadowSprite;
	public AnimatedSprite2D Particle;

	public AnimationPlayer AnimationPlayer;
	public DamageEmitter _DamageEmitter;
	public DamageReceiver _DamageReceiver;

	public AudioStreamPlayer AudioPlayer;
	public Timer AttackBufferTimer;
	public Prop Weapon;
	public Area2D PickUpCheck;
	public int[] InvincibleStates;
	public string AvatarName = null;

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
		AudioPlayer.Stream = ResourceLoader.Load<AudioStreamWav>("res://Music/SFX/" + name + ".wav", null, ResourceLoader.CacheMode.Reuse);
		AudioPlayer.Play();
	}

}
