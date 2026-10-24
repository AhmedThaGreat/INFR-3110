// The Command pattern turns an action (like "jump") into an object.
// Anything that can be done in the game and run with Execute() is a "command".
// This interface is the rule every command follows.
public interface ICommand
{
    void Execute(); // Does the action
}
