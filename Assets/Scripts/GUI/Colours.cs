
using UnityEngine;
[CreateAssetMenu(fileName = "Colours", menuName = "Chess_Engine_V1/Colours", order = 0)]
public class Colours : ScriptableObject {
    // board
    public Color dark;
    public Color light;

    // highlights
    public Color selectedDark;
    public Color selectedLight;
    public Color newestMove;
    public Color validMoves;
    public Color checkingPiece;
    public Color whiteAttackedSquare;
    public Color blackAttackedSquare;
}

