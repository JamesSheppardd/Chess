namespace Chess.Audio
{
    using UnityEngine;
    
    public class AudioManager : MonoBehaviour {
        public AudioSource makeMove, takePiece;

        public void PlayMoveSound(){
            makeMove.Play();
        }
        public void PlayPieceTakeSound(){
            takePiece.Play();
        }
    }
}