using Godot;
using System;
using System.Linq;
public partial class Enemy : Character
{
    Player _Player;
    Timer AttackTimer;
    EnemySolt Solt = null;
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
                AttackID++;
                if (AttackID >= AttackAnimationGroup.Length)
                {
                    AttackID = 0;
                }
                Vector2 direction = (Vector2.Right * _DamageEmitter.Scale.X).Normalized();
                DamageReceiver.DamageReceivedEventArgs e = new(direction);
                a.DamageReceived(this, e);
            }
        }

    }
    private void OnDamageReceiver_DamageReceived(object sender, DamageReceiver.DamageReceivedEventArgs e)
    {
        if (!InvincibleStates.Contains(StateID))
        {
            Direction = e.Direction;
            Repel = e.Repel;
            HeightSpeed = e.HeightSpeed;
            if (e.Type == DamageReceiver.HitType.Normal)
            {
                SwitchState((int)State.Hurt);
            }
            else if (e.Type == DamageReceiver.HitType.knockDown)
            {
                SwitchState((int)State.KnockFly);
            }
            Health = Mathf.Clamp(Health - e.Damage, 0, MaxHealth);
        }
    }
    private partial class StateIdle : Node, IState
    {
        Enemy Character;
        public int GetId { get; } = (int)State.Idle;
        public StateIdle(Enemy character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.Direction = Vector2.Zero;
            Character.AnimationPlayer.Play("Idle");
            return true;
        }

        public int Update(double delta)
        {
            Character.Solt ??= Character._Player.ReserveSlot(Character);
            if (Character.Solt != null && Character.Position.DistanceTo(Character._Player.Position) > 20)
            {
                Character.Direction = (Character.Solt.GlobalPosition - Character.GlobalPosition).Normalized();
            }
            return Exit();
        }
        public int Exit()
        {
            if (Character.Position.DistanceTo(Character._Player.Position) < 15 && Character.AttackTimer.TimeLeft == 0 && Character.AttackRange(Character._Player.Position))
            {
                return (int)State.Attack;
            }
            if (Character.Direction != Vector2.Zero)
            {
                return (int)State.Walk;
            }
            return GetId;
        }
    }
    private partial class StateWalk : Node, IState
    {
        Enemy Character;
        public int GetId { get; } = (int)State.Walk;
        public StateWalk(Enemy character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.AnimationPlayer.Play("Walk");
            return true;
        }

        public int Update(double delta)
        {
            if (Character.Solt != null)
            {
                Character.Direction = (Character.Solt.GlobalPosition - Character.GlobalPosition).Normalized();


            }
            if (Character.Direction.X < 0)
            {
                Character.CharacterSprite.FlipH = true;
                Character.WeaponSprite.FlipH = true;

                Character._DamageEmitter.Scale = new Vector2(-1, 1);
            }
            else if (Character.Direction.X > 0)
            {
                Character.CharacterSprite.FlipH = false;
                Character.WeaponSprite.FlipH = false;
                Character._DamageEmitter.Scale = new Vector2(1, 1);
            }

            Character.Velocity = Character.Direction * Character.MoveSpeed;
            Character.MoveAndSlide();

            return Exit();
        }

        public int Exit()
        {
            if (Character.Position.DistanceTo(Character._Player.Position) < 15 && Character.AttackTimer.TimeLeft == 0 && Character.AttackRange(Character._Player.Position))
            {
                return (int)State.Attack;
            }
            if (Character.GlobalPosition.DistanceTo(Character.Solt.GlobalPosition) < 2)
            {
                if (Character.Position.X < Character._Player.Position.X)
                {
                    if (Character.CharacterSprite.FlipH == true)
                    {
                        Character.CharacterSprite.FlipH = !Character.CharacterSprite.FlipH;
                        Character._DamageEmitter.Scale *= -1;
                    }
                }
                else if (Character.Position.X > Character._Player.Position.X)
                {
                    if (Character.CharacterSprite.FlipH == false)
                    {
                        Character.CharacterSprite.FlipH = !Character.CharacterSprite.FlipH;
                        Character._DamageEmitter.Scale *= -1;
                    }
                }
                return (int)State.Idle;
            }
            if (Character.AttackRange(Character._Player.Position))
            {
                // return (int)State.Attack;
            }

            return GetId;
        }
    }
    private partial class StateAttack : Node, IState
    {
        Enemy Character;
        int id;

        public int GetId { get; } = (int)State.Attack;
        public StateAttack(Enemy character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.AttackTimer.Start();
            if (Character.AttackID == id)
            {
                id = 0;
                Character.AttackID = id;
            }
            else
            {
                id = Character.AttackID;
            }
            Character.AnimationPlayer.Play(Character.AttackAnimationGroup[id]);
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
        Enemy Character;
        public int GetId { get; } = (int)State.Hurt;
        public StateHurt(Enemy character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character._DamageEmitter.Monitoring = false;
            Character.AttackID = 0;
            Character.AnimationPlayer.Play("Hurt");
            Character.Velocity = Character.Direction * Character.Repel;

            return true;
        }

        public int Update(double delta)
        {

            Character.MoveAndSlide();
            return Exit();
        }
        public int Exit()
        {
            return GetId;
        }
    }

    private partial class StateCrouchDown : Node, IState
    {
        Enemy Character;
        public int GetId { get; } = (int)State.CrouchDown;
        public StateCrouchDown(Enemy character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.AnimationPlayer.Play("CrouchDown");
            Character.Velocity = Vector2.Zero;
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
        Enemy Character;
        public int GetId { get; } = (int)State.KnockFly;
        public StateKnockFly(Enemy character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.AnimationPlayer.Play("KnockFly");
            Character.Velocity = Character.Direction * Character.Repel;
            return true;
        }

        public int Update(double delta)
        {
            Character.Velocity += Character.Direction * Character.Repel * (float)delta;
            Character.MoveAndSlide();
            return Exit();
        }
        public int Exit()
        {
            if (Character.IsOnWall())
            {
                Character.Direction *= -1;
                return (int)State.KnockFall;
            }
            return GetId;
        }
    }
    private partial class StateKnockFall : Node, IState
    {
        Enemy Character;
        public int GetId { get; } = (int)State.KnockFall;
        public StateKnockFall(Enemy character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.Velocity = Character.Direction * Character.Repel / 10;
            Character.AnimationPlayer.Play("KnockFall");
            return true;
        }

        public int Update(double delta)
        {
            Character.Height += Character.HeightSpeed * (float)delta;
            Character.HeightSpeed -= Character.Gravity * (float)delta;
            Character.CharacterSprite.Position = Vector2.Up * Character.Height;


            Character.MoveAndSlide();
            return Exit();
        }
        public int Exit()
        {
            if (Character.Height <= 0)
            {
                Character.CharacterSprite.Position = Vector2.Zero;
                Character.Height = 0;
                return (int)State.KnockDown;
            }
            return GetId;
        }
    }

    public void KnockDownEnd()
    {
        if (Health <= 0)
        {
            _Player.FreeSolt(this);
            QueueFree();
        }
        else
        {
            SwitchState((int)State.CrouchDown);
        }
    }
    private partial class StateKnockDown : Node, IState
    {
        Enemy Character;
        public int GetId { get; } = (int)State.KnockDown;
        public StateKnockDown(Enemy character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.AnimationPlayer.Play("KnockDown");
            Character.Velocity = Vector2.Zero;
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
