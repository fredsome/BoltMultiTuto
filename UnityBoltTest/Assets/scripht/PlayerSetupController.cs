using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
public class PlayerSetupController : GlobalEventListener
{

    [SerializeField]
    private GameObject _setupPanel;
    [SerializeField]
    private Camera _sceneCamera;
    public Camera SceneCamera { get => _sceneCamera; }

    //Le server et le clients charge tout les deux cette cette
    //Comme c'est un server , il n'a pas besoin d'afficher le panel du debut
    //Quand le joueur rejoint la session, lui aussi il charge cette scene et quand il arrive vu que c'est un client on lui donne la possibilité de spawn
    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        if (!BoltNetwork.IsServer)
        {
            _setupPanel.SetActive(true);
        }
    }
   
    public override void OnEvent(SpawnControllerEvent evnt)
    {

        //Le server va
        BoltEntity entity = BoltNetwork.Instantiate(BoltPrefabs.Playerpref, new Vector3(0, 1, 0), Quaternion.identity);
        //On donne la permission de controller le personnage a la personne qui a declanché l'event
        entity.AssignControl(evnt.RaisedBy);
    }
    //Quand on appuie sur le button spawn on cree un event au server
    public void SpawnPlayer()
    {
        //Seul le server recoit cette event
        SpawnControllerEvent evnt = SpawnControllerEvent.Create(GlobalTargets.OnlyServer);
        evnt.Send();
    }

}
