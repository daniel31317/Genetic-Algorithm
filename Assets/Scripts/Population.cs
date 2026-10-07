using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Population : MonoBehaviour
{
    [SerializeField] private int initialPopulation = 100;
    [SerializeField] private GameObject individualPrefab;

    private SpriteRenderer environmentSpriteRenderer;

    private List<Individual> population = new List<Individual>();


    private void Awake()
    {
        for(int i = 0; i < initialPopulation; i++)
        {
            GameObject individualObject = Instantiate(individualPrefab);
            individualObject.GetComponent<Individual>().InitialiseIndividual();
            population.Add(individualObject.GetComponent<Individual>());
        }

        environmentSpriteRenderer = GetComponent<SpriteRenderer>();

        SetPopulationPositions();

        StartCoroutine(GeneticAlgorithm());
    }


    private void SetPopulationPositions()
    {
        float xPos = -67.5f;
        float yPos = 67.5f;
        
        for(int i = 0; i < population.Count; i++)
        {
            population[i].transform.position = new Vector3(xPos, yPos, 0);
            xPos += 15f;
            if(xPos > 67.5f)
            {
                xPos = -67.5f;
                yPos -= 15f;
            }
        }
    }


    private void GeneticAlgorithmLoop()
    {
        DetermineFitnessOfPopulation();
        SortPopulation();
        KillUnfitPopulation();
        RepopulatePopulation();
        SetPopulationPositions();
    }



    private void DetermineFitnessOfPopulation()
    {
        for (int i = 0; i < population.Count; i++)
        {
            population[i].SetFitness(CalculateFitness(population[i]));
        }
    }


    private float CalculateFitness(Individual individual)
    {
        Color targetColour = environmentSpriteRenderer.color;
        Color individualColour = individual.GetColour();

        Vector3 targetVector = new Vector3(targetColour.r, targetColour.g, targetColour.b); 
        Vector3 individualVector = new Vector3(individualColour.r, individualColour.g, individualColour.b); 

        float fitness = (targetVector - individualVector).magnitude;    

        return fitness;
    }


    private void SortPopulation()
    {
        population.Sort(

            delegate(Individual a, Individual b)
            {
                if(a.GetFitness() < b.GetFitness())
                {
                    return -1;
                }
                else if(a.GetFitness() > b.GetFitness())
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

        );
    }


    private void KillUnfitPopulation()
    {
        int halfPopulation = population.Count / 2;
        for(int i = halfPopulation; i < population.Count; i++)
        {
            Destroy(population[i].gameObject);
        }
        population.RemoveRange(halfPopulation, halfPopulation);
    }


    private void RepopulatePopulation()
    {
        List<Individual> newPopulation = new List<Individual>();

        for (int i = 1; i < population.Count; i+=2)
        {
            Individual parent1 = population[i - 1];
            Individual parent2 = population[i];

            Color parentColour1 = parent1.GetColour();
            Color parentColour2 = parent2.GetColour();

            float geneticFactor = Random.Range(0f, 1f);

            Color newColour1 = new Color();
            Color newColour2= new Color();


            if(geneticFactor < 0.16f)
            {
                newColour1 = new Color(parentColour1.r, parentColour1.g, parentColour2.b);
                newColour2 = new Color(parentColour2.r, parentColour2.g, parentColour1.b);
            }
            else if(geneticFactor < 0.32f)
            {
                newColour1 = new Color(parentColour1.r, parentColour2.g, parentColour2.b);
                newColour2 = new Color(parentColour2.r, parentColour1.g, parentColour1.b);
            }
            else if(geneticFactor < 0.48f)
            {
                newColour1 = new Color(parentColour1.r, parentColour2.g, parentColour1.b);
                newColour2 = new Color(parentColour2.r, parentColour1.g, parentColour2.b);
            }
            else if(geneticFactor < 0.64f)
            {
                newColour1 = new Color(parentColour2.r, parentColour1.g, parentColour2.b);
                newColour2 = new Color(parentColour1.r, parentColour2.g, parentColour1.b);
            }
            else if(geneticFactor < 0.80f)
            {
                newColour1 = new Color(parentColour2.r, parentColour1.g, parentColour1.b); 
                newColour2 = new Color(parentColour1.r, parentColour2.g, parentColour2.b);
            }
            else
            {
                newColour1 = new Color(parentColour2.r, parentColour2.g, parentColour1.b); 
                newColour2 = new Color(parentColour1.r, parentColour1.g, parentColour2.b);
            }

            GameObject newIndividualObject1 = Instantiate(individualPrefab);
            GameObject newIndividualObject2 = Instantiate(individualPrefab);

            Individual newIndividual1 = newIndividualObject1.GetComponent<Individual>();
            Individual newIndividual2 = newIndividualObject2.GetComponent<Individual>();


            //Mutate
            newIndividual1.SetColour(Mutate(newColour1));
            newIndividual2.SetColour(Mutate(newColour2));

            newPopulation.Add(newIndividual1);
            newPopulation.Add(newIndividual2);
        }


        for (int i = 0; i < newPopulation.Count; i++)
        {
            population.Add(newPopulation[i]);
        }
    }


    private Color Mutate(Color colour)
    {
        float mutationRate = 0.1f;

        Vector3 mutatedColour = new Vector3(colour.r, colour.g, colour.b);

        for(int i = 0; i < 3; i++)
        {
            if (Random.Range(0.0f, 1.0f) < mutationRate)
            {
                mutatedColour[i] = Random.Range(0.0f, 1.0f);
            }
        }

      
        return new Color(mutatedColour.x, mutatedColour.y, mutatedColour.z);
    }



    IEnumerator GeneticAlgorithm()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);
            GeneticAlgorithmLoop();
        }
    }


}
