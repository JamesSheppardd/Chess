using System.Collections;
using UnityEditor;
using UnityEngine;
using Chess.GUI;

[CustomEditor(typeof(BoardGUI))]
public class ShowBoard : Editor
{
    public override void OnInspectorGUI(){
        DrawDefaultInspector();

        BoardGUI script = (BoardGUI)target;
        if(GUILayout.Button("Toggle Board Representation")){
            script.showBoardRep = !script.showBoardRep;
        }
        // attack tables
        if(GUILayout.Button("Toggle White Attack Tables")){
            script.showWhiteAttacks = !script.showWhiteAttacks;
            script.ClearColours();
            script.ShowAttackedSquares();
        }
        if(GUILayout.Button("Toggle Black Attack Tables")){
            script.showBlackAttacks = !script.showBlackAttacks;
            script.ClearColours();
            script.ShowAttackedSquares();
        }
        
        
    }
}
