
namespace mlbapi.services;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using OpenAI;

public class OpenAIServ{
    
    private readonly string _apiKey;
    public OpenAIServ(IConfiguration configuration){
        _apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI:ApiKey is missing (user-secrets/appsettings/env).");
    }
    
}