using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// //La Classe qui s'occupe de mettre la vie dans le UI du joeur pour que se soit visible
/// </summary>
public class UIHealthBar : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private Text _text = null;
    //On lui demande constament de modifier le UI
    public void UpdateLife(int hp, int totalHp)
    {
        float f = (float)hp / (float)totalHp;
        _text.text = hp.ToString();
    }
}
