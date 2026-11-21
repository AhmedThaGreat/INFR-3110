// The Command pattern turns an action (like "jump") into an object.
// Every command follows this rule:
//   Execute() - do the action
//   Undo()    - reverse it (for actions that can be reversed)
//
// Check-in feedback: "doing it every frame is a bit silly".
// Before, a MoveCommand ran EVERY FRAME while a key was held.
// Now a command runs ONCE when a key is pressed, and Undo() runs ONCE when
// the key is released - so commands are only used for real events.
public interface ICommand
{
    void Execute(); // does the action
    void Undo();    // reverses it
}
