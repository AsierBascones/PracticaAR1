using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SnakeGameFull : MonoBehaviour
{
    [Header("Configuración de Cuadrícula (8x8)")]
    public float stepSize = 0.03f;       // Casillas de 3 cm
    public float moveInterval = 0.25f;
    public int gridLimit = 3;            // De -3 a +3 (área de 8x8)
    public int manzanasParaGanar = 5;

    [Header("Ajustes de la Moneda")]
    public Vector3 escalaMoneda = new Vector3(0.12f, 0.12f, 0.12f);
    public float alturaMonedaY = 0.088f;

    [Header("Referencias")]
    public GameObject foodObject;       // Objeto FoodItem (moneda)
    public GameObject bodyPrefab;       // BodySegmentPrefab
    public TextMeshProUGUI scoreText;   // ScoreText del Canvas
    public AudioSource audioSource;
    public AudioClip eatSound;
    public AudioClip sonidoVictoria;    // Clip que sonará al ganar

    private Vector3 direction = Vector3.forward;
    private Vector3 nextDirection = Vector3.forward;
    private List<Transform> bodyParts = new List<Transform>();
    private float timer = 0f;
    private int score = 0;
    private bool juegoTerminado = false;

    void Awake()
    {
        if (foodObject == null && transform.parent != null)
        {
            Transform f = transform.parent.Find("FoodItem");
            if (f != null) foodObject = f.gameObject;
        }
    }

    void Start()
    {
        IniciarJuego();
    }

    void OnEnable()
    {
        if (foodObject != null)
        {
            ColocarManzanaAleatoria();
        }
    }

    public void IniciarJuego()
    {
        juegoTerminado = false;
        score = 0;
        timer = 0f;
        direction = Vector3.forward;
        nextDirection = Vector3.forward;

        // Limpiar eslabones previos si se reinicia
        for (int i = 1; i < bodyParts.Count; i++)
        {
            if (bodyParts[i] != null) Destroy(bodyParts[i].gameObject);
        }
        bodyParts.Clear();
        bodyParts.Add(transform);

        // Centrar la cabeza
        transform.localPosition = new Vector3(0f, 0.01f, 0f);
        ActualizarRotacionVisual();

        // Configurar la comida existente
        if (foodObject != null)
        {
            // Si el objeto asignado es un prefab de la carpeta de Assets y no de la escena, creamos una instancia
            if (foodObject.scene.rootCount == 0)
            {
                foodObject = Instantiate(foodObject, transform.parent);
                foodObject.name = "FoodItem";
            }
            else if (foodObject.transform.parent != transform.parent)
            {
                foodObject.transform.SetParent(transform.parent);
            }

            foodObject.SetActive(true);
            foodObject.transform.localScale = escalaMoneda;

            Rigidbody rb = foodObject.GetComponent<Rigidbody>();
            if (rb != null) Destroy(rb);

            Collider col = foodObject.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        ColocarManzanaAleatoria();
        ActualizarTextoUI();
    }

    void Update()
    {
        if (juegoTerminado) return;

        timer += Time.deltaTime;
        if (timer >= moveInterval)
        {
            timer = 0f;
            AvanzarPaso();
        }
    }

    public void ChangeDirectionUp() { if (direction != Vector3.back) nextDirection = Vector3.forward; }
    public void ChangeDirectionDown() { if (direction != Vector3.forward) nextDirection = Vector3.back; }
    public void ChangeDirectionLeft() { if (direction != Vector3.right) nextDirection = Vector3.left; }
    public void ChangeDirectionRight() { if (direction != Vector3.left) nextDirection = Vector3.right; }

    void AvanzarPaso()
    {
        direction = nextDirection;
        ActualizarRotacionVisual();

        Vector3 prevHeadPos = transform.localPosition;
        Vector3 newHeadPos = prevHeadPos + direction * stepSize;

        // Choque contra bordes del tablero
        float limiteMetros = (gridLimit * stepSize) + 0.005f;
        if (Mathf.Abs(newHeadPos.x) > limiteMetros || Mathf.Abs(newHeadPos.z) > limiteMetros)
        {
            return;
        }

        transform.localPosition = newHeadPos;

        // Mover eslabones del cuerpo
        Vector3 previousPartPos = prevHeadPos;
        for (int i = 1; i < bodyParts.Count; i++)
        {
            Vector3 currentPartPos = bodyParts[i].localPosition;
            bodyParts[i].localPosition = previousPartPos;
            previousPartPos = currentPartPos;
        }

        // Comprobación de recogida por distancia en X-Z
        if (foodObject != null && foodObject.activeSelf)
        {
            Vector2 headPos2D = new Vector2(transform.localPosition.x, transform.localPosition.z);
            Vector2 foodPos2D = new Vector2(foodObject.transform.localPosition.x, foodObject.transform.localPosition.z);

            if (Vector2.Distance(headPos2D, foodPos2D) < (stepSize * 0.8f))
            {
                ComerManzana();
            }
        }
    }

    void ActualizarRotacionVisual()
    {
        if (direction == Vector3.forward) transform.localRotation = Quaternion.Euler(0, 0, 0);
        else if (direction == Vector3.back) transform.localRotation = Quaternion.Euler(0, 180, 0);
        else if (direction == Vector3.left) transform.localRotation = Quaternion.Euler(0, 270, 0);
        else if (direction == Vector3.right) transform.localRotation = Quaternion.Euler(0, 90, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (juegoTerminado) return;

        if (foodObject != null && (other.gameObject == foodObject || other.transform.IsChildOf(foodObject.transform)))
        {
            ComerManzana();
        }
    }

    void ComerManzana()
    {
        score++;

        // Crecer
        if (bodyPrefab != null)
        {
            Vector3 spawnPos = bodyParts[bodyParts.Count - 1].localPosition;
            GameObject nuevoSegmento = Instantiate(bodyPrefab, transform.parent);
            nuevoSegmento.transform.localPosition = spawnPos;
            nuevoSegmento.transform.localRotation = Quaternion.identity;
            nuevoSegmento.transform.localScale = transform.localScale;
            bodyParts.Add(nuevoSegmento.transform);
        }

        // Comprobar si se ha alcanzado la meta
        if (score >= manzanasParaGanar)
        {
            juegoTerminado = true;
            if (foodObject != null) foodObject.SetActive(false);
            if (scoreText != null) scoreText.text = "¡Victoria! Puntos: " + score;

            // Reproducir sonido de victoria
            if (audioSource != null && sonidoVictoria != null)
            {
                audioSource.PlayOneShot(sonidoVictoria);
            }
            return;
        }

        // Si aún no se gana, reproducir el sonido normal de comer
        if (audioSource != null && eatSound != null)
        {
            audioSource.PlayOneShot(eatSound);
        }

        ActualizarTextoUI();
        ColocarManzanaAleatoria();
    }

    public void ColocarManzanaAleatoria()
    {
        if (foodObject == null) return;

        foodObject.SetActive(true);
        foodObject.transform.localScale = escalaMoneda;

        int rx = Random.Range(-gridLimit, gridLimit + 1);
        int rz = Random.Range(-gridLimit, gridLimit + 1);

        foodObject.transform.localPosition = new Vector3(rx * stepSize, alturaMonedaY, rz * stepSize);
    }

    void ActualizarTextoUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Puntos: " + score + " / " + manzanasParaGanar;
        }
    }
}