using UnityEngine;

public class Individual : MonoBehaviour
{
    private float fitness = 1.0f;
    private Color color = new Color();
    private SpriteRenderer spriteRenderer;


    public void InitialiseIndividual()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        color = new Color(Random.value, Random.value, Random.value);
        spriteRenderer.color = color; 
    }


    public void SetColour(Color newColor)
    {
        if(spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        color = newColor;
        spriteRenderer.color = color;
    }

    public Color GetColour()
    {
        return color;
    }


    public float GetFitness()
    {
        return fitness;
    }   

    public void SetFitness(float newFitness)
    {
        fitness = newFitness;
    }
}
