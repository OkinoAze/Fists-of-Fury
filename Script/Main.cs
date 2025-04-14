using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class Main : Node
{
    [Export]
    public Player _Player;
    [Export]
    public Camera2D Camera;
    [Export]
    float strength = 1.5f;
    bool ShackCamera = false;
    bool StopCamera = false;
    [Export]
    float ShackTime = 0.2f;
    float ShackNowTime = 0;


    uint Combo = 0;
    float ComboTime = 3f;
    float ComboNowTime = 0;


    ProgressBar PlayerHealthBar;
    Label PlayerCombo;
    Control EnemyHUD;
    TextureRect EnemyAvatar;
    ProgressBar EnemyHealthBar;

    AudioStreamPlayer SFX;
    AnimationPlayer GO;


    Control ReSpawn;
    Label ReSpawnTime;
    Label ReSpawnCaption;
    Timer PlayerReSpawnTimer;

    Node Stage;

    List<Node2D> StaticNodeList = [];

    AnimationPlayer TransitionAnimationPlayer;

    public override void _Ready()
    {
        PlayerHealthBar = GetTree().Root.GetNode<ProgressBar>("UI/HUD/Player/HealthBar");
        PlayerCombo = GetTree().Root.GetNode<Label>("UI/HUD/Player/Combo");

        EnemyHUD = GetTree().Root.GetNode<Control>("UI/HUD/Enemy");
        EnemyAvatar = EnemyHUD.GetNode<TextureRect>("Avatar");
        EnemyHealthBar = EnemyHUD.GetNode<ProgressBar>("HealthBar");

        GO = GetTree().Root.GetNode<AnimationPlayer>("UI/HUD/GO/AnimationPlayer");


        ReSpawn = GetTree().Root.GetNode<Control>("UI/ReSpawn");


        SFX = GetNode<AudioStreamPlayer>("SFX");

        PlayerReSpawnTimer = GetNode<Timer>("PlayerReSpawnTimer");
        ReSpawnTime = ReSpawn.GetNode<Label>("Time");
        ReSpawnCaption = ReSpawn.GetNode<Label>("Caption");

        Stage = GetNode<Node>("Stage");
        TransitionAnimationPlayer = GetNode<AnimationPlayer>("TransitionAnimationPlayer");

        EntityManager.Instance.EnterBattleArea += OnEnterBattleArea;
        EntityManager.Instance.ExitBattleArea += OnExitBattleArea;
        EntityManager.Instance.ShackCamera += OnShackCamera;
        EntityManager.Instance.ReSpawnPlayer += OnReSpawnPlayer;
        EntityManager.Instance.SwitchScene += OnSwitchScene;

        _Player._DamageEmitter.AttackSuccess += OnAttackSuccess;

        var packedScene = ResourceLoader.Load<PackedScene>("res://Scene/stage-1.tscn");
        EntityManager.Instance.SwitchScene(packedScene);

        EnemyHUD.Visible = false;
        ReSpawn.Visible = false;
    }

    private void OnSwitchScene(PackedScene scene)
    {
        _Player.ProcessMode = ProcessModeEnum.Disabled;

        var children = Stage.GetChildren();
        if (children.Count > 0)
        {
            foreach (var item in children)
            {
                item.QueueFree();
            }
        }
        var newStage = scene.Instantiate();

        Stage.AddChild(newStage);
        var actorContainer = newStage.GetNodeOrNull<Node2D>("ActorContainer");
        if (actorContainer != null)
        {
            var mainActorContainer = GetNode<Node2D>("ActorContainer");
            var actorContainerChildren = actorContainer.GetChildren();
            foreach (Node2D item in actorContainerChildren.Cast<Node2D>())
            {
                StaticNodeList.Add(item);
                actorContainer.RemoveChild(item);
                mainActorContainer.AddChild(item);
                item.ProcessMode = ProcessModeEnum.Disabled;
            }
        }
        StaticNodeList.Sort(new PositionXComparer());

        _Player.ProcessMode = ProcessModeEnum.Inherit;
        _Player.Position = new Vector2(25, 45);
        TransitionAnimationPlayer.Play("StartTransition");

    }


    private void OnReSpawnPlayer()
    {
        PlayerReSpawnTimer.Start();
        ReSpawn.Visible = true;
    }

    void ActiveNode()
    {
        if (StaticNodeList.Count > 0)
        {
            var item = StaticNodeList[0];
            if (_Player.GetCameraRect().HasPoint(item.GlobalPosition))
            {
                item.ProcessMode = ProcessModeEnum.Inherit;
                StaticNodeList.RemoveAt(0);
            }
            else
            {
                return;
            }
        }
    }

    public async void OnAttackSuccess(Character character)
    {
        EnemyHUD.Visible = true;
        ComboNowTime = 0;
        Combo++;
        PlayerCombo.Text = "x" + Combo.ToString();


        var path = "res://Art/UI/Avatars/avatar-" + character.AvatarName + ".png";
        if (ResourceLoader.Exists(path))
        {
            EnemyAvatar.Texture = ResourceLoader.Load<Texture2D>(path);
        }
        EnemyHealthBar.MaxValue = character.MaxHealth;
        EnemyHealthBar.Value = character.Health;
        if (character.Health <= 0)
        {
            await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);
            EnemyHUD.Visible = false;
        }
    }


    private void OnShackCamera()
    {
        ShackCamera = true;
    }


    private void OnEnterBattleArea(BattleArea battleArea)
    {
        StopCamera = true;
    }


    private void OnExitBattleArea(BattleArea battleArea)
    {
        StopCamera = false;
        GO.Play("GoGoGo");
        SFX.Play();
    }

    void CameraController(double delta)
    {
        if (StopCamera == false && _Player.Position.X > Camera.Position.X)
        {
            Camera.Position = new Vector2(_Player.Position.X, Camera.Position.Y);
        }

        if (ShackCamera == true && ShackNowTime < ShackTime)
        {
            ShackNowTime += (float)delta;
            Camera.Offset = new Vector2((float)GD.RandRange(-strength, strength), (float)GD.RandRange(-strength, strength));
        }
        else
        {
            ShackCamera = false;
            ShackNowTime = 0;
            Camera.Offset = new Vector2(0, 0);
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        CameraController(delta);
        ActiveNode();
        PlayerHealthBar.MaxValue = _Player.MaxHealth;
        PlayerHealthBar.Value = _Player.Health;
        if (ComboNowTime < ComboTime)
        {
            ComboNowTime += (float)delta;
        }
        else
        {
            Combo = 0;
            PlayerCombo.Text = "";
        }

        if (ReSpawn.Visible == true)
        {
            if (PlayerReSpawnTimer.TimeLeft <= 0)
            {
                ReSpawnCaption.Text = "GameOver";
                ReSpawnTime.Text = "";
            }
            else
            {
                ReSpawnCaption.Text = "ReSpawn";
                ReSpawnTime.Text = ((int)PlayerReSpawnTimer.TimeLeft).ToString();

                if (Input.IsActionJustPressed("attack"))
                {
                    _Player.Health = _Player.MaxHealth;
                    _Player.SwitchState((int)Player.State.EnterScene);
                    //TODO 给所有敌人一击

                    GetTree().CallGroup("Enemy", "PowerDamageReceived");

                    ReSpawn.Visible = false;
                }
            }
        }

        if (Input.IsActionPressed("ui_filedialog_refresh"))
        {
            EntityManager.Instance.GenerateBullet = null;
            EntityManager.Instance.GenerateActor = null;
            EntityManager.Instance.GenerateProp = null;
            EntityManager.Instance.GeneratePropName = null;
            EntityManager.Instance.GenerateParticle = null;
            EntityManager.Instance.ShackCamera = null;
            EntityManager.Instance.EnterBattleArea = null;
            EntityManager.Instance.ExitBattleArea = null;
            EntityManager.Instance.ReSpawnPlayer = null;
            EntityManager.Instance.SwitchScene = null;

            GetTree().ReloadCurrentScene();
        }
    }
    public class PositionXComparer : IComparer<Node2D>
    {
        public int Compare(Node2D x, Node2D y)
        {
            if (x.Position.X == y.Position.X)
            {
                return x.Position.Y.CompareTo(y.Position.Y);
            }
            else
            {
                return x.Position.X.CompareTo(y.Position.X);
            }

        }

    }
}
