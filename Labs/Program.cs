using Labs;

var (senderPort, receiverPort) = Utils.ChooseSenderAndReceiver();

var channel = new MonoChannel(senderPort, receiverPort);

while (true)
{
    Console.WriteLine("Choose an action:");
    Console.WriteLine("1 - Transmit data");
    Console.WriteLine("2 - Change Baud Rate");
    Console.WriteLine("3 - Exit");
    Console.Write("Input: ");
    
    var choice = int.Parse(Console.ReadLine()!);

    switch (choice)
    {
        case 1:
            ActionHandlers.TransmitDataHandler(channel);
            break;
        case 2:
            ActionHandlers.ChangeBaudRateHandler(senderPort, receiverPort);
            break;
        case 3:
            ActionHandlers.ExitProgramHandler(senderPort, receiverPort);
            return;
        default:
            Console.WriteLine("Invalid input, please try again.");
            break;
    }
}