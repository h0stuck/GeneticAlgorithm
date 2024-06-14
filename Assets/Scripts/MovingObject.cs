using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MovingObject : MonoBehaviour
{
    public GameObject system;
    public float speed = 30f;
    int i;

    GameObject targetP;
    GameObject[] p = new GameObject[10];
    int[] c = new int[10];

    // Start is called before the first frame update
    void Start()
    {
        i = 0;
    }

    // Update is called once per frame
    void Update()
    {
        bool d = system.GetComponent<GameSystem>().draw;
        p = system.GetComponent<GameSystem>().points;
        c = system.GetComponent<GameSystem>().nums;
        if (d)
        {
            //Debug.Log(i + " " + c[i]);
            //Debug.Log(Vector2.Distance(transform.position, targetP.transform.position));

            if(i < 9) targetP = p[c[i]];
            if (Vector2.Distance(transform.position, targetP.transform.position) <= 0.2f)
            {
                if (i > 9) { system.GetComponent<GameSystem>().draw = false; i = 0; }
                else if (i == 9) { targetP = system.GetComponent<GameSystem>().StartPosition.gameObject; i++; }
                else { i++; targetP = p[c[i]]; }
            }

            transform.position = Vector3.MoveTowards(this.transform.position,
                    targetP.transform.position, Time.deltaTime * speed);
        }
        else
        {
            transform.position = system.GetComponent<GameSystem>().StartPosition.position;
            targetP = p[c[0]];
        }
    }

    /*private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Point"))
        {
            if (Vector2.Distance(transform.position, targetP.transform.position) <= 0.8f)
            {
                if (i > 9) { system.GetComponent<GameSystem>().draw = false; i = 0; }
                else if(i == 9) targetP = system.GetComponent<GameSystem>().StartPosition.gameObject;
                else { i++; }
            }
        }
    }*/
}
