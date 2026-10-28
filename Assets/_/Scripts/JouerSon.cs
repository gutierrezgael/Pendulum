using UnityEngine;

public class JouerSon : MonoBehaviour
{
    public AudioSource source;   // Glisser l'Audio Source ici
    public AudioClip clip;       // Optionnel : pour PlayOneShot

    public void Jouer()
    {
        // Joue ce qui est dans Audio Generator (clip ou Audio Random Container)
        source.Play();
    }

    public void JouerParDessus()
    {
        // Joue un AudioClip par-dessus le son en cours, sans le couper
        source.PlayOneShot(clip);
    }
}