using Taskr;

var store = new JsonTaskStore(JsonTaskStore.DefaultPath());
return new App(store, Console.Out, Console.Error).Run(args);
