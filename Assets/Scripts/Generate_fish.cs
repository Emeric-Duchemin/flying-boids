using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generate_fish : MonoBehaviour
{
    public List<GameObject> fish_vect = new List<GameObject>();
    public int nb_fish=30;
    public GameObject fish_prefab;
    public int bound_x=100;
    public int bound_z=100;
    public int nb_hunting = 0;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 minPosition = new Vector3(-bound_x,0f,-bound_z);
        Vector3 maxPosition = new Vector3(bound_x, 0f, bound_z);
        for (int i = 0; i < nb_fish; i++) 
        {
            // On instancie un nouveau poisson à un endroit random sur la map.
            // On lui affecte le comportement de poisson.
            Vector3 randomPosition = new Vector3(Random.Range(minPosition.x, maxPosition.x), Random.Range(minPosition.y, maxPosition.y), Random.Range(minPosition.z, maxPosition.z));
            GameObject fish = Instantiate(fish_prefab,randomPosition, Quaternion.identity);
            Bloup_Behavior f = fish.GetComponent<Bloup_Behavior>();
            // On l'ajoute à la liste de poisson utile pour trouver le poisson le plus proche d'un oiseau (pour la chasse).
            fish_vect.Add(fish);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
