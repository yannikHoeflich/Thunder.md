namespace Thunder.Md.Extensions;

public interface ICommand{
    public string Name{ get; }
    public string Help{ get; }

    public void Execute(string[] args, ExtensionArgs extensionArgs);
}