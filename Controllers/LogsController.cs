using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using Assignment3BAD.Models;
using MongoDB.Bson;

namespace Assignment3BAD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class LogsController : ControllerBase
    {
        private readonly IMongoCollection<BsonDocument> _logs;
        private readonly ILogger<LogsController> _logger;

        public LogsController(IConfiguration configuration, ILogger<LogsController> logger)
        {
            var client = new MongoClient(configuration["MongoDBSettings:ConnectionString"]);
            var database = client.GetDatabase("LocalFoodAppLogs");
            _logs = database.GetCollection<BsonDocument>("Logs");
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchLogs(
            [FromQuery] string? userId = null,
            [FromQuery] string? userName = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? operation = null)
        {
            try
            {
                var builder = Builders<BsonDocument>.Filter;
                var filters = new List<FilterDefinition<BsonDocument>>();

                if (!string.IsNullOrEmpty(operation))
                {
                    var operationFilter = builder.Or(
                        builder.Regex("RenderedMessage", new BsonRegularExpression($"Operation = {operation}", "i")),
                        builder.Regex("Properties.Result", new BsonRegularExpression($"Operation = {operation}", "i")),
                        builder.Regex("Properties.ErrorDetails", new BsonRegularExpression($"Operation = {operation}", "i")),
                        builder.Regex("Properties.CookDetails", new BsonRegularExpression($"Operation = {operation}", "i"))
                    );
                    filters.Add(operationFilter);
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    var userIdFilter = builder.Or(
                        builder.Regex("RenderedMessage", new BsonRegularExpression($"UserId = {userId}", "i")),
                        builder.Regex("Properties.Result", new BsonRegularExpression($"UserId = {userId}", "i"))
                    );
                    filters.Add(userIdFilter);
                }

                if (!string.IsNullOrEmpty(userName))
                {
                    var userNameFilter = builder.Or(
                        builder.Regex("RenderedMessage", new BsonRegularExpression($"UserName = {userName}", "i")),
                        builder.Regex("Properties.Result", new BsonRegularExpression($"UserName = {userName}", "i"))
                    );
                    filters.Add(userNameFilter);
                }

                if (startDate.HasValue)
                {
                    var danishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");
                    var utcStartDate = TimeZoneInfo.ConvertTimeToUtc(startDate.Value, danishTimeZone);
                    filters.Add(builder.Gte("Timestamp", utcStartDate));
                    _logger.LogInformation($"Søger efter logs efter: {startDate.Value} (Dansk tid) / {utcStartDate} (UTC)");
                }

                if (endDate.HasValue)
                {
                    var danishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");
                    var utcEndDate = TimeZoneInfo.ConvertTimeToUtc(endDate.Value, danishTimeZone);
                    filters.Add(builder.Lte("Timestamp", utcEndDate));
                    _logger.LogInformation($"Søger efter logs før: {endDate.Value} (Dansk tid) / {utcEndDate} (UTC)");
                }

                var filter = filters.Any() ? builder.And(filters) : builder.Empty;

                var logs = await _logs
                    .Find(filter)
                    .Sort(Builders<BsonDocument>.Sort.Descending("Timestamp"))
                    .ToListAsync();

                var danishZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");

                var formattedLogs = logs.Select(log =>
                {
                    var properties = log["Properties"].AsBsonDocument;
                    var details = new Dictionary<string, string>();

                    string? resultString = null;
                    if (properties.Contains("Result"))
                        resultString = properties["Result"].AsString;
                    else if (properties.Contains("ErrorDetails"))
                        resultString = properties["ErrorDetails"].AsString;
                    else if (properties.Contains("CookDetails"))
                        resultString = properties["CookDetails"].AsString;

                    if (!string.IsNullOrEmpty(resultString))
                    {
                        foreach (var detail in resultString.Split(','))
                        {
                            var parts = detail.Split('=').Select(p => p.Trim()).ToArray();
                            if (parts.Length == 2)
                            {
                                var key = parts[0].Trim('{', ' ', '}', '"');
                                var value = parts[1].Trim('{', '}', ' ', '"');
                                details[key] = value;
                            }
                        }
                    }
                    // Operation-del for filtrering (HTTP-metoder):
                    var operation = details.GetValueOrDefault("Operation") ?? 
                                  (log["RenderedMessage"].AsString.Contains("oprettet") ? "POST" :
                                   log["RenderedMessage"].AsString.Contains("opdateret") ? "PUT" :
                                   log["RenderedMessage"].AsString.Contains("slettet") ? "DELETE" : "N/A");

                    var utcTimestamp = log["Timestamp"].ToUniversalTime();
                    var danishTimestamp = TimeZoneInfo.ConvertTimeFromUtc(utcTimestamp, danishZone);

                    return new
                    {
                        Timestamp = danishTimestamp,
                        Level = log["Level"].AsString,
                        Operation = operation,
                        EntityType = details.GetValueOrDefault("EntityType") ?? 
                                   (properties.Contains("SourceContext") ? 
                                    properties["SourceContext"].AsString.Split('.').Last().Replace("Controller", "") : 
                                    "N/A"),
                        UserName = details.GetValueOrDefault("UserName") ?? "N/A",
                        Message = log["MessageTemplate"].AsString,
                        Details = details.Where(kvp => 
                            !new[] { "Operation", "EntityType", "UserName" }.Contains(kvp.Key))
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                        Context = properties["SourceContext"].AsString,
                        RequestPath = properties["RequestPath"].AsString
                    };
                });

                return Ok(formattedLogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved søgning i logs");
                return StatusCode(500, "Der opstod en fejl ved søgning i logs");
            }
        }
    }
}
