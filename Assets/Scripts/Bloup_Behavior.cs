using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bloup_Behavior : MonoBehaviour
{
    public float maxSpeed = 4f;
    Rigidbody rb;
    SphereCollider mainCollider;
    BoxCollider mainCollider2;
    private LayerMask m_LayerMask_oiseau;
    private LayerMask m_LayerMask_poisson;
    int bound_x = 100;
    int bound_z = 100;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    void Awake()
    {
        m_LayerMask_oiseau = LayerMask.GetMask("cui cui");
        m_LayerMask_poisson = LayerMask.GetMask("bloup bloup");
        rb = this.GetComponent<Rigidbody>();
        mainCollider = this.GetComponent<SphereCollider>();
        mainCollider2 = this.GetComponent<BoxCollider>();
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
        if (transform.position.x < -bound_x || transform.position.x > bound_x)
        {
            Vector3 vel = rb.velocity;
            vel.x = -2 * vel.x;
            rb.AddForce(vel);
        }
        else if (transform.position.z < -bound_z || transform.position.z > bound_z)
        {
            Vector3 vel = rb.velocity;
            vel.z = -2 * vel.z;
            rb.AddForce(vel);
        }
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(rb.velocity), Time.deltaTime * 40f);
    }

    // Afin de donner des mouvements un peu plus intéressant on rajoute un peu d'aléatoire dans les déplacements
    // En effet sans celà les agents vont tous dans un sens, de façon regroupée sans trop simuler le comportement innée que pourrait avoir un individu.
    // Cependant ils ne doivent pas non plus être totalement aléatoire sans quoi il se rentrerait tous dedans
    // Pour le moment c'est un équilibre qui est trouvé mais les déplacements ne sont pas encore très réaliste
    Vector3 aleatoryMovement() {
        Vector3 vec = new Vector3(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1));
        vec.Normalize();
        return vec;
    }

    // Update is called once per frame
    void Update()
    {
        // Raisons similaires à celles énoncées dans Behavior.cs 
        Vector3 ret6 = 2f * aleatoryMovement();
        Vector3 ret3 = 2f * Separate_boids()[0];
        Vector3 ret4 = 1f * Separate_boids()[1];
        Vector3 ret = Align();
        Vector3 ret2 = Unite();
        Vector3 ret5 = 10f * put_back_in_bounds();
        Vector3 movement = ret + ret2 + ret3 + ret4 + ret5 + ret6;
        movement *= maxSpeed;
        rb.AddForce(movement);
        FixedUpdate();
        UpdateRotation(movement);
    }

    Vector3 put_back_in_bounds()
    {
        // Si on dépasse d'un côté on pousse de l'autre pour faire revenir l'agent dans la limite
        if (transform.position.x < -bound_x)
        {
            return new Vector3(1, 0, 0);
        }
        if (transform.position.x > bound_x)
        {
            return new Vector3(-1, 0, 0);
        }

        if (transform.position.z < -bound_z)
        {
            return new Vector3(0, 0, 1);
        }

        if (transform.position.z > bound_z)
        {
            return new Vector3(0, 0, -1);
        }
        return new Vector3(0, 0, 0);
    }

    Vector3[] Separate_boids()
    {
        // On récupère les autres boids qui sont dans la zone.
        // On cherche à séparer les oiseaux et les poissons dans l'environnement.
        // On va en effet s'échapper plus vite si c'est un prédateur plutôt qu'un congénère.
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, new Vector3(mainCollider.radius / 2, mainCollider.radius / 2, mainCollider.radius / 2), Quaternion.identity, m_LayerMask_poisson);
        Collider[] hitCollidersB = Physics.OverlapBox(gameObject.transform.position, new Vector3(mainCollider2.size.x / 2, mainCollider2.size.y / 2, mainCollider2.size.z / 2), Quaternion.identity, m_LayerMask_oiseau);
        int i = 0;
        int count = 0;
        Vector3 tmpVect = new Vector3(0,0,0);
        // Gestion du cas où les voisins sont des congénères.
        while (i < hitColliders.Length)
        {
            float dist = Vector3.Distance(this.transform.position, hitColliders[i].transform.position);
            // On enlève soi-même
            if (dist > 0)
            {
                tmpVect += this.transform.position - hitColliders[i].transform.position;
                count++;
            }
            i++;
        }
        i = 0;
        Vector3 tmpVect2 = this.transform.position;
        int count2 = 0;
        // gestion du cas où les voisins sont des oiseaux.
        while (i < hitCollidersB.Length)
        {
            float dist = Vector3.Distance(this.transform.position, hitCollidersB[i].transform.position);

            // On enlève soi-même
            if (dist > 0)
            {
                Vector3 col = hitCollidersB[i].transform.position;
                col.y = 0;
                col.Normalize();
                col *= dist;
                tmpVect2 -= col;
                count2++;
            }
            i++;
        }
        if (count > 0)
        {
            tmpVect = tmpVect / count;
        }
        if (count2 > 0)
        {
            tmpVect2 = tmpVect2 / count2;
        }
        if (tmpVect.magnitude > 0)
        {
            //tmpVect = tmpVect - rb.velocity;
            tmpVect.Normalize();
            //tmpVect = tmpVect  * maxSpeed;
        }
        if (tmpVect2.magnitude > 0)
        {
            //tmpVect = tmpVect - rb.velocity;
            tmpVect2.Normalize();
            //tmpVect = tmpVect  * maxSpeed;
        }
        Vector3[] ret = { tmpVect, tmpVect2 };
        return ret;
    }

    Vector3 Unite()
    {
        // On réunit les poissons entre eux
        int i = 0;
        int count = 0;
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, new Vector3(2 * mainCollider.radius, 2 * mainCollider.radius, 2 * mainCollider.radius), Quaternion.identity, m_LayerMask_poisson);
        Vector3 target = new Vector3(0f, 0f, 0f);
        while (i < hitColliders.Length)
        {
            float dist = Vector3.Distance(this.transform.position, hitColliders[i].transform.position);
            if (dist > 0)
            {
                target += hitColliders[i].transform.position;
                count++;
            }
            i++;
        }
        // Maintenant qu'on a la target on s'en rapproche par du steering.
        if (count != 0)
        {
            target = target / count;
            target = target - this.transform.position;
            //target = - target;
            target.Normalize();

        }
        return target;
    }
    
    Vector3 Align()
    {
        // On se calque sur le déplacement des boids qui sont dans notre espace proche
        int i = 0;
        int count = 0;
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, new Vector3(2 * mainCollider.radius, 2 * mainCollider.radius, 2 * mainCollider.radius), Quaternion.identity, m_LayerMask_poisson);
        Vector3 target = new Vector3(0f, 0f, 0f);
        while (i < hitColliders.Length)
        {
            target += hitColliders[i].GetComponent<Rigidbody>().velocity;
            count++;
            i++;
        }
        if (count > 0)
        {
            target = target / count;
            target = target - rb.velocity;
            target.Normalize();
            //target = target * maxSpeed;
        }
        return target;
    }

    // Si on est trop proche d'un oiseau, il nous mange.
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Cui cui") {
            Debug.Log(collision.gameObject.tag);
            collision.gameObject.GetComponent<Behavior>().mont = true;
            Destroy(this);
        }   
    }
}
