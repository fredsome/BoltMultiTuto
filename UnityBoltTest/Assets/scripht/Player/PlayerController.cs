using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Bolt;
public class PlayerController : EntityBehaviour<IPhysicState>
{

    private PlayerMotor _playerMotor;
    private PlayerWeapons _playerWeapons;
    private bool _forward;
    private bool _backward;
    private bool _left;
    private bool _right;
    private float _yaw;
    private float _pitch;
    private bool _jump;
    private bool _hasControl = false;


    private bool _fire;
    private bool _aiming;
    private bool _reload;
     int _seed = 0;

    private float _mouseSensitivity = 5f;

    public void Awake()
    {
        _playerWeapons = GetComponent<PlayerWeapons>();
        _playerMotor = GetComponent<PlayerMotor>();
    }

    //Cette fonction est appeler quand Bolt est au control de ce joeur et que tout a ete setup
    public override void Attached()
    {
        state.SetTransforms(state.Transform, transform);
        if (entity.HasControl)
        {
            //Quand on spawn on veux prendre le controle de notre joueur
            _hasControl = true;
            //On active son UI de son coté , le UI des autres joeurs est toujours desactivé
            GUIcontroller.Current.Show(true);
        }
        //On lance la fonction Init
        Init(entity.HasControl);
        //Les mouvements du joeuur sont Initialisés
        _playerMotor.Init(entity.HasControl);
        _playerWeapons.Init();
    }

    public void Init(bool isMine)
    {
        //Si on est le controller de cette object, le client qui veux manipuler son joeur
        if (isMine)
        {
            //On active la camera qu'on sais approprié dans le scripth PlayerSetupController
            Cursor.lockState = CursorLockMode.Locked;
            //On desactive la camera de la scene deh qu'on est lancé dans jeu en temp que client
            FindObjectOfType<PlayerSetupController>().SceneCamera.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (_hasControl)
            PollKeys();

    }

    private void PollKeys()
    {
        _forward = Input.GetKey(KeyCode.W);
        _backward = Input.GetKey(KeyCode.S);
        _left = Input.GetKey(KeyCode.A);
        _right = Input.GetKey(KeyCode.D);
        _jump = Input.GetKey(KeyCode.Space);
        _yaw += Input.GetAxisRaw("Mouse X") * _mouseSensitivity;
        _yaw %= 360f;
        _pitch += -Input.GetAxisRaw("Mouse Y") * _mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, -85, 85);

        _fire = Input.GetMouseButton(0);
        if (_fire)
            _seed = Random.Range(0, 1023);
        _aiming = Input.GetMouseButton(1);
        _reload = Input.GetKey(KeyCode.R);
    }

    public override void SimulateController()
    {
        // IPlayerCommandInputInput input C'est comme ca on appel les Imput de Bolt
        //PlayerCommandInput.Create(); c'est comme ca On appel la classImput que j'ai crée dans l'inspector
        IPlayerCommandInputInput input = PlayerCommandInput.Create();
        input.Forward = _forward;
        input.Backward = _backward;
        input.Right = _right;
        input.Left = _left;
        input.Yaw = _yaw;
        input.Pitch = _pitch;
        input.Jump = _jump;

        input.Fire = _fire;
        input.Scope = _aiming;
        input.Reload = _reload;
        input.Seed = _seed;

        //cette ligne est tres importante, elle permet de lister tout les imputs fait par le joeur
        entity.QueueInput(input);
        //Le joueur fait bouger le personnage dans lui sont monde vrai monde
        _playerMotor.ExecuteCommand(_forward, _backward, _left, _right, _jump, _yaw, _pitch);
        _playerWeapons.ExecuteCommand(_fire,_aiming,_reload,_seed);
    }

    //Le server et le client execute cette fonction
    public override void ExecuteCommand(Command command, bool resetState)
    {
        PlayerCommandInput cmd = (PlayerCommandInput)command;

        if (resetState)
        {
            _playerMotor.SetState(cmd.Result.Position, cmd.Result.Rotation);
        }
        else
        {
            PlayerMotor.State motorState = new PlayerMotor.State();
            //Quand on est le server
            if (!entity.HasControl)
            {
                motorState = _playerMotor.ExecuteCommand(
                cmd.Input.Forward,
                cmd.Input.Backward,
                cmd.Input.Left,
                cmd.Input.Right,
                cmd.Input.Jump,
                cmd.Input.Yaw,
                cmd.Input.Pitch);
                _playerWeapons.ExecuteCommand(cmd.Input.Fire, cmd.Input.Scope, cmd.Input.Reload, cmd.Input.Seed);
            }

            cmd.Result.Position = motorState.position;
            cmd.Result.Rotation = motorState.rotation;
        }
    }
}

