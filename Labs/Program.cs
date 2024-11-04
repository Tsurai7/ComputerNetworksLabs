using Labs;

var (senderPort, receiverPort) = Utils.ChooseSenderAndReceiver();

var station1 = new Station(senderPort.PortName, 1, true);
var station2 = new Station(receiverPort.PortName, 2, false);
var stations = new[]
{
    station1, station2
};
var tokenRingController = new TokenRingController(stations);

tokenRingController.StartRing();

while (true)
{
    Thread.Sleep(250);
    Console.WriteLine("Choose an action:");
    Console.WriteLine("1 - Transmit data");
    Console.WriteLine("2 - Change Baud Rate");
    Console.WriteLine("3 - Exit");
    Console.Write("Input: ");
    
    var choice = int.Parse(Console.ReadLine()!);

    switch (choice)
    {
        case 1:
            ActionHandlers.TransmitDataHandler(tokenRingController);
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
