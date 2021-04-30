using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
public class PlayerCallBacks : EntityEventListener<IPlayerState>
{
    private PlayerMotor _playerMotor;
    private PlayerWeapons _playerWeapons;
    private void Awake()
    {
        _playerMotor = GetComponent<PlayerMotor>();
        _playerWeapons = GetComponent<PlayerWeapons>();
    }
    //Cette fonction est appeler quand Bolt est au control de ce joeur et que tout a ete setup
    
    public override void Attached()
    {
        //Deh le debut on attache la modification de la vie a une fonction qui sera appeler
        //Lorsque On a modifié LifePoints alors la fonction Update PlayerLife sera appelé
        state.AddCallback("LifePoints", UpdatePlayerLife);
        state.AddCallback("Pitch", _playerMotor.SetPitch);

        //Si on est le server On Met la vie du joeur a une valeur par default au debut
        if (entity.IsOwner)
        {

            state.LifePoints = _playerMotor.TotalLife;
        }
    }
    //
    public void FireEffect(float precision, int seed)
    {
        //Notre joueur dit a tout le monde sauf au server de creer l'effet de tire avec des balles tracantes
        FireEffectEvent evnt = FireEffectEvent.Create(entity, EntityTargets.EveryoneExceptOwnerAndController);
        evnt.Precision = precision;
        evnt.Seed = seed;
        evnt.Send();
    }
    //L'orsque ce joueur recoit l'ordre de creer un effet de tire il le fait 
    //Tout le monde va donc le faire
    public override void OnEvent(FireEffectEvent evnt)
    {
        _playerWeapons.FireEffect(evnt.Seed, evnt.Precision);
    }
    private void Update()
    {
        //NB : seul le server peut modifier le state des joueurs
       //------------------------------------ //Juste Pour tester------------------------------------------------
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            //On modifie la vie de ce joeur specifiquement
            state.LifePoints += 10;

        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            //On modifie la vie de ce joeur specifiquement
            state.LifePoints -= 10;

        }
    }
    public void UpdatePlayerLife()
    {
        //Mis a jour de notre UI seulement si on controle l'entité
        //On ne veux pas modifié le UI de tout le monde
        if (entity.HasControl)
            GUIcontroller.Current.UpdateLife(state.LifePoints, _playerMotor.TotalLife);
    }
}
