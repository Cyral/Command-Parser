#  <center>[Command Parser](http://www.pyratron.com)</center>
#### <center>Pyratron Command Parser Framework</center>
<center>[![Build Status](https://travis-ci.org/Pyratron/Command-Parser.svg?branch=master)](https://travis-ci.org/Pyratron/Command-Parser)</center>

A simple, lightweight, but powerful command parsing library for use in games and other applications making use of user input commands.

The [demo application](https://github.com/Pyratron/Command-Parser/archive/master.zip) contains a short tutorial and a variety of example commands to get you started.

Documentation will be created when the final library is released.

###Example:

```csharp
Parser = CommandParser.CreateNew().UsePrefix(string.Empty).OnError(OnParseError);

Parser.AddCommand(Command
  .Create("Ban User") //Friendly Name.
  .AddAlias("ban") //Aliases.
  .AddAlias("banuser")
  .SetDescription("Bans a user from the server.") //Description.
  //Action to be executed when command is ran with correct parameters. (Of course, can be method, lamba, delegate, etc)
  .SetAction(OnBanExecuted)
  //Precondition to be checked before executing the command.
  .SetExecutePredicate(delegate
  {
      if (banned)
          return "You are already banned!";
      return string.Empty;
  })
  .AddArgument(Argument //Add an argument.
      .Create("User")));
```

...

```csharp
private static void OnBanExecuted(Argument[] args)
{
    var user = args.FromName("user");
    Console.WriteLine("User {0} was banned!", user);
    banned = true;
}

private static void OnParseError(object sender, string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(message);
    Console.ForegroundColor = ConsoleColor.Gray;
}
```

...

```csharp
Parser.Parse("ban someone");
```
