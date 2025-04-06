using Godot;
using System;

public partial class PropInstance : Node2D
{
    public Prop Instance;
    public float Height = 10;
    public float HeightSpeed = 60;
    protected const float Gravity = 320;
    public int StateID = 0;
    public bool EnterEnd = false;
    protected IState[] States;
    public Sprite2D ShadowSprite;
    public Sprite2D Sprite;
    public AnimationPlayer AnimationPlayer;
    public Area2D PickUpArea;

    protected void AccessingResources()
    {
        Sprite = GetNode<Sprite2D>("Sprite2D");
        ShadowSprite = GetNode<Sprite2D>("ShadowSprite");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        PickUpArea = GetNode<Area2D>("PickUpArea");

    }

    protected void StateMachineUpdate(double delta)
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
    protected interface IState
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
    partial class StateDefault : Node, IState
    {
        PropInstance character;
        public int GetId { get; } = 0;//默认状态
        public StateDefault(PropInstance c)
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
