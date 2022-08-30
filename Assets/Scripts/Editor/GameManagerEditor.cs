using System.Collections;
using UnityEditor;
using UnityEngine;
using Chess;

[CustomEditor(typeof(GameManager)), CanEditMultipleObjects]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI(){
        DrawDefaultInspector();

        GameManager script = (GameManager)target;
        if(GUILayout.Button("Start New Game")){
            script.NewGame(script.FEN);
            DebugTools.Print(script.FEN);
        }
        if(GUILayout.Button("Remove Board")){
            script.EmptyBoard();
        }
        
    }
}
