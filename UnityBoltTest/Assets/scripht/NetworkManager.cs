
using System;
using UdpKit;
using UnityEngine;
using Bolt.Matchmaking;

public class NetworkManager : Bolt.GlobalEventListener
{
    [SerializeField]
    private UnityEngine.UI.Text feedback;

    public void FeedbackUser(string text)
    {
        feedback.text = text;
    }

    public void Connect()
    {
        FeedbackUser("Connnecting ...");
        BoltLauncher.StartClient();
    }

    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {
        FeedbackUser("Searching ...");
        //On rejoin le seul server qui existe pour le moment
        //Vu que le server est dans PlayScene on va essayer de load ca
        BoltMatchmaking.JoinSession(HeadlessServerManager1.RoomID());
    }

    public override void Connected(BoltConnection connection)
    {
        FeedbackUser("Connected !");
    }
}
