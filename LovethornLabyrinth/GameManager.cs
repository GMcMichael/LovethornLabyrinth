namespace LovethornLabyrinth
{
    public class GameManager
    {
        private bool _isPaused = false;
        private bool _isRunning = false;

        public static GameManager Instance = new();
        public GameManager() { }
        public void StartGame()
        {
            if (_isRunning) return;
            _isRunning = true;
            Task.Run(async () =>
            {
                while (_isRunning)
                {
                    if (_isPaused) await Task.Yield();
                    HandleGame();
                }
            });
        }
        public void StopGame() { _isRunning = false; }

        public static void Log(string msg)
        {

        }

        private void HandleGame()
        {
            //Do game stuff
            Log("Game running ...");
        }
        public void PassInput(string input)
        {
            if (!_isRunning) return;
            //pass to list or something
            Log($"Passing input: {input}");
        }
    }
}
