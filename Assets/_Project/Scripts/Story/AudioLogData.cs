using UnityEngine;

namespace SubjectZero.Story
{
    [System.Serializable]
    public struct SubtitleLine
    {
        public float startTime;
        [TextArea(1, 3)] public string text;
    }

    [CreateAssetMenu(fileName = "AudioLogData", menuName = "SubjectZero/Story/Audio Log Data")]
    public class AudioLogData : ScriptableObject
    {
        public string logTitle;
        public AudioClip clip;
        public SubtitleLine[] subtitles;
    }
}