using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Idée faire un composant global qui a un tableau à la fois des proies et des prédateurs et qui permet donc d'accéder aux différentes méthodes de chacun  attribut privé

public class Behavior : MonoBehaviour
{
    public float maxSpeed = 7f;
    Rigidbody rb;
    SphereCollider mainCollider;
    public LayerMask m_LayerMask;
    int bound_x = 100;
    int bound_z = 100;
    bool is_hunting = false;
    public GameObject generator;
    private Generate_fish gen_fish;
    private bool desc = false;
    public bool mont = false;
    private GameObject target;
    // Start is called before the first frame update
    void Awake()
    {
        gen_fish = generator.GetComponent<Generate_fish>();
        rb = this.GetComponent<Rigidbody>();
        mainCollider = this.GetComponent<SphereCollider>();
    }
    // Fonction qui permet de limiter la vitesse de l'agent en dessous d'un certain seuil, en l'occurence, maxSpeed
    void FixedUpdate()
    {
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    // Met à jour la rotation des objets pour qu'ils suivent l'endroit où ils vont
    void UpdateRotation(Vector3 movementDirection)
    {
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(rb.velocity), Time.deltaTime * 40f);
        //this.transform.rotation = Quaternion.LookRotation(movementDirection);
    }

    // Trouve le poisson le plus proche de l'oiseau dans la liste des poissons
    GameObject FindClosestFish() {
        // Initialisation de la distance avec une très grande valeur
        float dist = 7000000000;
        GameObject target=generator;
        for (int i = 0; i < gen_fish.fish_vect.Count; i++) {
            // On compare la distance avec le poisson à la distance minimale actuelle et on change cette distance s'il y a lieu
            float dista = Vector3.Distance(transform.position, gen_fish.fish_vect[i].transform.position);
            if (dista < dist) {
                dist = dista;
                target = gen_fish.fish_vect[i];
            }
        }
        return target;
    }

    // Afin de donner des mouvements un peu plus intéressant on rajoute un peu d'aléatoire dans les déplacements
    // En effet sans celà les agents vont tous dans un sens, de façon regroupée sans trop simuler le comportement innée que pourrait avoir un individu.
    // Cependant ils ne doivent pas non plus être totalement aléatoire sans quoi il se rentrerait tous dedans
    // Pour le moment c'est un équilibre qui est trouvé mais les déplacements ne sont pas encore très réaliste
    Vector3 aleatoryMovement()
    {
        Vector3 vec = new Vector3(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1));
        vec.Normalize();
        return vec;
    }

    // Update is called once per frame
    void Update()
    {
        // On regarde si l'oiseau peut aller chasser (s'il n'y a pas trop d'oiseau déjà en chasse)
        if (!is_hunting) {
            int r = Random.Range(0, 10000);
            if (r <= 5 && gen_fish.nb_hunting < 6) {
                // Il trouve le poisson le plus proche et va le chasser.
                is_hunting = true;
                gen_fish.nb_hunting += 1;
                desc = true;
                target = FindClosestFish();
                rb.constraints = RigidbodyConstraints.None;
            }
        }
        if(is_hunting && mont) {
            // Si on a fini notre chasse, alors on remonte voler en altitude
            if (transform.position.y >= 100)
            {
                // On est arrivée assez haut on arrête de monter
                mont = false;
                rb.constraints = RigidbodyConstraints.FreezePositionY;
                is_hunting = false;
                gen_fish.nb_hunting--;
            }
            else {
                // On est pas encore assez haut, on doit donc continuer vers le haut
                Vector3 goal = transform.position;
                goal.y = 100f;
                Vector3 mov = goal - transform.position;
                mov = mov.normalized*maxSpeed;
                rb.AddForce(mov);
            }
        }
        if (is_hunting && desc) {
            // On diminue sa vitesse à mesure qu'il descend (simule le passage dans l'eau)
            if (transform.position.y <= 5) {
                maxSpeed *= 0.999f;
            }
            // On se rapporche le plus possible de notre cible aquatique
            if (Vector3.Distance(target.transform.position, transform.position) < 0.5)
            {
                gen_fish.fish_vect.Remove(target);
                Destroy(target);
                desc = false;
                mont = true;
            }
            else if (maxSpeed < 1)
            {
                // Si la maxSpeed passe en dessous d'un certain seuil alors on remonte car on ne rattrappera jamais le poisson
                desc = false;
                mont = true;
            }
            else {
                // Sinon on ajoute le mouvement.
                Vector3 mov = (target.transform.position - transform.position) * maxSpeed;
                rb.AddForce(mov);
            }
        }
        // On additionne toutes les forces en les pondérants
        // Le mouvement aléatoire a une importance de 2 afin qu'il n'agisse pas trop mais que ses effets se ressentent quand même
        Vector3 ret5 = 2f * aleatoryMovement();
        // On veut que les animaux restent toujours dans leur zone, on lui donne donc une grosse importance 
        Vector3 ret4 = 6f * put_back_in_bounds();
        // On veut que les boids soient séparé, et que cela compte plus que la réunion et l'alignement
        Vector3 ret3 = 2f*Separate_boids();
        // On aligne les oiseaux
        Vector3 ret = Align();
        // On les faits se réunir
        Vector3 ret2 = Unite();
        Vector3 movement = ret + ret2 + ret3 + ret4 + ret5;
        movement.Normalize();
        movement *= maxSpeed;
        //Debug.Log(ret + ret2 + ret3);
        // On ajoute la force du vecteur résultant à notre agent 
        rb.AddForce(movement);
        //Debug.Log(movement);
        FixedUpdate();
        UpdateRotation(movement);
    }
    Vector3 put_back_in_bounds() {
        //Debug.Log(transform.rotation.x);
        // Si on dépasse d'un côté on pousse de l'autre pour faire revenir l'agent dans la limite
        if (transform.position.x < -bound_x) {
            return new Vector3(1, 0, 0);
        } 
        if( transform.position.x > bound_x)
        {
            return new Vector3(-1, 0, 0);
        }

        if (transform.position.z < -bound_z) {
            return new Vector3(0, 0, 1);
        }
            
        if(transform.position.z > bound_z)
        {
            return new Vector3(0, 0, -1);
        }
        return new Vector3(0, 0, 0);
    }

    Vector3 Separate_boids() {
        // On regarde dans notre environnement proche (dans notre collider)
        // On regarde si ce sont des congénères
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position,new Vector3(mainCollider.radius/2, mainCollider.radius/2, mainCollider.radius/2), Quaternion.identity, m_LayerMask);
        int i = 0;
        int count = 0;
        Vector3 tmpVect = new Vector3(0f,0f,0f);
        while (i < hitColliders.Length)
        {
            // On regarde la distance avec ce qui est dans notre environnement proche
            float dist = Vector3.Distance(this.transform.position, hitColliders[i].transform.position);
            // On enlève soi-même
            if(dist > 0) {
                tmpVect += transform.position - hitColliders[i].transform.position;
                //tmpVect += dist_vect/dist ;
                count++;
            }
            i++;
        }
        // On moyenne les positions de chaque congénère dans notre espace proche.
        if (count > 0) {
            tmpVect = tmpVect / count;
        }
        // On applique une force qui va dans le sens opposé au vector moyen de la position
        if (tmpVect.magnitude > 0)
        {
            tmpVect.Normalize();
        }
        return tmpVect;
    }

    Vector3 Unite() {
        // On réunit les oiseaux entre eux
        int i = 0;
        int count = 0;
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, new Vector3(2*mainCollider.radius, 2*mainCollider.radius, 2*mainCollider.radius), Quaternion.identity, m_LayerMask);
        Vector3 target = new Vector3(0f, 0f, 0f);
        while (i < hitColliders.Length) {
            float dist = Vector3.Distance(this.transform.position, hitColliders[i].transform.position);
            if (dist > 0)
            {
                target += hitColliders[i].transform.position;
                count++;
            }
            i++;
        }
        // Maintenant qu'on a la target moyenne on s'en rapproche par du steering.
        if (count != 0)
        {
            target = target / count;
            target = target - this.transform.position;
            target.Normalize();

        }
        return target;
    }

    Vector3 Align() {
        // On se calque sur le déplacement des boids qui sont dans notre espace proche
        int i = 0;
        int count = 0;
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, new Vector3(2*mainCollider.radius, 2*mainCollider.radius, 2*mainCollider.radius), Quaternion.identity, m_LayerMask);
        Vector3 target = new Vector3(0f, 0f, 0f);
        while (i < hitColliders.Length)
        {
            target += hitColliders[i].GetComponent<Rigidbody>().velocity;
            count++;
            i++;
        }
        if (count > 0) {
            target = target / count;
            target = target - rb.velocity;
            target.Normalize();
        }
        return target;
    }
}
