using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SnakeMovement : MonoBehaviour
{
    [Header("Configuración del Tablero")]
    [Tooltip("Distancia en metros entre casillas (el ancho de un cuadrado verde)")]
    [SerializeField] private float stepSize = 0.05f;
    [SerializeField] private float moveInterval = 0.35f;

    [Header("Dirección Inicial")]
    [SerializeField] private Vector3 currentDirection = Vector3.forward;
    private Vector3 nextDirection = Vector3.forward;

    [Header("Cuerpo")]
    [SerializeField] private GameObject bodySegmentPrefab;
    private readonly List<Transform> bodyParts = new List<Transform>();
    private readonly List<Vector3> previousPositions = new List<Vector3>();

    private float timer;

    private void Start()
    {
        // Registrar la cabeza como primer elemento
        bodyParts.Add(transform);
    }

    private void Update()
    {
        HandleKeyboardInput();

        timer += Time.deltaTime;
        if (timer >= moveInterval)
        {
            timer = 0f;
            MoveStep();
        }
    }

    // Controles por teclado para probar cómodamente en el simulador del editor
    private void HandleKeyboardInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return; // Si no hay teclado conectado

        if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
            SetDirection(Vector3.forward);
        else if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)
            SetDirection(Vector3.back);
        else if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)
            SetDirection(Vector3.left);
        else if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame)
            SetDirection(Vector3.right);
    }

    // Métodos públicos para enlazar con botones de UI (EventTrigger o OnClick)
    public void SetDirection(Vector3 newDir)
    {
        // Evitar giro de 180 grados en sentido contrario
        if (newDir != -currentDirection)
        {
            nextDirection = newDir;
        }
    }

    private void MoveStep()
    {
        currentDirection = nextDirection;

        // Guardar posiciones previas para que el cuerpo las herede
        previousPositions.Clear();
        for (int i = 0; i < bodyParts.Count; i++)
        {
            previousPositions.Add(bodyParts[i].localPosition);
        }

        // 1. Mover la cabeza en coordenadas locales al plano
        transform.localPosition += currentDirection * stepSize;

        // Orientar la cabeza hacia la dirección de avance
        if (currentDirection != Vector3.zero)
        {
            transform.localRotation = Quaternion.LookRotation(currentDirection, Vector3.up);
        }

        // 2. Mover cada parte del cuerpo al hueco que ocupaba la anterior
        for (int i = 1; i < bodyParts.Count; i++)
        {
            bodyParts[i].localPosition = previousPositions[i - 1];
        }
    }

    public void Grow()
    {
        if (bodySegmentPrefab == null) return;

        Vector3 spawnPos = previousPositions.Count > 0 ? previousPositions[previousPositions.Count - 1] : transform.localPosition;
        GameObject segment = Instantiate(bodySegmentPrefab, transform.parent);
        segment.transform.localPosition = spawnPos;
        segment.transform.localRotation = Quaternion.identity;
        bodyParts.Add(segment.transform);
    }
    private void OnTriggerEnter(Collider other)
    {
        // Al tocar la comida
        if (other.GetComponent<FoodSpawner>() != null || other.name.Contains("FoodItem"))
        {
            Grow();

            // Pedir al FoodSpawner que coloque otra comida
            FoodSpawner spawner = GetComponentInParent<FoodSpawner>();
            if (spawner == null)
            {
                spawner = Object.FindFirstObjectByType<FoodSpawner>();
            }

            if (spawner != null)
            {
                spawner.SpawnFood();
            }
        }
    }
}