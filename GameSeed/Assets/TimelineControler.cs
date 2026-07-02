using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class TimelineControler : MonoBehaviour
{
    public float speedMultiplier = 0.5f;

    private PlayableDirector director;

    void Awake()
    {
        director = GetComponent<PlayableDirector>();
    }

    void Start()
    {
        if (director != null && director.playableGraph.IsValid())
        {
            director.playableGraph.GetRootPlayable(0).SetSpeed(speedMultiplier);
        }
    }
}