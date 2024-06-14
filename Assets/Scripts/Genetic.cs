using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

namespace Genetic_Algorithm
{
    public class Node
    {
        public char Name { get; }
        public Vector2 Position { get; }

        public Node(char name, Vector2 position)
        {
            this.Name = name;
            this.Position = position;
        }
    }
    //---------------------------------------------------------------------------------------------------------
    public static class Constant
    {
        public static int MAX_DISTANCE = 100000;
        public static int MAX_BUFFER = 10;
    }
    //---------------------------------------------------------------------------------------------------------
    internal class Genetic
    {
        public int NodeCounts { get; set; }             // 한개의 유전자에 포함된 노드의 수 
        public int Generation { get; set; } = 0;        // 현재 세대 수 
        public double optimumFitness { get; set; }      // 현재 세대에서 최적의 적합도 값 
        private int refValueMutation;                   // 돌연변이 발생을 위한 기준값 

        public List<Node> EvolutionPos = new List<Node>();         // 현재 세대 유전자 중 최적의 유전자 정보
        private List<Node> NodeGroup = new List<Node>();            // 생성된 노드들의 집합
        private List<List<Node>> genes = new List<List<Node>>();    // 현재 세대 유전자 그룹
        private List<double> Fitness = new List<double>();          // 각 유전자 별 적합도 값의 모음
        private List<int> RouletteWheel = new List<int>();          // 유전자 교배에 사용할 룰렛 

        Random rand = new Random(DateTime.Now.Millisecond);
        //---------------------------------------------------------------------------------------------------------
        public Genetic(int nodeCounts)
        {
            this.NodeCounts = nodeCounts;
            Debug.Log("genetic");
            reset();
        }
        //---------------------------------------------------------------------------------------------------------
        public List<Node> getGene(int index)
        {
            return genes[index];
        }
        //---------------------------------------------------------------------------------------------------------
        public Vector2 getNodePosition(int index)
        {
            return NodeGroup[index].Position ;
        }
        //---------------------------------------------------------------------------------------------------------
        public char getNodeName(int index)
        {
            return NodeGroup[index].Name ;
        }
        //---------------------------------------------------------------------------------------------------------
        public Vector2 getEvolutionPosition(int index)
        {
            return EvolutionPos[index].Position;
        }
        //---------------------------------------------------------------------------------------------------------
        public void reset()
        {
            Generation = 1;
            optimumFitness = 1000000.0;
            refValueMutation = Environment.TickCount % 100;

            NodeGroup.Clear();
            EvolutionPos.Clear();
            genes.Clear();
            Fitness.Clear();

            initNodePosition();     // 노드를 생성
            generateParent();       // 생성된 노드를 이용하여 부모 유전자 생성 
            calcuFitness();         // 부모 유전자의 최적 경로 계산     

            EvolutionPos = NodeGroup.ToList();
        }
        //---------------------------------------------------------------------------------------------------------
        public void Operate()
        {
            var index = 0;

            while (true)
            {
                switch(index)
                {
                    case 0: makeRouletteWheel();    break;
                    case 1: selection();            break;
                    case 2: calcuFitness();         break;          // 각 유전자의 최적 경로값 계산 
                    case 3: evalutionGene();        break;         // 최적 경로의 유전자를 searching  
                }

                if (++index > 3) break;
            }

            Generation++ ;   // 1 번 수행할때 마다 세대가 증가 
        }
        //---------------------------------------------------------------------------------------------------------
        private void makeRouletteWheel()
        {
            // 적합도가 가장 작은순서로 가중치를 두어서 룰렛휠을 만든다. 

            List<double> fitnessList = Fitness.ToList();
            RouletteWheel.Clear();
            int index, count = Constant.MAX_BUFFER ;

            for(int i = 0; i < NodeCounts; i++)
            {
                index = fitnessList.IndexOf(fitnessList.Min());
                fitnessList[index] = 1000000;

                for (int loop = 0 ; loop < count ; loop++)
                {
                    RouletteWheel.Add(index);
                }

                count--;
            }
        }
        //---------------------------------------------------------------------------------------------------------
        private void initNodePosition()
        {
            // random 으로 노드의 위치를 생성한다.
            char name ;
            var g = GameSystem.instance;

            for (int loop = 0 ; loop < NodeCounts ; loop++)
            {

                name = (char)('A' + loop) ;

                NodeGroup.Add(new Node(name, g.points[loop].transform.position));     
            }
        }
        //---------------------------------------------------------------------------------------------------------
        private double distancePointToPoint(Vector2 P1, Vector2 P2)
        {
            // 2개 노드간의 거리를 계산한다. 
            double distance = Math.Sqrt(Math.Pow(P1.x - P2.x, 2) + Math.Pow(P1.y - P2.y, 2));  // 좌표평면 상의 두점의 거리(피타고라스 정리)

            return distance;
        }
        //---------------------------------------------------------------------------------------------------------
        private void calcuFitness()
        {
            double sum = 0;
            Fitness.Clear();

            foreach (var gene in genes) 
            {
                sum = 0;

                for(int loop=0; loop < NodeCounts-1; loop++)
                {
                    sum += distancePointToPoint(gene[loop].Position, gene[loop+1].Position);  // 유전자의 적합도 계산 
                }

                sum += distancePointToPoint(gene[NodeCounts-1].Position, gene[0].Position);  // 마지막 노드에서 처음 위치로 회귀 거리

                Fitness.Add(sum);  // 각 유전자별 적합도 값을 저장한다. 
            }
        }
        //---------------------------------------------------------------------------------------------------------
        private void evalutionGene()
        {
            // 최적의 경로를 가지고 있는 유전자를 선택한다. 

            int index = 0, max = 0, max_pos = 0 ;           
            bool change = false;
            
            foreach (var value in Fitness)
            {
                if(value < optimumFitness)
                {
                    optimumFitness = value;
                    EvolutionPos.Clear();
                    EvolutionPos = genes[index].ToList() ;

                    change = true;
                }

                if(value > max)
                {
                    max_pos = index;
                }
                index++;
            }

            if(change == false)     // 최적의 경로롤 가진 유전자를 찾지 못했을 경우 
            {
                genes.RemoveAt(max_pos);
                genes.Add(EvolutionPos);
            }
        }
        //---------------------------------------------------------------------------------------------------------
        public void generateParent()
        {
            // 개별 유전자를 만들고 이 유전자들을 모아서 유전자 그룹의 만든다. 

            for(int loop=0 ; loop < Constant.MAX_BUFFER ; loop++)
            {
                genes.Add(generateGene());  // 부모 유전자 그룹 생성(1 세대 유전자)
            }
        }
        //---------------------------------------------------------------------------------------------------------
        public List<Node> crossover(List<Node> gene1, List<Node> gene2)
        {
            // 유전자 교배 : 두개의 유전자를 합쳐서 새로운 자식 유전자를 생성한다. 
            List<Node> child = new List<Node>();

            int division = rand.Next(1, NodeCounts-1);  // 유전자가 분리되는 위치를 랜덤으로 정한다. 
            int index = 0;

            for (int loop=0 ; loop < division ; loop++)  // 선행 부분은 gene1의 것을 취한다. 
            {
                child.Add(gene1[loop]);
            }

            for (int loop = division ; loop < NodeCounts ; loop++)   // 후행 부분은 gene2의 것을 취한다. 
            {
                index = child.FindIndex(x => x.Name == gene2[loop].Name);   // 동일한 노드가 있는지 확인 

                if(index < 0 ) child.Add(gene2[loop]);            // 동일한 노드가 없으면 추가
                else                                              // 동일한 노드가 있으면 gene2 의 선행 부분에서 없는것을 찾아서 채운다. 
                {
                    for (int i = 0; i < division; i++) 
                    {
                        index = child.FindIndex(x => x.Name == gene2[i].Name);
                        if (index < 0)
                        {
                            child.Add(gene2[i]);
                            break;
                        }
                    }
                }
            }

            mutation(child);  // 돌연변이 생성

            return child;
        }
        //---------------------------------------------------------------------------------------------------------
        private void selection()
        {
            int g1, g2 ;
            bool compare = false ;
            List<List<Node>> newGeneGroup = new List<List<Node>>();
            List<Node> newGene = new List<Node>();

            for (int i = 0; i < Constant.MAX_BUFFER; i++)
            {
                g1 = RouletteWheel[rand.Next(0, RouletteWheel.Count)];  // 룰렛휠을 이용하여 첫번째 유전자를 선택

                while (true)
                {
                    var temp = rand.Next(0, RouletteWheel.Count);

                    if (g1 != RouletteWheel[temp])
                    {
                        g2 = RouletteWheel[temp];                       // 첫번째 유전자를 제외한 다른 유전자 선택
                        break;
                    }
                }

                newGene = crossover(genes[g1], genes[g2]);
                compare = false;

                foreach (var node in newGeneGroup)
                {
                    compare = Enumerable.SequenceEqual(node, newGene);  // 동일한 유전자가 있는지 확인 
                    if (compare) break;
                }             
                
                if (compare == false) newGeneGroup.Add(newGene);        // 동인한 유전자가 없을 경우만 삽입 
                else i--;
            }

            genes.Clear();                      // 기존의 부모 유전자 제거 
            genes = newGeneGroup.ToList();      // 새로운 유전자로 갱신 
        }
        //---------------------------------------------------------------------------------------------------------
        private void mutation(List<Node> gene)
        {
            // 유전자의 노드 위치를 변경하여 돌연변이 형성 ( 1% 확율 )
            Node imsi;

            if(refValueMutation == rand.Next(0, 100))   // 1 % 확율 적용 
            {
                var pos1 = rand.Next(1, NodeCounts);
                int pos2;

                while (true)
                {
                    pos2 = rand.Next(1, NodeCounts);
                    if (pos1 != pos2) break;            // 서로 다른 위치의 노드를 선택
                }

                imsi = gene[pos1];
                gene[pos1] = gene[pos2];        // 두개 노드의 위치를 바꾼다. 
                gene[pos2] = imsi;
            }
        }
        //---------------------------------------------------------------------------------------------------------
        public List<Node> generateGene()
        {
            // 각 노드들의 연결 순서를 랜덤하게 배열하여 유전자 생성 

            List<Node> gene = new List<Node>();
            var used = Enumerable.Repeat<bool>(false, NodeCounts).ToArray();
            int count = 0;

            gene.Add(NodeGroup[0]);  // 처음 시작 위치는 동일 
            used[0] = true;

            while (true)
            {
                var index = rand.Next(0, NodeCounts);           // 노드들 중 임의의 순번 노드를 선택

                if (used[index] == false)                       // 사용된 노드가 아니라면 
                {
                    used[index] = true;                         // 사용되었음을 표시 
                    gene.Add(NodeGroup[index]);                 // 노드를 유전자에 추가 
                    count ++;
                }

                if (count >= NodeCounts-1) break;
            }

            return gene;
        }
        //---------------------------------------------------------------------------------------------------------
    }
}
