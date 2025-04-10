using Godot;
using System;
using System.Linq;
public partial class Enemy : Character
{
    Player _Player;
    Timer WaitTimer;
    Timer AttackWaitTimer;
    Timer MoveTimer;
    EnemySlot Slot = null;
    public Vector2 MovePoint = Vector2.Zero; //使用全局坐标
    public string[] AttackAnimationGroup = ["Punch", "Punch2"];

    public enum State
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
        Death,
        Patrol,
        MoveToSlot,
        MoveToEdge,
        Regress,
        PickUpWeapon,
        EnterScene,
        EdgeLock,
        NearLock,

    }
    public override void _Ready()
    {
        States = new IState[Enum.GetNames(typeof(State)).Length];
        InvincibleStates = [(int)State.EnterScene, (int)State.KnockDown, (int)State.KnockFly, (int)State.KnockFall, (int)State.CrouchDown, (int)State.Death];

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
        _ = new StateDeath(this);
        _ = new StatePatrol(this);
        _ = new StateMoveToSlot(this);
        _ = new StateMoveToEdge(this);
        _ = new StateRegress(this);
        _ = new StatePickUpWeapon(this);
        _ = new StateEnterScene(this);
        _ = new StateEdgeLock(this);
        _ = new StateNearLock(this);


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
        if (InvincibleStates.Contains(StateID))
        {
            _DamageReceiver.Monitorable = false;
        }
        else
        {
            _DamageReceiver.Monitorable = true;
        }
        StateMachineUpdate(delta);
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


    private void OnDamageEmitter_AttackSuccess()
    {
        if (Weapon?.Property == Prop.Properties.MeleeWeapon && StateID == (int)State.MeleeWeaponAttack)
        {
            Weapon.Durability--;
        }
    }

    private void OnDamageEmitter_AreaEntered(Area2D area)
    {
        if (area is DamageReceiver a)
        {
            if (AttackRange((a.Owner as Node2D).Position))
            {
                DamageReceiver.DamageReceivedEventArgs e;
                Vector2 direction = (Vector2.Right * _DamageEmitter.Scale.X).Normalized();

                if (StateID == (int)State.MeleeWeaponAttack)
                {
                    e = new(_DamageEmitter.GetNode<CollisionShape2D>("CollisionShape2D").GlobalPosition, direction, Weapon.Damage);
                }
                else
                {
                    e = new(_DamageEmitter.GetNode<CollisionShape2D>("CollisionShape2D").GlobalPosition, direction);
                }

                a.DamageReceived(_DamageEmitter, e);
            }
        }

    }
    private void OnDamageReceiver_DamageReceived(DamageEmitter emitter, DamageReceiver.DamageReceivedEventArgs e)
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
            FaceToPosition((emitter.Owner as Node2D).Position);
            SwitchState((int)State.KnockFly);
        }

        Health = Mathf.Clamp(Health - e.Damage, 0, MaxHealth);
        emitter?.AttackSuccess();

        if (Health <= 0 || e.Type == DamageReceiver.HitType.knockDown)
        {
            EntityManager.Instance.GenerateParticle(e.Position, e.Direction.X < 0);
        }
    }
    partial class StateIdle : Node, IState
    {
        Rect2 rect;
        Enemy character;
        public int GetId { get; } = (int)State.Idle;
        public StateIdle(Enemy c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.WaitTimer.Start();
            rect = character.GetCrimeaRect().Grow(-15);
            character.Direction = Vector2.Zero;
            character.MovePoint = Vector2.Zero;
            character.AnimationPlayer.Play("Idle");
            return true;
        }

        public int Update(double delta)
        {
            character.Slot ??= character._Player.ReserveSlot(character);

            return Exit();
        }
        public int Exit()
        {

            if (character.IsOnWall())
            {
                return (int)State.Patrol;
            }

            if (character.CanAttackPlayer() && character.AttackWaitTimer.TimeLeft <= 0)
            {
                character.FaceToPosition(character._Player.Position);
                if (character.Weapon == null || character?.Weapon?.Property == Prop.Properties.MeleeWeapon)
                {
                    return (int)State.NearLock;
                }
            }
            if (character.Weapon == null)
            {
                if (character?.CanPickUpProp?.Property == Prop.Properties.MeleeWeapon || character?.CanPickUpProp?.Property == Prop.Properties.RangedWeapon)
                {
                    return (int)State.PickUpWeapon;
                }
            }

            if (character.Slot != null)
            {
                if (character?.Weapon?.Property == Prop.Properties.RangedWeapon)
                {
                    character.Slot.FreeUp();
                    if (rect.HasPoint(character.GlobalPosition))
                    {
                        return (int)State.MoveToEdge;
                    }
                    else
                    {
                        return (int)State.EdgeLock;
                    }
                }
                if (character.GlobalPosition.DistanceTo(character.Slot.GlobalPosition) > 10)
                {
                    return (int)State.MoveToSlot;
                }
            }
            else if (character.WaitTimer.TimeLeft <= 0)
            {

                return (int)State.Patrol;
            }
            return GetId;
        }
    }
    partial class StateWalk : Node, IState
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
                if (character.Weapon == null || character?.Weapon?.Property == Prop.Properties.MeleeWeapon)
                {
                    return (int)State.NearLock;
                }
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
        SwitchState((int)State.Regress);
    }
    partial class StateAttack : Node, IState
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
            SwitchState((int)State.Idle);
        }
    }
    partial class StateHurt : Node, IState
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
    partial class StateKnockFly : Node, IState
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
            Slot?.FreeUp();
            SwitchState((int)State.Death);
        }
        else
        {
            SwitchState((int)State.CrouchDown);
        }
    }
    partial class StateKnockDown : Node, IState
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
    partial class StateMeleeWeaponAttack : Node, IState
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
            character.AttackWaitTimer.Start();
            character.Direction = Vector2.Zero;
            if (character.Weapon.Durability > 0)
            {
                character.AnimationPlayer.Play("KnifeAttack");
                character.PlayAudio("miss");
            }
            else
            {
                character.DropWeapon();
                character.SwitchState((int)State.Idle);
            }
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
    partial class StateRangedWeaponAttack : Node, IState
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
            character.AttackWaitTimer.Start();
            character.Direction = Vector2.Zero;
            character.AnimationPlayer.Play("GunShot");
            if (character.Weapon.Durability > 0)
            {
                character.Weapon.Durability--;
                character.PlayAudio("click");
                var d = (Vector2.Right * character._DamageEmitter.Scale.X).Normalized();
                var p = new Vector2(character.Weapon.ShotPosition.X * d.X, character.Weapon.ShotPosition.Y);
                EntityManager.Instance.GenerateBullet(character.Weapon.Damage, d, new Vector2(character.Position.X + p.X, character.Position.Y), new Vector2(0, p.Y));
            }
            else
            {
                character.DropWeapon();
                character.SwitchState((int)State.Idle);
            }
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
        Enemy character;
        public int GetId { get; } = (int)State.Death;
        public StateDeath(Enemy c)
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
        Enemy character;
        public int GetId { get; } = (int)State.Patrol;
        public StatePatrol(Enemy c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.WaitTimer.Start();
            var r = GD.RandRange(-15, 15);
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
            if (character.Weapon == null)
            {
                if (character?.CanPickUpProp?.Property == Prop.Properties.MeleeWeapon || character?.CanPickUpProp?.Property == Prop.Properties.RangedWeapon)
                {
                    return (int)State.PickUpWeapon;
                }
            }
            if (character.WaitTimer.TimeLeft <= 0)
            {
                return (int)State.Idle;
            }
            return GetId;
        }
    }

    partial class StateMoveToSlot : Node, IState
    {
        Enemy character;
        public int GetId { get; } = (int)State.MoveToSlot;
        public StateMoveToSlot(Enemy c)
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
                character.MovePoint = character.Slot.GlobalPosition;
                character.Direction = character.GlobalPosition.DirectionTo(character.Slot.GlobalPosition);
            }

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
            if (character.GlobalPosition.DistanceTo(character.Slot.GlobalPosition) < 2)
            {
                character.FaceToPosition(character._Player.Position);
                return (int)State.Idle;
            }

            return GetId;
        }
    }
    partial class StateMoveToEdge : Node, IState
    {
        Rect2 rect;
        Enemy character;
        public int GetId { get; } = (int)State.MoveToEdge;
        public StateMoveToEdge(Enemy c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            rect = character.GetCrimeaRect().Grow(-15);
            character.FaceToPosition(character._Player.Position);
            character.AnimationPlayer.Play("Walk");
            return true;
        }

        public int Update(double delta)
        {
            var y = Mathf.Clamp(character._Player.GlobalPosition.Y, rect.Position.Y, rect.End.Y);
            character.MovePoint = new Vector2(rect.End.X, y);
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
    partial class StateRegress : Node, IState
    {
        Enemy character;
        public int GetId { get; } = (int)State.Regress;
        public StateRegress(Enemy c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.WaitTimer.Start();
            character.Direction *= character._DamageEmitter.Scale.X;
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
            if (character.IsOnWall() || character.WaitTimer.TimeLeft <= 0)
            {
                return (int)State.Idle;
            }
            return GetId;
        }
    }
    partial class StatePickUpWeapon : Node, IState
    {
        Enemy character;
        public int GetId { get; } = (int)State.PickUpWeapon;
        public StatePickUpWeapon(Enemy c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.AnimationPlayer.Play("Walk");
            if (character.CanPickUpProp != null)
            {
                character.MovePoint = character.CanPickUpProp.GlobalPosition;
                character.Direction = character.GlobalPosition.DirectionTo(character.CanPickUpProp.GlobalPosition);
            }
            else
            {
                character.MovePoint = Vector2.Zero;
                character.Direction = Vector2.Zero;
            }
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
            if (character.CanPickUpProp == null || character.IsOnWall())
            {
                return (int)State.Idle;
            }
            if (character.GlobalPosition.DistanceTo(character.CanPickUpProp.GlobalPosition) <= 2)
            {
                character.PickUpProp(character.CanPickUpProp);
                return (int)State.CrouchDown;
            }
            return GetId;
        }
    }
    partial class StateEnterScene : Node, IState
    {
        Enemy character;
        public int GetId { get; } = (int)State.EnterScene;
        public StateEnterScene(Enemy c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.SetCollisionMaskValue(1, false);

            character.Direction = character.GlobalPosition.DirectionTo(character.MovePoint);
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
                character.SetCollisionMaskValue(1, true);

                return (int)State.Walk;
            }
            if (character.GlobalPosition.DistanceTo(character.MovePoint) < 2)
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
        Enemy character;
        public int GetId { get; } = (int)State.EdgeLock;
        public StateEdgeLock(Enemy c)
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
                character.Direction = new Vector2(0, Mathf.Sign(p)).Normalized();
            }
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
            if (Mathf.Abs(character._Player.Position.Y - character.Position.Y) < 2 && character.AttackWaitTimer.TimeLeft <= 0)
            {
                return (int)State.RangedWeaponAttack;
            }
            return GetId;
        }
    }
    partial class StateNearLock : Node, IState
    {
        Rect2 rect;
        Enemy character;
        public int GetId { get; } = (int)State.NearLock;
        public StateNearLock(Enemy c)
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
                    if (character?.Weapon?.Property == Prop.Properties.MeleeWeapon)
                    {
                        return (int)State.MeleeWeaponAttack;
                    }
                    else
                    {
                        return (int)State.Attack;
                    }
                }
            }
            return GetId;
        }
    }
}
