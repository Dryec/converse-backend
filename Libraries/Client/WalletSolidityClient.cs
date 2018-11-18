using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Protocol;

namespace Client
{
    public class WalletSolidityClient : IWalletSolidityClient
    {
        private Protocol.WalletSolidity.WalletSolidityClient _grpcClient;

        public WalletSolidityClient(string targetNode)
        {
            var channel = new Channel(targetNode, ChannelCredentials.Insecure);

            _grpcClient = new
                Protocol.WalletSolidity.WalletSolidityClient(channel);
        }

        public async Task<Account> GetAccountAsync(Account account)
        {
            return await _grpcClient.GetAccountAsync(account);
        }

        public async Task<Account> GetAccountByIdAsync(Account account)
        {
            return await _grpcClient.GetAccountByIdAsync(account);
        }

        public async Task<WitnessList> ListWitnessesAsync()
        {
            return await _grpcClient.ListWitnessesAsync(new EmptyMessage());
        }

        public async Task<AssetIssueList> GetAssetIssueListAsync()
        {
            return await _grpcClient.GetAssetIssueListAsync(new EmptyMessage());
        }

        public async Task<BlockExtention> GetNowBlockAsync()
        {
            return await _grpcClient.GetNowBlock2Async(new EmptyMessage());
        }

        public async Task<BlockExtention> GetBlockByNumAsync(NumberMessage message)
        {
            return await _grpcClient.GetBlockByNum2Async(message);
        }

        public async Task<Transaction> GetTransactionByIdAsync(BytesMessage message)
        {
            return await _grpcClient.GetTransactionByIdAsync(message);
        }

        public async Task<TransactionInfo> GetTransactionInfoByIdAsync(BytesMessage message)
        {
            return await _grpcClient.GetTransactionInfoByIdAsync(message);
        }
    }
}
