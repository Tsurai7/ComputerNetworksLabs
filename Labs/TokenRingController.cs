namespace Labs;

public class TokenRingController
{
    private readonly Station[] _stations;
    private int _currentTokenHolder;
    private Frame? _pendingPacket;

    public TokenRingController(Station[] stations)
    {
        _stations = stations;
        _currentTokenHolder = 0;

        foreach (var station in _stations)
        {
            station.OnReceivePacket += OnReceivePacket;
        }
    }

    public void StartRing()
    {
        foreach (var station in _stations)
        {
            station.Start();
        }
        
        PassToken();
    }
    
    public void QueueMessage(Frame dataPacket)
    {
        _pendingPacket = dataPacket;
        Console.WriteLine("Сообщение добавлено в очередь, ожидает получения токена.");
    }

    private void OnReceivePacket(Frame packet)
    {
        if (packet.IsToken)
        {
            // Console.WriteLine($"Токен получен станцией {_stations[_currentTokenHolder].Address}");

            if (_pendingPacket != null && _pendingPacket.SourceAddress == _stations[_currentTokenHolder].Address)
            {
                Console.WriteLine($"Станция {_stations[_currentTokenHolder].Address} отправляет сообщение на {_pendingPacket.DestinationAddress}");
                _stations[_currentTokenHolder].Send(_pendingPacket);
                _pendingPacket = null;
            }

            PassToken();
        }
        else if (packet.DestinationAddress == _stations[_currentTokenHolder].Address)
        {
            Console.WriteLine($"Сообщение получено на станции {_stations[_currentTokenHolder].Address}: {packet.Data}");
            
            packet.DisplayFrameStructure();
        }
        else
        {
            _stations[_currentTokenHolder].Send(packet);
        }
    }

    private void PassToken()
    {
        Thread.Sleep(600);

        _currentTokenHolder = (_currentTokenHolder + 1) % _stations.Length;

        var tokenPacket = new Frame
        {
            IsToken = true
        };
        //Console.WriteLine($"Передача токена от станции {_stations[_currentTokenHolder].Address}.");

        _stations[_currentTokenHolder].Send(tokenPacket);
    }
}
