using System;
using Godot;

public partial class Character : MoveObject
{

	[Export]
	public int MaxHealth = 1;
	[Export]
	public int Health = 1;
	[Export]
	public int Damage = 1;
	[Export]
	public EntityManager.WeaponType HasWeapon = EntityManager.WeaponType.Default;
	protected const float _AttackRange = 5;
	protected float Repel = 0;
	public int AttackID { get; protected set; } = 0;
	public Sprite2D CharacterSprite;
	protected Sprite2D WeaponSprite;
	protected Sprite2D ShadowSprite;
	protected AnimationPlayer AnimationPlayer;
	public DamageEmitter _DamageEmitter;
	public DamageReceiver _DamageReceiver;

	protected AudioStreamPlayer AudioPlayer;
	protected Timer AttackBufferTimer;
	protected Prop Weapon;
	protected Prop CanPickUpProp;

	protected Area2D PickUpCheck;
	protected int[] InvincibleStates;
	[Export]
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

		PickUpCheck.AreaEntered += OnPickUpCheck_AreaEntered;
		PickUpCheck.AreaExited += OnPickUpCheck_AreaExited;

		if (HasWeapon != EntityManager.WeaponType.Default)
		{
			GetWeapon(HasWeapon);
		}

	}
	protected void GetWeapon(EntityManager.WeaponType weaponType)
	{
		var path = "res://Scene/Prefab/" + Enum.GetName(weaponType) + ".tscn";
		bool exists = ResourceLoader.Exists(path);
		if (exists)
		{
			var weapon = ResourceLoader.Load<PackedScene>(path).Instantiate();
			PickUpProp(weapon as Prop);
		}
	}
	protected void OnPickUpCheck_AreaExited(Area2D area)
	{
		CanPickUpProp = null;
	}

	protected void OnPickUpCheck_AreaEntered(Area2D area)
	{
		if (area.Owner is Prop p)
		{
			CanPickUpProp = p;
		}
	}
	public Rect2 GetCrimeaRect()
	{
		var viewportRect = GetViewportRect();
		var position = GetViewport().GetCamera2D().GlobalPosition;
		var size = viewportRect.Size / 2;
		return new Rect2(position.X - size.X, position.Y - size.Y, viewportRect.Size);
	}
	public void FaceToDirection()
	{
		if (Direction.X < 0)
		{
			CharacterSprite.FlipH = true;
			WeaponSprite.FlipH = true;
			_DamageEmitter.Scale = new Vector2(-1, 1);
		}
		else if (Direction.X > 0)
		{
			CharacterSprite.FlipH = false;
			WeaponSprite.FlipH = false;
			_DamageEmitter.Scale = new Vector2(1, 1);
		}

	}
	public void FaceToPosition(Vector2 position)
	{
		if (Position.X < position.X)
		{
			if (CharacterSprite.FlipH == true)
			{
				CharacterSprite.FlipH = !CharacterSprite.FlipH;
				WeaponSprite.FlipH = CharacterSprite.FlipH;
				_DamageEmitter.Scale = new Vector2(1, 1);

			}
		}
		else if (Position.X > position.X)
		{
			if (CharacterSprite.FlipH == false)
			{
				CharacterSprite.FlipH = !CharacterSprite.FlipH;
				WeaponSprite.FlipH = CharacterSprite.FlipH;
				_DamageEmitter.Scale = new Vector2(-1, 1);

			}
		}
	}
	public void PickUpProp(Prop prop)
	{
		if (prop.RestoreHealth > 0)
		{
			PlayAudio("eat-food");
			Health = Mathf.Clamp(Health + prop.RestoreHealth, 0, MaxHealth);
		}
		if (prop.Property == Prop.Properties.MeleeWeapon || prop.Property == Prop.Properties.RangedWeapon)
		{
			Weapon = prop;
			if (prop is PropKnife)
			{
				WeaponSprite.Texture = ResourceLoader.Load<Texture2D>("res://Art/Characters/player_knife.png");
			}
			else if (prop is PropGun)
			{
				WeaponSprite.Texture = ResourceLoader.Load<Texture2D>("res://Art/Characters/player_gun.png");
			}
			WeaponSprite.Visible = true;
		}
		prop.QueueFree();
	}

	public void DropWeapon()
	{
		if (Weapon != null)
		{
			WeaponSprite.Texture = null;
			WeaponSprite.Visible = false;
			EntityManager.Instance.GenerateProp(Weapon, Position);
			Weapon = null;
		}
	}
	public bool AttackRange(Vector2 position)
	{
		if (position.Y > Position.Y - _AttackRange && position.Y < Position.Y + _AttackRange)
		{
			return true;
		}
		return false;
	}
	public void PlayAudio(string name)
	{
		AudioPlayer.Stream = ResourceLoader.Load<AudioStreamWav>("res://Music/SFX/" + name + ".wav", null, ResourceLoader.CacheMode.Reuse);
		AudioPlayer.Play();
	}

}
