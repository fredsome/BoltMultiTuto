using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
public class PlayerCallBacks : EntityBehaviour<IPlayerState>
{
    private PlayerMotor _playerMotor;

    private void Awake()
    {
        _playerMotor = GetComponent<PlayerMotor>();
    }
    //Cette fonction est appeler quand Bolt est au control de ce joeur et que tout a ete setup
    
    public override void Attached()
    {
        //Deh le debut on attache la modification de la vie a une fonction qui sera appeler
        //Lorsque On a modifié LifePoints alors la fonction Update PlayerLife sera appelé
        state.AddCallback("LifePoints", UpdatePlayerLife);

        //Si on est le server On Met la vie du joeur a une valeur par default au debut
        if (entity.IsOwner)
        {

            state.LifePoints = _playerMotor.TotalLife;
        }
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
