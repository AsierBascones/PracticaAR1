using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private Transform boardTransform; // Referencia a SnakePlane

    [Header("Dimensiones de la cuadrícula")]
    [Tooltip("Número de celdas desde el centro hacia cada borde en X y Z")]
    [SerializeField] private int gridRadius = 4; // Si la cuadrícula es de 10x10, usa 4 o 5
    [SerializeField] private float stepSize = 0.05f;
    [SerializeField] private float yOffset = 0.02f;

    private GameObject currentFood;

    private void Start()
    {
        SpawnFood();
    }

    public void SpawnFood()
    {
        if (foodPrefab == null) return;

        // Si ya hay comida, la destruimos antes de reaparecer
        if (currentFood != null)
        {
            Destroy(currentFood);
        }

        // Elegir casilla aleatoria en la cuadrícula local
        int randomX = Random.Range(-gridRadius, gridRadius + 1);
        int randomZ = Random.Range(-gridRadius, gridRadius + 1);

        Vector3 spawnLocalPos = new Vector3(randomX * stepSize, yOffset, randomZ * stepSize);

        // Instanciar como hijo del Spawner/Tablero para mantener la posición local relativa al marcador
        currentFood = Instantiate(foodPrefab, transform);
        currentFood.transform.localPosition = spawnLocalPos;
        currentFood.transform.localRotation = Quaternion.identity;
    }
}