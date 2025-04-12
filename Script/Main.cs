using Godot;
using System;


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



    AnimationPlayer GO;

    public override void _Ready()
    {
        PlayerHealthBar = GetTree().Root.GetNode<ProgressBar>("UI/HUD/Player/HealthBar");
        PlayerCombo = GetTree().Root.GetNode<Label>("UI/HUD/Player/Combo");

        EnemyHUD = GetTree().Root.GetNode<Control>("UI/HUD/Enemy");
        EnemyAvatar = EnemyHUD.GetNode<TextureRect>("Avatar");
        EnemyHealthBar = EnemyHUD.GetNode<ProgressBar>("HealthBar");

        GO = GetTree().Root.GetNode<AnimationPlayer>("UI/HUD/GO/AnimationPlayer");

        EntityManager.Instance.EnterBattleArea += OnEnterBattleArea;
        EntityManager.Instance.ExitBattleArea += OnExitBattleArea;
        EntityManager.Instance.ShackCamera += OnShackCamera;

        _Player._DamageEmitter.AttackSuccess += OnAttackSuccess;

        EnemyHUD.Visible = false;

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
    }

    public override void _PhysicsProcess(double delta)
    {
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
        if (Input.IsActionPressed("ui_filedialog_refresh"))
        {
            EntityManager.Instance.GenerateBullet = null;
            EntityManager.Instance.GenerateActor = null;
            EntityManager.Instance.GenerateProp = null;
            EntityManager.Instance.GeneratePropName = null;
            EntityManager.Instance.GenerateParticle = null;
            EntityManager.Instance.ShackCamera = null;
            GetTree().ReloadCurrentScene();
        }
    }
}
