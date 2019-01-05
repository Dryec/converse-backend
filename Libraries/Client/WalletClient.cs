using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Protocol;

namespace Client
{
    public sealed class WalletClient : IWalletClient
    {
        private Protocol.Wallet.WalletClient _grpcClient;

        public WalletClient(string targetNode)
        {
            var channel = new Channel(targetNode, ChannelCredentials.Insecure);

            _grpcClient = new
                Protocol.Wallet.WalletClient(channel);
        }

        public async Task<NodeList> ListNodesAsync()
        {
            return await _grpcClient.ListNodesAsync(new EmptyMessage());
        }

        public async Task<WitnessList> ListWitnessesAsync()
        {
            return await _grpcClient.ListWitnessesAsync(new EmptyMessage());
        }

        public async Task<Account> GetAccountAsync(Account account)
        {
            return await _grpcClient.GetAccountAsync(account);
        }

        public async Task<Account> GetAccountByIdAsync(Account account)
        {
            return await _grpcClient.GetAccountByIdAsync(account);
        }

        public async Task<TransactionExtention> TransferAsync(TransferContract contract)
        {
            return await _grpcClient.CreateTransaction2Async(contract);
        }

        public async Task<Return> BroadcastTransactionAsync(Transaction transaction)
        {
            return await _grpcClient.BroadcastTransactionAsync(transaction);
        }

        public async Task<TransactionExtention> UpdateAccountAsync(AccountUpdateContract contract)
        {
            return await _grpcClient.UpdateAccount2Async(contract);
        }

        public async Task<TransactionExtention> VoteWitnessAccountAsync(VoteWitnessContract contract)
        {
            return await _grpcClient.VoteWitnessAccount2Async(contract);
        }

        public async Task<TransactionExtention> CreateAssetIssueAsync(AssetIssueContract contract)
        {
            return await _grpcClient.CreateAssetIssue2Async(contract);
        }

        public async Task<TransactionExtention> UpdateWitnessAsync(WitnessUpdateContract contract)
        {
            return await _grpcClient.UpdateWitness2Async(contract);
        }

        public async Task<TransactionExtention> CreateAccountAsync(AccountCreateContract contract)
        {
            return await _grpcClient.CreateAccount2Async(contract);
        }

        public async Task<TransactionExtention> CreateWitnessAsync(WitnessCreateContract contract)
        {
            return await _grpcClient.CreateWitness2Async(contract);
        }

        public async Task<TransactionExtention> TransferAssetAsync(TransferAssetContract contract)
        {
            return await _grpcClient.TransferAsset2Async(contract);
        }

        public async Task<TransactionExtention> ParticipateAssetIssueAsync(ParticipateAssetIssueContract contract)
        {
            return await _grpcClient.ParticipateAssetIssue2Async(contract);
        }

        public async Task<TransactionExtention> FreezeBalanceAsync(FreezeBalanceContract contract)
        {
            return await _grpcClient.FreezeBalance2Async(contract);
        }

        public async Task<TransactionExtention> UnfreezeBalanceAsync(UnfreezeBalanceContract contract)
        {
            return await _grpcClient.UnfreezeBalance2Async(contract);
        }

        public async Task<TransactionExtention> UnfreezeAssetAsync(UnfreezeAssetContract contract)
        {
            return await _grpcClient.UnfreezeAsset2Async(contract);
        }

        public async Task<TransactionExtention> WithdrawBalanceAsync(WithdrawBalanceContract contract)
        {
            return await _grpcClient.WithdrawBalance2Async(contract);
        }

        public async Task<TransactionExtention> UpdateAssetAsync(UpdateAssetContract contract)
        {
            return await _grpcClient.UpdateAsset2Async(contract);
        }

        public async Task<AssetIssueList> GetAssetIssueByAccountAsync(Account account)
        {
            return await _grpcClient.GetAssetIssueByAccountAsync(account);
        }

        public async Task<AccountNetMessage> GetAccountNetAsync(Account account)
        {
            return await _grpcClient.GetAccountNetAsync(account);
        }

        public async Task<AssetIssueContract> GetAssetIssueByNameAsync(BytesMessage message)
        {
            return await _grpcClient.GetAssetIssueByNameAsync(message);
        }

        public async Task<BlockExtention> GetNowBlockAsync()
        {
            return await _grpcClient.GetNowBlock2Async(new EmptyMessage());
        }

        public async Task<BlockExtention> GetBlockByNumAsync(NumberMessage message)
        {
            return await _grpcClient.GetBlockByNum2Async(message);
        }

        public async Task<Block> GetBlockByIdAsync(BytesMessage message)
        {
            return await _grpcClient.GetBlockByIdAsync(message);
        }

        public async Task<BlockListExtention> GetBlockByLimitNextAsync(BlockLimit blockLimit)
        {
            return await _grpcClient.GetBlockByLimitNext2Async(blockLimit);
        }

        public async Task<BlockListExtention> GetBlockByLatestNumAsync(NumberMessage message)
        {
            return await _grpcClient.GetBlockByLatestNum2Async(message);
        }

        public async Task<Transaction> GetTransactionByIdAsync(BytesMessage message)
        {
            return await _grpcClient.GetTransactionByIdAsync(message);
        }

        public async Task<AssetIssueList> GetAssetIssueListAsync()
        {
            return await _grpcClient.GetAssetIssueListAsync(new EmptyMessage());
        }

        public async Task<NumberMessage> TotalTransactionAsync()
        {
            return await _grpcClient.TotalTransactionAsync(new EmptyMessage());
        }

        public async Task<NumberMessage> GetNextMaintenanceTimeAsync()
        {
            return await _grpcClient.GetNextMaintenanceTimeAsync(new EmptyMessage());
        }

        public async Task<AccountResourceMessage> GetAccountResourceAsync(Account account)
        {
            return await _grpcClient.GetAccountResourceAsync(account);
        }

        public async Task<TransactionExtention> ExchangeCreate(ExchangeCreateContract contract)
        {
            return  await _grpcClient.ExchangeCreateAsync(contract);
        }

        public async Task<TransactionExtention> ExchangeInject(ExchangeInjectContract contract)
        {
            return await _grpcClient.ExchangeInjectAsync(contract);
        }

        public async Task<TransactionExtention> ExchangeWithdraw(ExchangeWithdrawContract contract)
        {
            return await _grpcClient.ExchangeWithdrawAsync(contract);
        }

        public async Task<TransactionExtention> ExchangeTransaction(ExchangeTransactionContract contract)
        {
            return await _grpcClient.ExchangeTransactionAsync(contract);
        }

        public async Task<AssetIssueContract> GetAssetIssueByIdAsync(BytesMessage message)
        {
            return await _grpcClient.GetAssetIssueByIdAsync(message);
        }
    }
}
