using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
/// <summary>
/// Une classe qui doit etre ajouter a tout les Entitées qui ont un rigid body pour pouvoir etre synchronisé sur le network!!
/// Quand on veut appliquer des mouvements ou des forces a cette objects, on manipule la fonction MoveVelocity
/// </summary>
public class NetworkRigidbody : EntityBehaviour<IPhysicState>
{
    private Rigidbody _rb;
    [SerializeField]
    private float _gravityForce = 1f;
    private bool _useGravity = true;
    private Vector3 _moveVelocity;
    public Vector3 MoveVelocity
    {
        set
        {
            if (entity.IsControllerOrOwner)
            {
                _moveVelocity = value;
            }
        }

        get
        {
            return _moveVelocity;
        }
    }

    public float GravityForce
    {
        get
        {
            return Physics.gravity.y * _gravityForce * BoltNetwork.FrameDeltaTime;
        }
    }

    public void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public override void Attached()
    {
        state.SetTransforms(state.Transform, transform);
    }

    private void FixedUpdate()
    {
        if (entity.IsAttached)
        {
            if (entity.IsControllerOrOwner)
            {
                float g = _moveVelocity.y;

                if (_useGravity)
                {
                    if (_moveVelocity.y < 0f)
                        g += 1.5f * GravityForce;
                    else if (_moveVelocity.y > 0f)
                        g += 1f * GravityForce;
                    else
                        g = _rb.velocity.y;
                }

                _moveVelocity = new Vector3(_moveVelocity.x, g, _moveVelocity.z);
                _rb.velocity = _moveVelocity;
            }
        }
    }
}
