using System.Collections;
using System.Collections.Generic;
using Genetic_Algorithm;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSystem : MonoBehaviour
{
    public Transform StartPosition;
    public GameObject[] points = new GameObject[11];

    public TextMeshProUGUI disText;
    public TextMeshProUGUI seqText;
    public TextMeshProUGUI genText;

    public int lastGen = 0;
    public float fastestDis = 999999999f;
    public bool draw = false;
    public bool useDraw = true;
    public int[] nums = { 4, 0, 8, 1, 7, 3, 2, 6, 9, 5 };
    
    private Genetic genetic;

    public static GameSystem instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        genetic = new Genetic(10);
        fastestDis = 100000f;
        disText.text = string.Format("Fitness : {0:0.000}", genetic.optimumFitness);
        genText.text = string.Format("Generation : {0}", genetic.Generation);
        seqText.text = " 4, 0, 8, 1, 7, 3, 2, 6, 9, 5 }";
        FirstDraw();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            while ((float)genetic.optimumFitness >= fastestDis && lastGen + 1000 > genetic.Generation)
            {
                Operate();
            }

            lastGen = genetic.Generation;
            fastestDis = (float)genetic.optimumFitness;

            string s = "";
            for (int i = 0; i < 10; i++)
            {
                s += nums[i] + ", ";
            }
            Debug.Log(s);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            for (int i = 0; i < 5; i++)
            {
                Operate();
            }
            
            lastGen = genetic.Generation;
            fastestDis = (float)genetic.optimumFitness;

            string s = "";
            for (int i = 0; i < 10; i++)
            {
                s += nums[i] + ", ";
            }
            Debug.Log(s);
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            draw = true;
        }
    }

    void FirstDraw()
    {
        draw = true;
    }
    
    private void Operate()
    {
        genetic.Operate();
        //panel1.Refresh();
        string name = "";
        List<Node> Gene = new List<Node>();

        disText.text = string.Format("Fitness : {0:0.000}", genetic.optimumFitness);
        genText.text = string.Format("Generation : {0}", genetic.Generation);

        //textBox1.Clear();
        name = "";

        Gene = genetic.EvolutionPos;

        int i = 0;
        foreach (var id in Gene)
        {
            nums[i++] = id.Name - 'A';
            Debug.Log(nums[i-1]);
            name += (id.Name - 'A').ToString();
            name += ", ";
        }
        

        seqText.text = name;
        //textBox1.Text = name;
    }
}
