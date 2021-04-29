using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Class singleton pour s'assurer qu'on a qu'une vie par joeur
/// 
/// </summary>
public class GUIcontroller : MonoBehaviour
{
    #region Singleton
    private static GUIcontroller _instance = null;

    public static GUIcontroller Current
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<GUIcontroller>();

            return _instance;
        }
    }
    #endregion

    [SerializeField]
    private UIHealthBar _healthBar = null;

    private void Start()
    {
        Show(false);
    }
    //On active l'affichage de la vie
    public void Show(bool active)
    {
        _healthBar.gameObject.SetActive(active);
    }
    //elle recoit l'ordre de modifier la vie et appel la class qui gere les modifications sur le UI
    public void UpdateLife(int current, int total)
    {
        _healthBar.UpdateLife(current, total);
    }
}
