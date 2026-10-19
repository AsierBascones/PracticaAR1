using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTrackingGameSpawner : MonoBehaviour
{
    [Header("Componentes AR")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("Juegos")]
    [SerializeField] private GameObject snakeGamePrefab;
    [SerializeField] private string snakeMarkerName = "SnakeMarker";

    // Instancia activa para no duplicar el juego al volver a detectar la imagen
    private GameObject spawnedSnakeGame;

    private void Awake()
    {
        if (trackedImageManager == null)
            trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    private void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    private void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // 1. Nuevas imágenes detectadas por primera vez
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            CheckAndSpawnGame(trackedImage);
        }

        // 2. Imágenes cuyo estado o posición se actualiza
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateGameState(trackedImage);
        }
    }

    private void CheckAndSpawnGame(ARTrackedImage trackedImage)
    {
        if (trackedImage.referenceImage.name == snakeMarkerName && spawnedSnakeGame == null)
        {
            // Instanciar el prefab como hijo de la imagen trackeada
            spawnedSnakeGame = Instantiate(snakeGamePrefab, trackedImage.transform);
            spawnedSnakeGame.transform.localPosition = Vector3.zero;
            spawnedSnakeGame.transform.localRotation = Quaternion.identity;
        }
    }

    private void UpdateGameState(ARTrackedImage trackedImage)
    {
        if (trackedImage.referenceImage.name == snakeMarkerName && spawnedSnakeGame != null)
        {
            // Ocultar o pausar si se pierde la vista directa del marcador
            bool isTracking = trackedImage.trackingState == TrackingState.Tracking;
            spawnedSnakeGame.SetActive(isTracking);
        }
    }
}