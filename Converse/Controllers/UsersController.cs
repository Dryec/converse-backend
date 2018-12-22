using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Converse.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Converse.Models;
using Converse.Singleton.WalletClient;
using Google.Protobuf;
using Grpc.Core;
using Newtonsoft.Json;

namespace Converse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DatabaseContext _context;
	    private readonly ILogger _logger;

	    public struct RequestTokensResult
	    {
		    public enum Type
		    {
			    Transferred,
				HasEnough,
			    MaximumReachedPerDay,
			    ServerError,
		    }

		    public Type Result { get; set; }

		    [JsonProperty(PropertyName = "txid")] public string TransactionHash { get; set; }
	    }

		public UsersController(DatabaseContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
	        _logger = loggerFactory.CreateLogger("UsersController");
        }

		// GET: api/Users/{address}
		// Return informations about an user
		[HttpGet("{address}")]
        public async Task<IActionResult> GetUser([FromRoute] string address)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
			
	        if (Client.WalletAddress.Decode58Check(address) == null)
	        {
		        return NotFound();
	        }

	        var user = await _context.Users.SingleOrDefaultAsync(u => u.Address == address);
            if (user == null)
            {
                return NotFound();
            }

	        return Content(JsonConvert.SerializeObject(new Models.View.User(user),
		        new JsonSerializerSettings()
		        {
					NullValueHandling = NullValueHandling.Ignore,
			        DateTimeZoneHandling = DateTimeZoneHandling.Utc
		        }));
        }

		// GET: api/Users/{address}/requestTokens
		// Send an user tokens if not enough left
		[HttpGet("{address}/requestTokens")]
	    public async Task<IActionResult> RequestTokens([FromRoute] string address, [FromServices] WalletClient walletClient, [FromServices] IOptions<Configuration.Token> tokenOptions)
	    {
		    if (!ModelState.IsValid)
		    {
			    return BadRequest(ModelState);
		    }

		    if (Client.WalletAddress.Decode58Check(address) == null)
		    {
			    return NotFound();
		    }

			var result = new RequestTokensResult()
		    {
			    Result = RequestTokensResult.Type.MaximumReachedPerDay,
			    TransactionHash = null,
		    };

			var ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
			var receivedTokensToday = _context.UserReceivedTokens.Where(rt => 
			    (rt.Address == address || rt.Ip == ipAddress) &&
			    rt.CreatedAt.Date == DateTime.Today
			).ToList();
		    var sumTokensReceivedToday = receivedTokensToday.Sum(rt => rt.ReceivedTokens);
		    if (sumTokensReceivedToday < tokenOptions.Value.TransferMaxPerAccountEveryDay)
		    {
			    try
			    {
				    var userAccount = await walletClient.GetAddressInformation(address);
				    var asset = userAccount.Asset.SingleOrDefault(a => a.Key.Equals(tokenOptions.Value.Name, StringComparison.CurrentCultureIgnoreCase));

				    if (asset.Key == null || asset.Value <= tokenOptions.Value.TransferOnlyWhenHasLessOrEqualThan)
				    {
					    var canReceiveToday = (tokenOptions.Value.TransferMaxPerAccountEveryDay - sumTokensReceivedToday);
					    var amount = (canReceiveToday <= tokenOptions.Value.TransferSteps
						    ? canReceiveToday
						    : tokenOptions.Value.TransferSteps
						);

					    var transferResult = await walletClient.TransferTokenFromProperty(amount, address);
					    if (transferResult.Result)
					    {
						    var transactionHash = Common.Utils
							    .ToHexString(Crypto.Sha256.Hash(transferResult.Transaction.RawData.ToByteArray()))
							    .ToLower();

							result.TransactionHash =  transactionHash;
						    result.Result = RequestTokensResult.Type.Transferred;

						    _context.UserReceivedTokens.Add(new UserReceivedToken()
						    {
								Address = address,
								Ip = ipAddress,
								ReceivedTokens = amount,
								CreatedAt = DateTime.UtcNow,
						    });
						    _context.SaveChanges();
					    }
					    else
					    {
						    result.Result = RequestTokensResult.Type.ServerError;
					    }
				    }
				    else
				    {
					    result.Result = RequestTokensResult.Type.HasEnough;
				    }
				}
			    catch (RpcException e)
			    {
				    result.Result = RequestTokensResult.Type.ServerError;
					_logger.LogError("Couldn't transfer tokens to {Receiver}! Error:", address);
					_logger.LogError(e.Message);
					_logger.LogError(e.StackTrace);
				}
			    catch (DbUpdateException e)
			    {
				    _logger.LogError("Couldn't save the userReceivedToken in Database! Error:");
				    _logger.LogError(e.Message);
				    _logger.LogError(e.StackTrace);
				}
			}

			return Ok(result);
		}
    }
}