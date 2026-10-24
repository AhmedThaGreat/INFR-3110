// A command that makes the player jump.
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
}