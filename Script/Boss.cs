using Godot;
using System;
using System.Linq;

public partial class Boss : Character
{
    Player _Player;
    Timer WaitTimer;
    Timer AttackWaitTimer;
    Timer MoveTimer;
    public Vector2 MovePoint = Vector2.Zero; //使用全局坐标
    public string[] AttackAnimationGroup = ["Punch", "Punch2"];
    public enum State
    {
        Idle,
        Walk,
        Attack,
        Hurt,
        CrouchDown,
        KnockFly,
        KnockFall,
        KnockDown,
        Death,
        Patrol,
        MoveToEdge,
        Regress,
        EnterScene,
        EdgeLock,
        NearLock,
        Kick,
        KickEnd,
        Defense,
        DefenseRegress,

    }
    public override void _Ready()
    {
        States = new IState[Enum.GetNames(typeof(State)).Length];
        InvincibleStates = [(int)State.DefenseRegress, (int)State.EnterScene, (int)State.KnockDown, (int)State.KnockFly, (int)State.KnockFall, (int)State.CrouchDown, (int)State.Death, (int)State.Defense];

        _ = new StateIdle(this);
        _ = new StateWalk(this);
        _ = new StateHurt(this);
        _ = new StateAttack(this);
        _ = new StateKnockDown(this);
        _ = new StateKnockFly(this);
        _ = new StateKnockFall(this);
        _ = new StateCrouchDown(this);
        _ = new StateDeath(this);
        _ = new StatePatrol(this);
        _ = new StateMoveToEdge(this);
        _ = new StateRegress(this);
        _ = new StateEnterScene(this);
        _ = new StateEdgeLock(this);
        _ = new StateNearLock(this);
        _ = new StateKick(this);
        _ = new StateKickEnd(this);
        _ = new StateDefense(this);
        _ = new StateDefenseRegress(this);


        _Player = GetTree().GetNodesInGroup("Player")[0] as Player;
        WaitTimer = GetNode<Timer>("WaitTimer");
        AttackWaitTimer = GetNode<Timer>("AttackWaitTimer");

        AccessingResources();

        _DamageEmitter.AreaEntered += OnDamageEmitter_AreaEntered;
        _DamageEmitter.AttackSuccess += OnDamageEmitter_AttackSuccess;
        _DamageReceiver.DamageReceived += OnDamageReceiver_DamageReceived;
    }

    public override void _PhysicsProcess(double delta)
    {
        StateMachineUpdate(delta);
        if (!InvincibleStates.Contains(StateID))
        {
            _DamageReceiver.Monitorable = true;
        }
        else
        {
            _DamageReceiver.Monitorable = false;
        }
        //GD.Print((State)StateID);
    }
    public bool CanAttackPlayer(float distance = 20)
    {
        if (Position.DistanceTo(_Player.Position) < distance && AttackWaitTimer.TimeLeft <= 0 && AttackRange(_Player.Position))
        {
            return true;
        }
        return false;
    }


    private void OnDamageEmitter_AttackSuccess(Character character)
    {


    }

    private void OnDamageEmitter_AreaEntered(Area2D area)
    {
        if (area is DamageReceiver a)
        {
            if (AttackRange((a.Owner as Node2D).Position))
            {
                DamageReceiver.DamageReceivedEventArgs e;
                Vector2 direction = (Vector2.Right * _DamageEmitter.Scale.X).Normalized();

                if (StateID == (int)State.Kick)
                {
                    e = new(_DamageEmitter.GetNode<CollisionShape2D>("CollisionShape2D").GlobalPosition, direction, 3, 100, 100, DamageReceiver.HitType.knockDown);

                }
                else
                {
                    e = new(_DamageEmitter.GetNode<CollisionShape2D>("CollisionShape2D").GlobalPosition, direction, 2);
                }

                a.DamageReceived(_DamageEmitter, e);
            }
        }

    }
    private void OnDamageReceiver_DamageReceived(DamageEmitter emitter, DamageReceiver.DamageReceivedEventArgs e)
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
                FaceToPosition((emitter.Owner as Node2D).Position);
                SwitchState((int)State.KnockFly);
            }

            Health = Mathf.Clamp(Health - e.Damage, 0, MaxHealth);
            emitter?.AttackSuccess(this);

            if (Health <= 0 || e.Type == DamageReceiver.HitType.knockDown)
            {
                EntityManager.Instance.ShackCamera();
                EntityManager.Instance.GenerateParticle(e.Position, e.Direction.X < 0);
            }
        }

    }
    partial class StateIdle : Node, IState
    {
        Rect2 rect;
        Boss character;
        public int GetId { get; } = (int)State.Idle;
        public StateIdle(Boss c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.WaitTimer.Start();
            rect = character.GetCrimeaRect().GrowSide(Side.Left, -10).GrowSide(Side.Right, -10);
            character.Direction = Vector2.Zero;
            character.MovePoint = Vector2.Zero;
            character.AnimationPlayer.Play("Idle");
            return true;
        }

        public int Update(double delta)
        {

            return Exit();
        }
        public int Exit()
        {
            if (character.CanAttackPlayer() && character.AttackWaitTimer.TimeLeft <= 0)
            {
                return (int)State.NearLock;
            }

            if (character.IsOnWall() || character.WaitTimer.TimeLeft <= 0)
            {
                return (int)State.Patrol;
            }

            if (rect.HasPoint(character.GlobalPosition))
            {
                return (int)State.MoveToEdge;
            }

            return GetId;
        }
    }
    partial class StateWalk : Node, IState
    {
        Boss character;
        public int GetId { get; } = (int)State.Walk;
        public StateWalk(Boss c)
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
            if (character.IsOnWall())
            {
                character.MovePoint = character.MovePoint.Rotated(Mathf.Pi / 8);
                character.Direction = character.MovePoint.Normalized();
                character.WaitTimer.Start();
            }

            character.FaceToDirection();

            character.Velocity = character.Direction * character.MoveSpeed;
            character.MoveAndSlide();

            return Exit();
        }

        public int Exit()
        {
            if (character.CanAttackPlayer() && character.AttackWaitTimer.TimeLeft <= 0)
            {
                character.FaceToPosition(character._Player.Position);
                return (int)State.NearLock;
            }

            if (!character.IsOnWall() && character.WaitTimer.TimeLeft <= 0)
            {
                return (int)State.Idle;
            }
            return GetId;
        }
    }
    public void AttackEnd()
    {
        if (Health <= MaxHealth / 2)
        {
            SwitchState((int)State.DefenseRegress);
        }
        else
        {

            SwitchState((int)State.Regress);
        }
    }
    partial class StateAttack : Node, IState
    {
        Boss character;

        public int GetId { get; } = (int)State.Attack;
        public StateAttack(Boss c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.AttackWaitTimer.Start();
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
            SwitchState((int)State.Defense);
        }
    }
    partial class StateHurt : Node, IState
    {
        Boss character;
        public int GetId { get; } = (int)State.Hurt;
        public StateHurt(Boss c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character._DamageEmitter.Monitoring = false;
            character.DropWeapon();
            character.AnimationPlayer.Play("Hurt");
            if (character.Health <= 0)
            {
                character.PlayAudio("hit-2");
            }
            else
            {
                character.PlayAudio("hit-1");
            }
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
    partial class StateCrouchDown : Node, IState
    {
        Boss character;
        public int GetId { get; } = (int)State.CrouchDown;
        public StateCrouchDown(Boss c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.AnimationPlayer.Play("CrouchDown");
            character.Direction = Vector2.Zero;
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
    partial class StateKnockFly : Node, IState
    {
        Boss character;
        public int GetId { get; } = (int)State.KnockFly;
        public StateKnockFly(Boss c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.DropWeapon();
            character._DamageEmitter.Monitoring = false;
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
    partial class StateKnockFall : Node, IState
    {
        Boss character;
        public int GetId { get; } = (int)State.KnockFall;
        public StateKnockFall(Boss c)
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
            character.HeightSpeed -= Gravity * (float)delta;
            character.Height += character.HeightSpeed * (float)delta;
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
            SwitchState((int)State.Death);
        }
        else
        {
            SwitchState((int)State.CrouchDown);
        }
    }
    partial class StateKnockDown : Node, IState
    {
        Boss character;
        public int GetId { get; } = (int)State.KnockDown;
        public StateKnockDown(Boss c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.AnimationPlayer.Play("KnockDown");
            character.Direction = Vector2.Zero;
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
    public void DeathEnd()
    {
        QueueFree();
    }
    partial class StateDeath : Node, IState
    {
        Boss character;
        public int GetId { get; } = (int)State.Death;
        public StateDeath(Boss c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.Direction = Vector2.Zero;
            character.AnimationPlayer.Play("Death");
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
    partial class StatePatrol : Node, IState
    {
        Boss character;
        public int GetId { get; } = (int)State.Patrol;
        public StatePatrol(Boss c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.WaitTimer.Start();
            var r = GD.RandRange(-10, 10);
            character.MovePoint = character._Player.Position.Rotated(r);
            character.Direction = character.Position.DirectionTo(character.MovePoint);
            character.AnimationPlayer.Play("Walk");
            return true;
        }

        public int Update(double delta)
        {
            character.FaceToDirection();
            character.Velocity = character.Direction * character.MoveSpeed;
            character.MoveAndSlide();

            return Exit();
        }
        public int Exit()
        {
            if (character.IsOnWall())
            {
                return (int)State.Walk;
            }
            if (character.WaitTimer.TimeLeft <= 0)
            {
                return (int)State.Idle;
            }
            return GetId;
        }
    }

    partial class StateMoveToEdge : Node, IState
    {
        Rect2 rect;
        Boss character;
        public int GetId { get; } = (int)State.MoveToEdge;
        public StateMoveToEdge(Boss c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.FaceToPosition(character._Player.Position);
            character.AnimationPlayer.Play("Walk");
            return true;
        }

        public int Update(double delta)
        {
            rect = character.GetCrimeaRect().GrowSide(Side.Left, -10).GrowSide(Side.Right, -10);
            character.MovePoint = new Vector2(rect.End.X, character._Player.GlobalPosition.Y);
            character.Direction = character.GlobalPosition.DirectionTo(character.MovePoint);

            character.Velocity = character.Direction * character.MoveSpeed;
            character.MoveAndSlide();

            return Exit();
        }
        public int Exit()
        {
            if (character.IsOnWall())
            {
                return (int)State.Idle;
            }
            if (character.GlobalPosition.DistanceTo(character.MovePoint) < 2)
            {
                return (int)State.EdgeLock;
            }
            return GetId;
        }
    }
    partial class StateDefenseRegress : Node, IState
    {
        Boss character;
        public int GetId { get; } = (int)State.DefenseRegress;
        public StateDefenseRegress(Boss c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.WaitTimer.Start();
            character.Direction = new Vector2(-character._DamageEmitter.Scale.X, 0);
            character.AnimationPlayer.Play("DefenseWalk");
            return true;
        }

        public int Update(double delta)
        {
            character.Velocity = character.Direction * character.MoveSpeed;
            character.MoveAndSlide();
            return Exit();
        }
        public int Exit()
        {
            if (character.IsOnWall() || character.WaitTimer.TimeLeft <= character.WaitTimer.WaitTime / 2)
            {
                return (int)State.Idle;
            }
            return GetId;
        }
    }
    partial class StateRegress : Node, IState
    {
        Boss character;
        public int GetId { get; } = (int)State.Regress;
        public StateRegress(Boss c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.WaitTimer.Start();
            character.Direction = new Vector2(-character._DamageEmitter.Scale.X, 0);
            character.AnimationPlayer.Play("Walk");
            return true;
        }

        public int Update(double delta)
        {
            character.Velocity = character.Direction * character.MoveSpeed;
            character.MoveAndSlide();
            return Exit();
        }
        public int Exit()
        {
            if (character.IsOnWall() || character.WaitTimer.TimeLeft <= character.WaitTimer.WaitTime / 2)
            {
                return (int)State.Idle;
            }
            return GetId;
        }
    }
    partial class StateEnterScene : Node, IState
    {
        Boss character;
        public int GetId { get; } = (int)State.EnterScene;
        public StateEnterScene(Boss c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.Visible = true;

            character.SetCollisionMaskValue(1, false);

            character.Direction = character.GlobalPosition.DirectionTo(character.MovePoint);
            character.AnimationPlayer.Play("Walk");
            return true;
        }

        public int Update(double delta)
        {

            character.FaceToDirection();

            character.HeightSpeed -= Gravity * (float)delta;
            character.Height += character.HeightSpeed * (float)delta;
            character.CharacterSprite.Position = Vector2.Up * character.Height;
            if (character.Height <= 0)
            {
                character.CharacterSprite.Position = Vector2.Zero;
                character.Height = 0;
            }
            character.Velocity = character.Direction * character.MoveSpeed;
            character.MoveAndSlide();

            return Exit();
        }
        public int Exit()
        {
            if (character.IsOnWall())
            {
                character.SetCollisionMaskValue(1, true);

                return (int)State.Walk;
            }
            if (character.GlobalPosition.DistanceTo(character.MovePoint) < 2 && character.Height == 0)
            {
                character.SetCollisionMaskValue(1, true);

                return (int)State.Idle;
            }

            return GetId;
        }
    }
    partial class StateEdgeLock : Node, IState
    {
        Rect2 rect;
        Boss character;
        public int GetId { get; } = (int)State.EdgeLock;
        public StateEdgeLock(Boss c)
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
            character.FaceToPosition(character._Player.Position);
            var p = character._Player.Position.Y - character.Position.Y;
            if (Mathf.Abs(p) <= 2)
            {
                character.AnimationPlayer.Play("Idle");
                character.Direction = Vector2.Zero;
            }
            else
            {
                character.AnimationPlayer.Play("Walk");
                character.Direction = new Vector2(0, p).Normalized();
            }
            character.Velocity = character.Direction * character.MoveSpeed * 2 / 3;
            character.MoveAndSlide();

            return Exit();
        }
        public int Exit()
        {
            if (character.IsOnWall())
            {
                return (int)State.Idle;
            }
            if (Mathf.Abs(character._Player.Position.Y - character.Position.Y) < 2 && character.AttackWaitTimer.TimeLeft <= 0)
            {
                return (int)State.Kick;
            }
            return GetId;
        }
    }
    partial class StateNearLock : Node, IState
    {
        Rect2 rect;
        Boss character;
        public int GetId { get; } = (int)State.NearLock;
        public StateNearLock(Boss c)
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
            character.Direction = character.Position.DirectionTo(character._Player.Position).Normalized();
            character.Velocity = character.Direction * character.MoveSpeed;

            character.FaceToDirection();

            character.MoveAndSlide();

            return Exit();
        }
        public int Exit()
        {
            if (character.IsOnWall())
            {
                return (int)State.Idle;
            }
            if (Mathf.Abs(character._Player.Position.Y - character.Position.Y) < 2)
            {
                if (Mathf.Abs(character._Player.Position.Y - character.Position.Y) < 8 && character.AttackWaitTimer.TimeLeft <= 0)
                {
                    return (int)State.Attack;
                }
            }
            return GetId;
        }
    }
    partial class StateKick : Node, IState
    {
        Rect2 rect;
        Boss character;
        public int GetId { get; } = (int)State.Kick;
        public StateKick(Boss c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.AnimationPlayer.Play("Kick");
            return true;
        }

        public int Update(double delta)
        {

            character.Direction = character.Position.DirectionTo(character._Player.Position).Normalized();
            character.Velocity = character.Direction * character.MoveSpeed * 3;
            character.MoveAndSlide();

            return Exit();
        }
        public int Exit()
        {
            if (character._Player.StateID == (int)Player.State.Jump || character.IsOnWall() || character.Position.DistanceTo(character._Player.Position) < 15)
            {
                return (int)State.KickEnd;
            }
            return GetId;
        }
    }
    partial class StateKickEnd : Node, IState
    {
        Rect2 rect;
        Boss character;
        public int GetId { get; } = (int)State.KickEnd;
        public StateKickEnd(Boss c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.AnimationPlayer.Play("KickEnd");
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
    partial class StateDefense : Node, IState
    {
        Rect2 rect;
        Boss character;
        public int GetId { get; } = (int)State.Defense;
        public StateDefense(Boss c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.AnimationPlayer.Play("Defense");
            return true;
        }

        public int Update(double delta)
        {
            character.FaceToPosition(character._Player.Position);
            return Exit();
        }
        public int Exit()
        {
            return GetId;
        }
    }

}
