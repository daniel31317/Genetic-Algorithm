using Mono.Cecil;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.LudiqRootObjectEditor;

public class Population : MonoBehaviour
{
    [Header("Population Settings")]
    [SerializeField] private int initialPopulation = 100;
    [SerializeField] private float mutationChance = 10;

    [Header("Cosmetical Stuff")]
    [SerializeField] private GameObject individualPrefab;
    [SerializeField] private TMP_Text generationText;
    [SerializeField] private TMP_Text correctPopulationText;

    private SpriteRenderer environmentSpriteRenderer;


    private int correctPopulationCount = 0;
    private int generationCount = 1;

    private List<Individual> population = new List<Individual>();

    private Vector2 populationStartingPos = new Vector2();
    private float populationSpacing = 0f;

    private float individualScale = 0f;


    private void Awake()
    {
        environmentSpriteRenderer = GetComponent<SpriteRenderer>();

        SetEnvironment();

        for (int i = 0; i < initialPopulation; i++)
        {
            GameObject individualObject = Instantiate(individualPrefab);
            Individual individual = individualObject.GetComponent<Individual>();
            individual.InitialiseIndividual();
            individual.SetScale(individualScale);
            population.Add(individual);
        }



        SetPopulationPositions();

        StartCoroutine(GeneticAlgorithm());
    }

    private void SetEnvironment()
    {
        individualScale = Mathf.Sqrt(initialPopulation);

        float scale = initialPopulation + (individualScale / 2f * (individualScale - 1f));

        transform.localScale = new Vector3(scale, scale, 1f);

        Camera.main.orthographicSize = scale / 2f;

        populationStartingPos = new Vector2((-scale + individualScale) / 2f, (scale - individualScale) / 2f);

        populationSpacing = individualScale + (individualScale / 2f);
    }


    private void SetPopulationPositions()
    {
        float xPos = populationStartingPos.x;
        float yPos = populationStartingPos.y;
        
        for(int i = 0; i < population.Count; i++)
        {
            population[i].transform.position = new Vector3(xPos, yPos, 0);
            xPos += populationSpacing;
            if(xPos > -populationStartingPos.x)
            {
                xPos = populationStartingPos.x;
                yPos -= populationSpacing;
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
        correctPopulationCount = 0;
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

        if(fitness < 0.005f)
        {
            correctPopulationCount++;
        }

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
        if(population.Count % 2 != 0)
        {
            halfPopulation++;
        }

        for (int i = halfPopulation; i < population.Count; i++)
        {
            Destroy(population[i].gameObject);
        }
        population.RemoveRange(halfPopulation, population.Count - halfPopulation);
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

            newIndividual1.SetScale(individualScale);
            newIndividual2.SetScale(individualScale);

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
        float mutationRate = mutationChance / 100f;

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
            generationText.text = "Generation: " + generationCount;
            correctPopulationText.text = "Correct Population: " + correctPopulationCount;
            yield return new WaitForSeconds(1f);
            GeneticAlgorithmLoop();
            generationCount++;
        }
    }


}
