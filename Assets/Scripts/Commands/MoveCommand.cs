// A command that starts the player walking in one direction.
// We make four of these: forward, back, left and right.
//
// Execute() runs once when the key goes DOWN  -> start walking that way
// Undo()    runs once when the key comes UP   -> stop walking that way
// Holding W and D together just adds both directions = a diagonal.
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
        player.AddMoveDirection(x, z);
    }

    public void Undo()
    {
        player.AddMoveDirection(-x, -z);
    }
}
