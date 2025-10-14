using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "DrawingData", menuName = "Scriptable Objects/DrawingData")]
public class DrawingData : ScriptableObject
{
    public string Title;
    [TextArea]
    public string Description;
    public Sprite Drawing;
}
