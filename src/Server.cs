using System.Net;
using System.Net.Sockets;
using System.Text;

byte[] StringToByteArray(string input)
{
    return Encoding.ASCII.GetBytes(input);
}


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
string query = Encoding.ASCII.GetString(outBytes, 0, numberOfBytes);
string[] requestLines = query.Split("\r\n");
string httpMethod = requestLines[0].Split(" ")[0];
string httpTarget = requestLines[0].Split(" ")[1];
var userAgentLine = requestLines.Where(l => l.StartsWith("User-Agent")).ToList()[0];
var elements = userAgentLine.Split(' ');
var userAgentName = elements[1];

// string methodSubstring = 
while (true)
{
    if (httpMethod != "GET")
    {
        connection.Send(notFoundResponse);
    }
    else
    {
        if (httpTarget == "/")
        {
            connection.Send(okResponse);
        }
        else if (httpTarget.StartsWith("/echo"))
        {
            //Skip to the part of the string after '/echo/'
            int startingIndex = httpTarget.LastIndexOf("/") + 1;
            string message = httpTarget.Substring(startingIndex);
            string stringResponse =
                $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {message.Length}\r\n\r\n{message}";
            byte[] byteResponse = StringToByteArray(stringResponse);
            connection.Send(byteResponse);
        }
        else if (httpTarget.StartsWith("/user-agent"))
        {
            Console.WriteLine("USER AGENT: " + userAgentName);
            Console.WriteLine("UA Length: " + userAgentName.Length);
            string stringResponse =
                $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {userAgentName.Length}\r\n\r\n{userAgentName}";
            byte[] byteResponse = StringToByteArray(stringResponse);
            connection.Send(byteResponse);
        }
        else
        {
            connection.Send(notFoundResponse);
        }
    }
}


