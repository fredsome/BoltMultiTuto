using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
public class PlayerMotor : EntityBehaviour<IPlayerState>
{
    [SerializeField]
    private Camera _cam = null;
    private NetworkRigidbody _networkRigidbody = null;

    private float _speed = 7f;

    private Vector3 _lastServerPos = Vector3.zero;
    private bool _firstState = true;
    //La vie du joueur maximum
    [SerializeField]
    private int _totalLife = 250;
    public int TotalLife { get => _totalLife; }

    private bool _jumpPressed = false;
    private float _jumpForce = 9f;
    SphereCollider _headCollider;
    private bool _isGrounded = false;
    private float _maxAngle = 45f;

    private void Awake()
    {
        _networkRigidbody = GetComponent<NetworkRigidbody>();
        _headCollider = GetComponent<SphereCollider>();
    }

    public void Init(bool isMine)
    {
        if (isMine)
            _cam.gameObject.SetActive(true);
    }
    public bool IsHeadshot(Collider c)
    {
        return c == _headCollider;
    }
    public State ExecuteCommand(bool forward, bool backward, bool left, bool right, bool jump, float yaw, float pitch)
    {
        Vector3 movingDir = Vector3.zero;
        if (forward ^ backward)
        {
            movingDir += forward ? transform.forward : -transform.forward;
        }
        if (left ^ right)
        {
            movingDir += right ? transform.right : -transform.right;
        }

        if (jump)
        {
            if (_jumpPressed == false && _isGrounded)
            {
                _isGrounded = false;
                _jumpPressed = true;
                _networkRigidbody.MoveVelocity += Vector3.up * _jumpForce;
            }
        }
        else
        {
            if (_jumpPressed)
                _jumpPressed = false;
        }

        movingDir.Normalize();
        movingDir *= _speed;
        _networkRigidbody.MoveVelocity = new Vector3(movingDir.x, _networkRigidbody.MoveVelocity.y, movingDir.z);

        _cam.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
        transform.rotation = Quaternion.Euler(0, yaw, 0);

        State stateMotor = new State();
        stateMotor.position = transform.position;
        stateMotor.rotation = yaw;

        return stateMotor;
    }
    //Code pour que le rigidbody fonctionne
    private void FixedUpdate()
    {
        if (entity.IsAttached)
        {
            if (entity.IsControllerOrOwner)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.3f))
                {
                    float slopeNormal = Mathf.Abs(Vector3.Angle(hit.normal, new Vector3(hit.normal.x, 0, hit.normal.z)) - 90) % 90;

                    if (_networkRigidbody.MoveVelocity.y < 0)
                        _networkRigidbody.MoveVelocity = Vector3.Scale(_networkRigidbody.MoveVelocity, new Vector3(1, 0, 1));

                    if (!_isGrounded && slopeNormal <= _maxAngle)
                    {
                        _isGrounded = true;
                    }
                }
                else
                {
                    if (_isGrounded)
                    {
                        _isGrounded = false;
                    }
                }
            }
        }
    }
    //La position et rotation du joeur
    public void SetState(Vector3 position, float rotation)
    {
        if (Mathf.Abs(rotation - transform.rotation.y) > 5f)
            transform.rotation = Quaternion.Euler(0, rotation, 0);

        if (_firstState)
        {
            if (position != Vector3.zero)
            {
                transform.position = position;
                _firstState = false;
                _lastServerPos = Vector3.zero;
            }
        }
        else
        {
            if (position != Vector3.zero)
            {
                _lastServerPos = position;
            }

            transform.position += (_lastServerPos - transform.position) * 0.5f;
        }
    }

    public struct State
    {
        public Vector3 position;
        public float rotation;
    }
    //Cette fonction est appelé par le joeur qui nous a touché avec son arme et qui veux nous infliger des degats
    //Le server s'occupe donc de diminuer notre vie
    public void Life(PlayerMotor killer, int life)
    {
        //Le server s'occupe alors de donner des degat au joeur
        if (entity.IsOwner)
        {
            int value = state.LifePoints + life;

            if (value < 0)
            {
                state.LifePoints = 0;
            }
            else if (value > _totalLife)
            {
                state.LifePoints = _totalLife;
            }
            else
            {
                state.LifePoints = value;
            }
        }
    }

}
