﻿namespace ServerSharing.Data
{
    public class Response
    {
        public Response(uint statusCode, string reasonPhrase, string body)
        {
            StatusCode = statusCode;
            Body = body;
            ReasonPhrase = reasonPhrase;
        }

        public bool IsSuccess => StatusCode == 400000u;
        public uint StatusCode { get; private set; }
        public string ReasonPhrase { get; private set; }
        public string Body { get; private set; }
    }
}