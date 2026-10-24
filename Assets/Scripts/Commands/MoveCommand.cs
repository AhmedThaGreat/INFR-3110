// A command that walks the player in one fixed direction.
// We make four of these: forward, back, left and right.
public class MoveCommand : ICommand
{
    private PlayerMovement player; // who to move
    private float x;               // left/right amount
    private float z;               // forward/back amount

    public MoveCommand(PlayerMovement player, float x, float z)
    {
        this.player = player;
        this.x = x;
        this.z = z;
    }

    public void Execute()
    {
        player.Move(x, z);
    }
}
