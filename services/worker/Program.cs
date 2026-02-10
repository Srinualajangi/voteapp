/**
 * VoteApp - Worker Service (.NET Core)
 * =====================================
 * This background worker:
 * 1. Reads votes from Redis (in-memory cache)
 * 2. Processes and aggregates them
 * 3. Stores results in PostgreSQL (persistent database)
 * 
 * Think of it as: A vote counting machine that moves ballots
 * from the collection box (Redis) to the permanent records (PostgreSQL)
 */

using System;
using System.Threading;
using Npgsql;
using StackExchange.Redis;

namespace Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Worker starting...");
            
            // Get environment variables
            var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "redis";
            var pgHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "db";
            var pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
            var pgPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";
            var pgDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "postgres";
            
            var pgConnStr = $"Host={pgHost};Username={pgUser};Password={pgPassword};Database={pgDb}";
            
            // Retry logic for connecting to services
            ConnectionMultiplexer redis = null;
            NpgsqlConnection pgsql = null;
            
            while (true)
            {
                try
                {
                    // Connect to Redis
                    if (redis == null)
                    {
                        Console.WriteLine($"Connecting to Redis at {redisHost}...");
                        redis = ConnectionMultiplexer.Connect(redisHost);
                        Console.WriteLine("Connected to Redis!");
                    }
                    
                    // Connect to PostgreSQL
                    if (pgsql == null)
                    {
                        Console.WriteLine($"Connecting to PostgreSQL at {pgHost}...");
                        pgsql = new NpgsqlConnection(pgConnStr);
                        pgsql.Open();
                        
                        // Create votes table if not exists
                        using (var cmd = new NpgsqlCommand())
                        {
                            cmd.Connection = pgsql;
                            cmd.CommandText = @"
                                CREATE TABLE IF NOT EXISTS votes (
                                    id VARCHAR(255) NOT NULL UNIQUE,
                                    vote VARCHAR(255) NOT NULL
                                )";
                            cmd.ExecuteNonQuery();
                        }
                        Console.WriteLine("Connected to PostgreSQL!");
                    }
                    
                    // Process votes from Redis
                    var db = redis.GetDatabase();
                    var voteJson = db.ListLeftPop("votes");
                    
                    if (voteJson.HasValue)
                    {
                        Console.WriteLine($"Processing vote: {voteJson}");
                        
                        // Parse JSON manually (simple approach)
                        var json = voteJson.ToString();
                        var voterId = ExtractJsonValue(json, "voter_id");
                        var vote = ExtractJsonValue(json, "vote");
                        
                        // Upsert vote into PostgreSQL
                        using (var cmd = new NpgsqlCommand())
                        {
                            cmd.Connection = pgsql;
                            cmd.CommandText = @"
                                INSERT INTO votes (id, vote) VALUES (@id, @vote)
                                ON CONFLICT (id) DO UPDATE SET vote = @vote";
                            cmd.Parameters.AddWithValue("id", voterId);
                            cmd.Parameters.AddWithValue("vote", vote);
                            cmd.ExecuteNonQuery();
                        }
                        
                        Console.WriteLine($"Vote recorded: {voterId} -> {vote}");
                    }
                    else
                    {
                        // No votes to process, wait a bit
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    redis = null;
                    pgsql?.Dispose();
                    pgsql = null;
                    Thread.Sleep(1000);
                }
            }
        }
        
        static string ExtractJsonValue(string json, string key)
        {
            // Simple JSON extraction (production would use JSON library)
            var pattern = $"\"{key}\":\"";
            var start = json.IndexOf(pattern) + pattern.Length;
            var end = json.IndexOf("\"", start);
            return json.Substring(start, end - start);
        }
    }
}
