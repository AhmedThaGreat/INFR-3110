// A command that makes the player jump.
// A jump can't be taken back once it starts, so Undo() does nothing.
public class JumpCommand : ICommand
{
    private PlayerMovement player; // who should jump

    public JumpCommand(PlayerMovement player)
    {
        this.player = player;
    }

    public void Execute()
    {
        player.Jump();
    }

    public void Undo() { }
}
