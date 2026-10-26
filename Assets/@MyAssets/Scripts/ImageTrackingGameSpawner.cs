using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTrackingGameSpawner : MonoBehaviour
{
    public ARTrackedImageManager imageManager;

    [Header("Juegos asignados a cada marcador")]
    public GameObject gameSnake;     // SnakeGame_Prefab
    public GameObject gameDino;      // Prefab o contenido del marcador 2
    public GameObject gamePinball;   // Prefab o contenido del marcador 3

    [Header("Música / Audios de cada juego")]
    public AudioSource bgmSnake;
    public AudioSource bgmDino;
    public AudioSource bgmPinball;

    void OnEnable() => imageManager.trackablesChanged.AddListener(OnChanged);
    void OnDisable() => imageManager.trackablesChanged.RemoveListener(OnChanged);

    void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var img in eventArgs.added) UpdateGame(img);
        foreach (var img in eventArgs.updated) UpdateGame(img);
    }

    void UpdateGame(ARTrackedImage img)
    {
        bool isTracking = (img.trackingState == TrackingState.Tracking);
        string name = img.referenceImage.name;

        if (name == "SnakeMarker")
        {
            PositionGame(gameSnake, img, isTracking);
            ManageAudio(bgmSnake, isTracking);
        }
        else if (name == "DinoMarker")
        {
            PositionGame(gameDino, img, isTracking);
            ManageAudio(bgmDino, isTracking);
        }
        else if (name == "SparrowMarker")
        {
            PositionGame(gamePinball, img, isTracking);
            ManageAudio(bgmPinball, isTracking);
        }
    }
    void PositionGame(GameObject game, ARTrackedImage img, bool active)
    {
        if (game == null) return;

        game.transform.SetParent(img.transform);
        // 0.005f levanta el minijuego 5 milímetros respecto al papel/mesa
        game.transform.localPosition = new Vector3(0f, 0.005f, 0f);
        game.transform.localRotation = Quaternion.identity;
        game.transform.localScale = Vector3.one;

        game.SetActive(active);
    }

    void ManageAudio(AudioSource src, bool play)
    {
        if (src == null) return;
        if (play && !src.isPlaying) src.Play();
        else if (!play && src.isPlaying) src.Pause();
    }
}