namespace Chess
{
    public static class Positioning {
        public static int[] GetGamePositionFromIndex(int _index){
            // takes in the index ranging from 21 (bottom left) to 98 (top right)
            // and returns 2 ints, for coords, of the game position this corresponds 
            // to in form [file, rank].

            int[] coords = new int[] {GetFileFromIndex(_index), GetRankFromIndex(_index)};
            return coords;
        }

        public static int GetIndexFromGamePosition(int[] _gp){
            // takes in a GamePosition [file, rank] and returns the array index 
            // for the board[] array

            int lastDigit = _gp[0] + 1;
            int firstDigit = _gp[1] + 2;
            int index = int.Parse($"{firstDigit}{lastDigit}");  // ok fix this up like seriously this is shambolick future James have a good time :/
            return index;
        }

        public static int GetIndexAs64FromIndex(int _index){
            int file = GetFileFromIndex(_index);
            int rank = GetRankFromIndex(_index);
            return rank * 8 + file; 
        }
        public static int GetIndexFromIndexAs64(int _indexAs64){
            int file = GetFileFromIndex(_indexAs64);
            int rank = GetRankFromIndex(_indexAs64);
            return _indexAs64 != 0 ? (_indexAs64 % 8) + ((_indexAs64 / 8) * 10) + 21 : 21; 
        }

        public static int GetFileFromIndex(int _index){
            // takes in array index (see above for range) and returns what file
            // (column) it is in
            return _index % 10 - 1;
        }

        public static int GetRankFromIndex(int _index){
            // takes in array index (see above for range) and returns what rank
            // (row) it is in
            return (int)(_index / 10) - 2;
        }
    }
}