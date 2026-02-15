using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// TODO: Uncomment the code below to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var connection = server.AcceptSocket(); // wait for client
var response = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
connection.Send(response);
connection.Close();