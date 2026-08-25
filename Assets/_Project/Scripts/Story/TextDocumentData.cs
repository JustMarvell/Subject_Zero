using UnityEngine;

namespace SubjectZero.Story
{
    [CreateAssetMenu(fileName = "TextDocumentData", menuName = "SubjectZero/Story/Text Document Data")]
    public class TextDocumentData : ScriptableObject
    {
        public string documentTitle;
        [TextArea(10, 30)] public string bodyText;
    }
}