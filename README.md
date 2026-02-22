## Simple HTTP Server

Project: [Code Crafters Build your own HTTP Server](https://app.codecrafters.io/courses/http-server/overview)

This server supports multiple concurrent connections via sockets, and compressed responses via Gzip. Connections are kept alive for 3 seconds and then closed automatically if no requests are received from the client. If the client specifies it only wants to make one request, the socket is closed once the request is complete.
Supported Routes

#### GET Routes

    /

        Returns 200 OK with an empty response body

    /echo/{message}

        Returns message as the response body

    /user-agent

        Returns the client's provided user-agent header as the response body

    /files

        Returns a file's content as the response body if that file exists on the server.

#### POST Routes

    /files

        Only works when a --directory {directoryName} argument are passed to Server.cs via the command line

        If a directory was provided, the server attempts to stream the client-provided content into a file on the host machine.

        If no directory was provided, it attempts to stream the file into a root-level directory named /tmp/

#### Error Handling

    All other routes return a 400 Not Found.

    Any malformed requests return 400 Bad Request.