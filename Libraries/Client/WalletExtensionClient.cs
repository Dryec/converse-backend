using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Protocol;

namespace Client
{
    public class WalletExtensionClient : IWalletExtensionClient
    {
        private Protocol.WalletExtension.WalletExtensionClient _grpcClient;

        public WalletExtensionClient(string targetNode)
        {
            var channel = new Channel(targetNode, ChannelCredentials.Insecure);

            _grpcClient = new
                Protocol.WalletExtension.WalletExtensionClient(channel);
        }

        public async Task<TransactionListExtention> GetTransactionsFromThisAsync(AccountPaginated accountPagination)
        {
            return await _grpcClient.GetTransactionsFromThis2Async(accountPagination);
        }

        public async Task<TransactionListExtention> GetTransactionsToThisAsync(AccountPaginated accountPagination)
        {
            return await _grpcClient.GetTransactionsToThis2Async(accountPagination);
        }
    }
}
