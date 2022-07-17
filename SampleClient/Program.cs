// See https://aka.ms/new-console-template for more information

var client = new SampleClient.Client();
client.Start("127.0.0.1", 3000);

while(true)
{
    var line = Console.ReadLine();
    client.Send(line);
}
