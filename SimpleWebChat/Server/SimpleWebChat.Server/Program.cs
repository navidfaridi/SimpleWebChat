using System.Collections.Concurrent;
using WebSocketSharp;
using WebSocketSharp.Server;


namespace SimpleWebChat.Server
{
    public class Chat : WebSocketBehavior
    {
        private static readonly ConcurrentDictionary<string, Chat> ConnectedUsers = new ConcurrentDictionary<string, Chat>();

        private string _userId;

        protected override void OnOpen()
        {
            _userId = Context.QueryString["userId"];
            ConnectedUsers.TryAdd(_userId, this);

            Console.WriteLine($"User {_userId} connected");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine($"Received message from {_userId}: {e.Data}");

            // Parse the message
            var message = e.Data.Split('|');
            var receiverId = message[0];
            var text = message[1];

            // Find the receiver user
            if (ConnectedUsers.TryGetValue(receiverId, out var receiverUser))
            {
                // Send the message to the receiver user
                receiverUser.Send($"{_userId}: {text}");
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            ConnectedUsers.TryRemove(_userId, out _);

            Console.WriteLine($"User {_userId} disconnected");
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var wssv = new WebSocketServer("ws://localhost:8080");
            wssv.AddWebSocketService<Chat>("/chat");
            wssv.Start();

            Console.WriteLine("WebSocket server is running on ws://localhost:8080");
            Console.ReadLine();

            wssv.Stop();
        }
    }
}