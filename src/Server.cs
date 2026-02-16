using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// TODO: Uncomment the code below to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var connection = server.AcceptSocket(); // wait for client
var okResponse = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
var notFoundResponse = Encoding.ASCII.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n");
byte[] outBytes = new byte[1024];
int numberOfBytes = connection.Receive(outBytes);

while (true)
{
    string query = Encoding.ASCII.GetString(outBytes, 0, numberOfBytes);
    if (!query.StartsWith("GET"))
    {
        connection.Send(notFoundResponse);
    }
    else
    {
        connection.Send(okResponse);
        // int startingIndex = query.IndexOf("/");
        // int endingIndex = query.IndexOf(".");
        // string requestTarget =  query.Substring(startingIndex, endingIndex - startingIndex);
        // Console.WriteLine(requestTarget);
    }
}
//GET /index.html HTTP/1.1\r\nHost: localhost:4221\r\nUser-Agent: curl/7.64.1\r\nAccept: */*\r\n\r\n




connection.Close();