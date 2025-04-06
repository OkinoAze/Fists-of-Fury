using Godot;
using System;

public partial class PropGun : PropInstance
{
    enum State
    {
        Idle,
        Fall,
        Destroyed
    }
    public override void _Ready()
    {
        States = new IState[Enum.GetNames(typeof(State)).Length];
        _ = new StateIdle(this);
        _ = new StateFall(this);
        _ = new StateDestroyed(this);

        AccessingResources();

        Instance = EntityManager.Instance.GetProp(EntityManager.Props.Gun);
        SwitchState((int)State.Fall);
    }
    public override void _PhysicsProcess(double delta)
    {
        StateMachineUpdate(delta);
    }
    public partial class StateIdle : Node, IState
    {
        PropGun character;
        public int GetId { get; } = (int)State.Idle;
        public StateIdle(PropGun c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.PickUpArea.Monitorable = true;
            character.AnimationPlayer.Play("Idle");
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
    public partial class StateFall : Node, IState
    {
        PropGun character;
        public int GetId { get; } = (int)State.Fall;
        public StateFall(PropGun c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.PickUpArea.Monitorable = false;
            character.AnimationPlayer.Play("Fall");
            return true;
        }

        public int Update(double delta)
        {
            character.Height += character.HeightSpeed * (float)delta;
            character.HeightSpeed -= Gravity * (float)delta;
            character.Sprite.Position = new Vector2(0, -4) + Vector2.Up * character.Height;
            return Exit();
        }
        public int Exit()
        {
            if (character.Height <= 0)
            {
                return (int)State.Idle;
            }
            return GetId;
        }
    }
    public void DestroyedEnd()
    {
        QueueFree();
    }
    public partial class StateDestroyed : Node, IState
    {
        PropGun character;
        public int GetId { get; } = (int)State.Destroyed;
        public StateDestroyed(PropGun c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.PickUpArea.Monitorable = false;
            character.AnimationPlayer.Play("Destroyed");
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
