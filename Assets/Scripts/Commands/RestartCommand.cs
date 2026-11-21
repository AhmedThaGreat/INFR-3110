// A command that restarts the level once the game is won or lost.
// Same ICommand rule as MoveCommand and JumpCommand, so the InputHandler
// can link it to a key without knowing what it does.
public class RestartCommand : ICommand
{
    public void Execute()
    {
        GameManager manager = GameManager.Instance;
        if (manager != null && manager.State != GameState.Playing)
        {
            manager.Restart();
        }
    }

    public void Undo() { }   // a restart can't be undone
}
