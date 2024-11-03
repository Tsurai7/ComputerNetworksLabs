namespace Labs;

public class TokenRingController
{
    private readonly Station[] _stations;
    private int _currentTokenHolder;

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
    
    public void SendMessage(Frame dataPacket)
    {
        if (_currentTokenHolder == dataPacket.SourceAddress)
        {
            Console.WriteLine($"Станция {_stations[_currentTokenHolder].Address} отправляет сообщение на {dataPacket.DestinationAddress}");
            _stations[_currentTokenHolder].Send(dataPacket);
            PassToken();
        }
        else
        {
            Console.WriteLine($"Станция {_stations[_currentTokenHolder].Address} не может отправить сообщение. У нее нет токена.");
        }
    }

    private void OnReceivePacket(Frame packet)
    {
        if (packet.IsToken)
        {
            Console.WriteLine($"Токен получен станцией {_stations[_currentTokenHolder].Address}");

            if (ShouldSendData())
            {
                SendMessage(packet);
            }
            else
            {
                PassToken();
            }
        }
        else if (packet.DestinationAddress == _stations[_currentTokenHolder].Address)
        {
            Console.WriteLine($"Сообщение получено на станции {_stations[_currentTokenHolder].Address}: {packet.Data}");
        }
        else
        {
            _stations[_currentTokenHolder].Send(packet);
        }
    }

    private bool ShouldSendData()
    {
        return false; 
    }

    private void PassToken()
    {
        _currentTokenHolder = (_currentTokenHolder + 1) % _stations.Length;

        var tokenPacket = new Frame
        {
            IsToken = true
        };
        _stations[_currentTokenHolder].Send(tokenPacket);
    }
}

