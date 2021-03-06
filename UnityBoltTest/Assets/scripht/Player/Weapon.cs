using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Weapon : MonoBehaviour
{
    protected Transform _camera;
  
  
    [SerializeField]
    private Transform _muzzle;
    //lire le scriptable object
    [SerializeField]
    protected WeaponStats _weaponStat = null;
    //Utiliser les infos
    protected int _currentAmmo = 0;
    protected int _currentTotalAmmo = 0;
    protected bool _isReloading = false;
    /// <summary>
    /// /Les references de notre arme a initialiser
    /// </summary>
    protected PlayerWeapons _playerWeapons;
    //La personne qu'on tuera avec l'arme
    protected PlayerMotor _playerMotor;
    protected PlayerCallBacks _playerCallback;
    protected int _fireFrame = 0;
    private Coroutine _reloadCrt = null;
    //Permet de connaitre les degat qu'on inflige a la cible
    protected Dictionary<PlayerMotor, int> _dmgCounter;

    protected int _fireInterval
    {
        get
        {
            int rps = _weaponStat.rpm / 60;
            return BoltNetwork.FramesPerSecond / rps;
        }
    }

    public WeaponStats WeaponStat { get => _weaponStat; }
    public int CurrentAmmo { get => _currentAmmo; }
    public int TotalAmmo { get => _currentTotalAmmo; }

    public virtual void Init(PlayerWeapons pw)
    {
        _playerCallback = pw.GetComponent<PlayerCallBacks>();
        _playerWeapons = pw;
        _playerMotor = pw.GetComponent<PlayerMotor>();
        _camera = _playerWeapons.Cam.transform;

        _currentAmmo = _weaponStat.magazin;
        _currentTotalAmmo = _weaponStat.totalMagazin;
    }
    //Fonction qui execute le programme de l'arme, tirer, viser , tiree ectt........
    public virtual void ExecuteCommand(bool fire, bool aiming, bool reload, int seed)
    {
        if (!_isReloading)
        {
            if (reload && _currentAmmo != _weaponStat.magazin && _currentTotalAmmo > 0)
            {
                _Reload();
            }
            else
            {
                if (fire)
                {
                    _Fire(seed);
                }
            }
        }
    }

    protected virtual void _Fire(int seed)
    {
        if (_currentAmmo >= _weaponStat.ammoPerShot)
        {
            //Verifie si on peut tirer le prochain coup en utilisant le frame du server
            if (_fireFrame + _fireInterval <= BoltNetwork.ServerFrame)
            {
                int dmg = 0;
                _fireFrame = BoltNetwork.ServerFrame;
                //TODO Fire effect
                //Si je suis le server je dis a tout le monde de faire genre je tire: creer un Line pour la balle et un impact
                if (_playerCallback.entity.IsOwner)
                    _playerCallback.FireEffect(WeaponStat.precision, seed);
                //Quand je suis le controlleur du joeur je tire vraiment et je fait un impact
                if (_playerCallback.entity.HasControl)
                    FireEffect(seed, WeaponStat.precision);

                _currentAmmo -= _weaponStat.ammoPerShot;
                Random.InitState(seed);

                _dmgCounter = new Dictionary<PlayerMotor, int>();
                for (int i = 0; i < _weaponStat.multiShot; i++)
                {
                    Vector2 rnd = Random.insideUnitCircle * WeaponStat.precision;
                    Ray r = new Ray(_camera.position, (_camera.forward * 10f) + (_camera.up * rnd.y) + (_camera.right * rnd.x));
                    RaycastHit rh;

                    if (Physics.Raycast(r, out rh, _weaponStat.maxRange))
                    {
                        //Lorsque notre rayon touche un autre joueur on prend son Playermotor
                        PlayerMotor target = rh.transform.GetComponent<PlayerMotor>();
                        if (target != null)
                        {
                            //Si on  la touch? a la tete on applique un scripth particulier
                            if (target.IsHeadshot(rh.collider))
                                dmg = (int)(_weaponStat.dmg * 1.5f);
                            else
                                //Applique des domages
                                dmg = _weaponStat.dmg;

                            if (!_dmgCounter.ContainsKey(target))
                                _dmgCounter.Add(target, dmg);
                            else
                                _dmgCounter[target] += dmg;
                        }
                    }
                }
                //A la fin on fait le total de tout les joeurs qu'on a toucher et on modifie l?tat de leur sante
                foreach (PlayerMotor pm in _dmgCounter.Keys)
                    pm.Life(_playerMotor, -_dmgCounter[pm]);
            }
        }
        else if (_currentTotalAmmo > 0)
        {
            _Reload();
        }
    }

    protected void _Reload()
    {
        _reloadCrt = StartCoroutine(Reloading());
    }

    IEnumerator Reloading()
    {
        _isReloading = true;
        yield return new WaitForSeconds(_weaponStat.reloadTime);
        _currentTotalAmmo += _currentAmmo;
        int _ammo = Mathf.Min(_currentTotalAmmo, _weaponStat.magazin);
        _currentTotalAmmo -= _ammo;
        _currentAmmo = _ammo;
        _isReloading = false;
    }
    //Fonction qui s'occupe d'afficher des effect de bals a l'ecran
    public virtual void FireEffect(int seed, float precision)
    {
        Random.InitState(seed);

        for (int i = 0; i < _weaponStat.multiShot; i++)
        {
            Vector2 rnd = Random.insideUnitSphere * precision;
            Ray r = new Ray(_camera.position, _camera.forward + (_camera.up * rnd.y) + (_camera.right * rnd.x));
            RaycastHit rh;

            if (Physics.Raycast(r, out rh))
            {
                if (_weaponStat.impact)
                    Instantiate(_weaponStat.impact, rh.point, Quaternion.LookRotation(rh.normal));

                if (_weaponStat.decal)
                    if (!rh.rigidbody)
                        Instantiate(_weaponStat.decal, rh.point, Quaternion.LookRotation(rh.normal));

                if (_weaponStat.trail)
                {
                    var trailGo = Instantiate(_weaponStat.trail, _muzzle.position, Quaternion.identity);
                    var trail = trailGo.GetComponent<LineRenderer>();

                    trail.SetPosition(0, _muzzle.position);
                    trail.SetPosition(1, rh.point);
                }
            }
            else if (_weaponStat.trail)
            {
                var trailGo = Instantiate(_weaponStat.trail, _muzzle.position, Quaternion.identity);
                var trail = trailGo.GetComponent<LineRenderer>();

                trail.SetPosition(0, _muzzle.position);
                trail.SetPosition(1, r.direction * _weaponStat.maxRange + _camera.position);
            }
        }
    }
}
