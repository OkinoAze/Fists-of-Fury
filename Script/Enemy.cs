using Godot;
using System;
using System.Linq;
public partial class Enemy : Character
{
    Player _Player;
    Timer AttackTimer;
    EnemySlot Slot = null;
    public string[] AttackAnimationGroup = ["Punch", "Punch2"];

    enum State
    {
        Idle,
        Walk,
        Attack,
        Skill,
        Hurt,
        CrouchDown,
        KnockFly,
        KnockFall,
        KnockDown,
        MeleeWeaponAttack,
        RangedWeaponAttack,

    }
    public override void _Ready()
    {
        _Player = GetTree().GetNodesInGroup("Player")[0] as Player;
        AttackTimer = GetNode<Timer>("AttackTimer");

        AttackTimer.WaitTime = GD.RandRange(3f, 6f);
        States = new IState[Enum.GetNames(typeof(State)).Length];

        InvincibleStates = [(int)State.Hurt, (int)State.KnockDown, (int)State.KnockFly, (int)State.KnockFall, (int)State.CrouchDown];

        MaxHealth = 10;
        Health = 5;
        AccessingResources();


        _DamageEmitter.AreaEntered += OnDamageEmitter_AreaEntered;
        _DamageReceiver.DamageReceived += OnDamageReceiver_DamageReceived;

        _ = new StateIdle(this);
        _ = new StateWalk(this);
        _ = new StateHurt(this);
        _ = new StateAttack(this);
        _ = new StateKnockDown(this);
        _ = new StateKnockFly(this);
        _ = new StateKnockFall(this);
        _ = new StateCrouchDown(this);
        _ = new StateMeleeWeaponAttack(this);
        _ = new StateRangedWeaponAttack(this);

    }

    public override void _PhysicsProcess(double delta)
    {

        StateMachineUpdate(delta);
    }

    private void OnDamageEmitter_AreaEntered(Area2D area)
    {
        if (area is DamageReceiver a)
        {
            if (AttackRange((a.Owner as Node2D).Position))
            {
                Vector2 direction = (Vector2.Right * _DamageEmitter.Scale.X).Normalized();
                DamageReceiver.DamageReceivedEventArgs e = new(direction);
                a.DamageReceived(this, e);
            }
        }

    }
    private void OnDamageReceiver_DamageReceived(Node2D sender, DamageReceiver.DamageReceivedEventArgs e)
    {
        if (!InvincibleStates.Contains(StateID))
        {
            Direction = e.Direction;
            Repel = e.Repel;
            HeightSpeed = e.HeightSpeed;
            if (e.Type == DamageReceiver.HitType.Normal)
            {
                AnimationPlayer.Stop();
                SwitchState((int)State.Hurt);
            }
            else if (e.Type == DamageReceiver.HitType.knockDown)
            {
                AnimationPlayer.Stop();
                SwitchState((int)State.KnockFly);
            }
            Health = Mathf.Clamp(Health - e.Damage, 0, MaxHealth);
        }
        else
        {
            if (sender.Owner is Character c)
            {
                c.AttackBlocked(this, AttackBlockedStates.Invincible);
            }
        }
    }
    private partial class StateIdle : Node, IState
    {
        Enemy character;
        public int GetId { get; } = (int)State.Idle;
        public StateIdle(Enemy c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.Direction = Vector2.Zero;
            character.AnimationPlayer.Play("Idle");
            return true;
        }

        public int Update(double delta)
        {
            character.Slot ??= character._Player.ReserveSlot(character);
            if (character.Slot != null && character.Position.DistanceTo(character._Player.Position) > 20)
            {
                character.Direction = (character.Slot.GlobalPosition - character.GlobalPosition).Normalized();
            }
            return Exit();
        }
        public int Exit()
        {
            if (character.Position.DistanceTo(character._Player.Position) < 15 && character.AttackTimer.TimeLeft == 0 && character.AttackRange(character._Player.Position))
            {
                return (int)State.Attack;
            }
            if (character.Direction != Vector2.Zero)
            {
                return (int)State.Walk;
            }
            return GetId;
        }
    }
    private partial class StateWalk : Node, IState
    {
        Enemy character;
        public int GetId { get; } = (int)State.Walk;
        public StateWalk(Enemy c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.AnimationPlayer.Play("Walk");
            return true;
        }

        public int Update(double delta)
        {
            if (character.Slot != null)
            {
                character.Direction = (character.Slot.GlobalPosition - character.GlobalPosition).Normalized();


            }
            if (character.Direction.X < 0)
            {
                character.CharacterSprite.FlipH = true;
                character.WeaponSprite.FlipH = true;

                character._DamageEmitter.Scale = new Vector2(-1, 1);
            }
            else if (character.Direction.X > 0)
            {
                character.CharacterSprite.FlipH = false;
                character.WeaponSprite.FlipH = false;
                character._DamageEmitter.Scale = new Vector2(1, 1);
            }

            character.Velocity = character.Direction * character.MoveSpeed;
            character.MoveAndSlide();

            return Exit();
        }

        public int Exit()
        {
            if (character.Position.DistanceTo(character._Player.Position) < 15 && character.AttackTimer.TimeLeft == 0 && character.AttackRange(character._Player.Position))
            {
                return (int)State.Attack;
            }
            if (character.GlobalPosition.DistanceTo(character.Slot.GlobalPosition) < 2)
            {
                if (character.Position.X < character._Player.Position.X)
                {
                    if (character.CharacterSprite.FlipH == true)
                    {
                        character.CharacterSprite.FlipH = !character.CharacterSprite.FlipH;
                        character._DamageEmitter.Scale *= -1;
                    }
                }
                else if (character.Position.X > character._Player.Position.X)
                {
                    if (character.CharacterSprite.FlipH == false)
                    {
                        character.CharacterSprite.FlipH = !character.CharacterSprite.FlipH;
                        character._DamageEmitter.Scale *= -1;
                    }
                }
                return (int)State.Idle;
            }
            if (character.AttackRange(character._Player.Position))
            {
                // return (int)State.Attack;
            }

            return GetId;
        }
    }
    private partial class StateAttack : Node, IState
    {
        Enemy character;

        public int GetId { get; } = (int)State.Attack;
        public StateAttack(Enemy c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.AttackTimer.Start();

            if (character.AttackBufferTimer.TimeLeft > 0)
            {
                character.AttackID++;
                if (character.AttackID >= character.AttackAnimationGroup.Length)
                {
                    character.AttackID = 0;
                }
            }
            else
            {
                character.AttackID = 0;
            }
            character.AnimationPlayer.Play(character.AttackAnimationGroup[character.AttackID]);
            character.AttackBufferTimer.Start();

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

    public void HurtEnd()
    {
        if (Health <= 0 || Height > 0)
        {
            SwitchState((int)State.KnockFall);
        }
        else
        {
            SwitchState((int)State.Idle);
        }
    }
    private partial class StateHurt : Node, IState
    {
        Enemy character;
        public int GetId { get; } = (int)State.Hurt;
        public StateHurt(Enemy c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character._DamageEmitter.Monitoring = false;
            character.AnimationPlayer.Play("Hurt");
            character.PlayAudio("hit-1");
            character.Velocity = character.Direction * character.Repel;

            return true;
        }

        public int Update(double delta)
        {

            character.MoveAndSlide();
            return Exit();
        }
        public int Exit()
        {
            return GetId;
        }
    }

    private partial class StateCrouchDown : Node, IState
    {
        Enemy character;
        public int GetId { get; } = (int)State.CrouchDown;
        public StateCrouchDown(Enemy c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.AnimationPlayer.Play("CrouchDown");
            character.Velocity = Vector2.Zero;
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
    private partial class StateKnockFly : Node, IState
    {
        Enemy character;
        public int GetId { get; } = (int)State.KnockFly;
        public StateKnockFly(Enemy c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.AnimationPlayer.Play("KnockFly");
            character.PlayAudio("hit-2");
            character.Velocity = character.Direction * character.Repel;
            return true;
        }

        public int Update(double delta)
        {
            character.Velocity += character.Direction * character.Repel * (float)delta;
            character.MoveAndSlide();
            return Exit();
        }
        public int Exit()
        {
            if (character.IsOnWall())
            {
                character.Direction *= -1;
                return (int)State.KnockFall;
            }
            return GetId;
        }
    }
    private partial class StateKnockFall : Node, IState
    {
        Enemy character;
        public int GetId { get; } = (int)State.KnockFall;
        public StateKnockFall(Enemy c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.Velocity = character.Direction * character.Repel / 10;
            character.AnimationPlayer.Play("KnockFall");
            return true;
        }

        public int Update(double delta)
        {
            character.Height += character.HeightSpeed * (float)delta;
            character.HeightSpeed -= Gravity * (float)delta;
            character.CharacterSprite.Position = Vector2.Up * character.Height;


            character.MoveAndSlide();
            return Exit();
        }
        public int Exit()
        {
            if (character.Height <= 0)
            {
                character.CharacterSprite.Position = Vector2.Zero;
                character.Height = 0;
                return (int)State.KnockDown;
            }
            return GetId;
        }
    }

    public void KnockDownEnd()
    {
        if (Health <= 0)
        {
            _Player.FreeSlot(this);
            QueueFree();
        }
        else
        {
            SwitchState((int)State.CrouchDown);
        }
    }
    private partial class StateKnockDown : Node, IState
    {
        Enemy character;
        public int GetId { get; } = (int)State.KnockDown;
        public StateKnockDown(Enemy c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.AnimationPlayer.Play("KnockDown");
            character.Velocity = Vector2.Zero;
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
    private partial class StateMeleeWeaponAttack : Node, IState
    {
        Enemy character;
        public int GetId { get; } = (int)State.MeleeWeaponAttack;
        public StateMeleeWeaponAttack(Enemy c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.Direction = Vector2.Zero;
            character.AnimationPlayer.Play("KnifeAttack");
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
    private partial class StateRangedWeaponAttack : Node, IState
    {
        Enemy character;
        public int GetId { get; } = (int)State.RangedWeaponAttack;
        public StateRangedWeaponAttack(Enemy c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.Direction = Vector2.Zero;
            character.AnimationPlayer.Play("GunShot");
            character.PlayAudio("gunshot");
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
