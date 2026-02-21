using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;


// var okResponse = StringToByteArray("HTTP/1.1 200 OK\r\n\r\n");
// var notFoundResponse = StringToByteArray("HTTP/1.1 404 Not Found\r\n\r\n");
// var badRequestResponse = StringToByteArray("HTTP/1.1 400 Bad Request\r\n\r\n");
// var createdResponse = StringToByteArray("HTTP/1.1 201 Created\r\n\r\n");

string AddBodyToResponse(string response, string bodyContentAsString)
{
    string newResponse = string.Concat(response, bodyContentAsString);
    return newResponse;
}

string AddHeaderToResponse(string response, string headerNameValuePair)
{
    string newResponse = string.Concat(response, headerNameValuePair + "\r\n");
    return newResponse;
}

byte[] createBadRequestResponse(bool closeConnection, string? bodyContent = null)
{
    string badRequestResponse = "HTTP/1.1 400 BadRequest\r\n";
    if (closeConnection)
    {
        badRequestResponse = AddHeaderToResponse(badRequestResponse, $"Connection: close\r\n");
    }
    if (bodyContent != null)
    {
        badRequestResponse = AddHeaderToResponse(badRequestResponse, $"Content-Type: text/plain\r\n" +
                                                                     $"Content-Length: {bodyContent.Length}\r\n");
        badRequestResponse =  AddBodyToResponse(badRequestResponse, bodyContent);
        
    }
    return StringToByteArray(badRequestResponse);
}

byte[] CreateCreatedResponse(bool closeConnection, string? bodyContent = null)
{
    var createdResponse = "HTTP/1.1 201 Created\r\n";
    if (closeConnection)
    {
        createdResponse = AddHeaderToResponse(createdResponse, $"Connection: close\r\n\r\n");
    }
    if (bodyContent != null)
    {
        createdResponse = AddHeaderToResponse(createdResponse, $"Content-Type: text/plain\r\n\r\n" +
                                                               $"Content-Length: {bodyContent.Length}\r\n");
        createdResponse =  AddBodyToResponse(createdResponse, bodyContent);
    }
    
    return StringToByteArray(createdResponse);
}

byte[] CreateOKResponse(bool closeConnection, string? bodyContent = null)
{
    Console.WriteLine("bodyContent: " +  bodyContent);
    var okResponse = "HTTP/1.1 200 OK\r\n";
    if (closeConnection)
    {
        okResponse = AddHeaderToResponse(okResponse, $"Connection: close\r\n");
    }
    if (bodyContent != null)
    {
        okResponse = AddHeaderToResponse(okResponse, $"Content-Type: text/plain\r\n" +
                                                     $"Content-Length: {bodyContent.Length}\r\n");
        okResponse =  AddBodyToResponse(okResponse, bodyContent);
    }

    okResponse += "\r\n";
    Console.WriteLine(okResponse);
    return StringToByteArray(okResponse);
}

byte[] CreateNotFoundResponse(bool closeConnection, string? bodyContent = null)
{
    var notFoundResponse = "HTTP/1.1 404 Not Found\r\n";
    if (closeConnection)
    {
        notFoundResponse = AddHeaderToResponse(notFoundResponse, $"Connection: close\r\n\r\n");
    }
    
    if (bodyContent != null)
    {
        notFoundResponse = AddHeaderToResponse(notFoundResponse, $"Content-Type: text/plain\r\n\r\n" +
                                                                 $"Content-Length: {bodyContent.Length}\r\n\r\n");
        notFoundResponse =  AddBodyToResponse(notFoundResponse, bodyContent);
    }
    return StringToByteArray(notFoundResponse);
}

byte[] CreateGzippedResponse(string bodyContent, bool closeConnection)
{
    byte[] bodyContentAsBytes = [];
    using (MemoryStream memoryStream = new MemoryStream())
    {
        using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
        {
            using (StreamWriter streamWriter = new StreamWriter(gzipStream, Encoding.ASCII))
            {
                streamWriter.Write(bodyContent);
            }

            bodyContentAsBytes = memoryStream.ToArray();
        }
        
        string responseHeadersString = $"HTTP/1.1 200 OK\r\n";
        AddHeaderToResponse(responseHeadersString, "Content-Encoding: gzip");
        AddHeaderToResponse(responseHeadersString, "Content-Type: text/plain");
        AddHeaderToResponse(responseHeadersString, "Content-Type: text/plain");

        if (closeConnection)
            AddHeaderToResponse(responseHeadersString, "Connection: close");
        
        byte[] headerBytes =  Encoding.ASCII.GetBytes(responseHeadersString);
        byte[] response = new byte[headerBytes.Length + bodyContentAsBytes.Length];
        
        Buffer.BlockCopy(headerBytes, 0, response, 0, headerBytes.Length);
        Buffer.BlockCopy(bodyContentAsBytes, 0, response, headerBytes.Length, bodyContentAsBytes.Length);

        memoryStream.Dispose();
        return response;
    }
}

string CheckForConnectionHeader(string[] requestLines)
{
    string conectionCloseValue = "";
    for (int i = 0; i < requestLines.Length - 1; i++)
    {
        if (requestLines[i].StartsWith("Connection:", StringComparison.Ordinal))
        {
            conectionCloseValue = requestLines[i].Substring("Connection: ".Length);
        }
    }

    return conectionCloseValue; 
}

string[] CheckForAcceptedEncoding(string[] requestLines)
{
    string[] encodingLine = [];
    for (int i = 0; i < requestLines.Length - 1; i++)
    {
        if (requestLines[i].StartsWith("Accept-Encoding", StringComparison.Ordinal))
        {
            var myString = requestLines[i].Substring("Accept-Encoding: ".Length);
            encodingLine = myString.Split(", ");
        }
    }

    return encodingLine;
}

string GetMessageBody(string[] requestLines)
{
    int messageBodyIndex = 0;
    for (int i = 0; i < requestLines.Length-1; i++)
    {
        if (requestLines[i] == "" && requestLines.Length > i + 1);
        {
            messageBodyIndex = i + 1;
        }
    }

    string messageBody = "";
    if (messageBodyIndex > 0)
    {
        messageBody = requestLines[messageBodyIndex];
    }

    return messageBody;
}

byte[] StringToByteArray(string input)
{
    return Encoding.ASCII.GetBytes(input);
}

void HandleConnection(Socket connection)
{
    byte[] outBytes = new byte[1024];
    connection.SetSocketOption(SocketOptionLevel.Socket,  SocketOptionName.KeepAlive, true);
    connection.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 3);

    while (connection.Connected)
    {
        try
        {
            int numberOfBytes = connection.Receive(outBytes);

            string query = Encoding.ASCII.GetString(outBytes, 0, numberOfBytes);
            if (query == "" || query == null)
                throw new NullReferenceException();
            
            string[] requestLines = query.Split("\r\n");

            string httpMethod = requestLines[0].Split(" ")[0];
            string httpTarget = requestLines[0].Split(" ")[1];
            string connectionHeaderValue = CheckForConnectionHeader(requestLines);
            
            bool closeConnection = false;
            if (connectionHeaderValue.ToLower() == "close")
            {
                closeConnection = true;
            }
            string messageBody = GetMessageBody(requestLines);
            
            if (httpMethod == "GET")
            {
                var acceptEncodingValue = CheckForAcceptedEncoding(requestLines);

                if (httpTarget == "/")
                {
                    connection.Send(CreateOKResponse(closeConnection));
                }
                else if (httpTarget.StartsWith("/echo"))
                {
                    //Skip to the part of the string after '/echo/'
                    int startingIndex = httpTarget.LastIndexOf("/") + 1;
                    string message = httpTarget.Substring(startingIndex);
                    byte[] byteResponse;
                    if (acceptEncodingValue.Contains("gzip"))
                    {
                        byteResponse = CreateGzippedResponse(message, closeConnection);
                    }
                    else
                    {

                        byteResponse = CreateOKResponse(closeConnection, message);
                    }

                    connection.Send(byteResponse);
                }
                else if (httpTarget.StartsWith("/user-agent"))
                {
                    var userAgentLine = requestLines.Where(l => l.StartsWith("User-Agent")).ToList()[0];
                    var elements = userAgentLine.Split(' ');
                    var userAgentName = elements[1];
                    string stringResponse =
                        $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {userAgentName.Length}\r\n\r\n{userAgentName}";
                    Console.WriteLine("poozer " + userAgentName);
                    byte[] byteResponse = CreateOKResponse(closeConnection, userAgentName);
                    
                    connection.Send(byteResponse);
                }
                else if (httpTarget.StartsWith("/files"))
                {
                    // remove /files
                    string fileName = httpTarget.Substring(7);
                    string filePath = args[1] + fileName;
                    Console.WriteLine("File Path: " + filePath);
                    if (File.Exists(filePath))
                    {
                        string fileContent = File.ReadAllText(filePath);
                        string stringResponse =
                            $"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {fileContent.Length}\r\n\r\n{fileContent}";

                        if (closeConnection)
                            stringResponse = AddHeaderToResponse(stringResponse, "Connection: close");
                        
                        byte[] byteResponse = StringToByteArray(stringResponse);
                        connection.Send(byteResponse);
                    }
                    else
                    {
                        connection.Send(CreateNotFoundResponse(closeConnection));
                    }

                }
                else
                {
                    connection.Send(CreateNotFoundResponse(closeConnection));
                }
            }
            else if (httpMethod == "POST")
            {
                if (httpTarget.StartsWith("/files"))
                {
                    // remove /files
                    string fileName = httpTarget.Substring(7);
                    string directory = "/tmp/";
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] == "--directory" && args.Length > (i + 1))
                        {
                            directory = args[i + 1];
                        }
                    }

                    Directory.CreateDirectory(directory);
                    File.WriteAllText(directory + fileName, messageBody);
                    connection.Send(CreateCreatedResponse(closeConnection));
                }
                else
                {
                    connection.Send(CreateNotFoundResponse(closeConnection));
                }
            }
            else
            {
                connection.Send(CreateNotFoundResponse(closeConnection));
            }

            if (connectionHeaderValue.ToLower() == "close")
            {
                connection.Close();
            }
        }
        catch (Exception e)
        {
            var response = createBadRequestResponse(true);
            connection.Send(response);
            connection.Close();
        }

    }

    // connection.Close();
}


TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

while (true)
{
    var connection = server.AcceptSocket(); // wait for client
    Thread thread = new Thread(() => HandleConnection(connection));
    thread.Start();
}


