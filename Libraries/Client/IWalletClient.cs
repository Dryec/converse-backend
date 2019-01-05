using System.Threading;
using System.Threading.Tasks;
using Protocol;

namespace Client
{
    public interface IWalletClient
    {
        Task<NodeList> ListNodesAsync();

        Task<WitnessList> ListWitnessesAsync();

        Task<Account> GetAccountAsync(Account account);

        Task<Account> GetAccountByIdAsync(Account account);

        Task<AccountNetMessage> GetAccountNetAsync(Account account);

        Task<TransactionExtention> TransferAsync(TransferContract contract);

        Task<Return> BroadcastTransactionAsync(Transaction transaction);

        Task<TransactionExtention> UpdateAccountAsync(AccountUpdateContract contract);

        Task<TransactionExtention> VoteWitnessAccountAsync(VoteWitnessContract contract);

        Task<TransactionExtention> CreateAssetIssueAsync(AssetIssueContract contract);

        Task<TransactionExtention> UpdateWitnessAsync(WitnessUpdateContract contract);

        Task<TransactionExtention> CreateAccountAsync(AccountCreateContract contract);
        
        Task<TransactionExtention> CreateWitnessAsync(WitnessCreateContract contract);
        
        Task<TransactionExtention> TransferAssetAsync(TransferAssetContract contract);
        
        Task<TransactionExtention> ParticipateAssetIssueAsync(ParticipateAssetIssueContract contract);
        
        Task<TransactionExtention> FreezeBalanceAsync(FreezeBalanceContract contract);
        
        Task<TransactionExtention> UnfreezeBalanceAsync(UnfreezeBalanceContract contract);
        
        Task<TransactionExtention> UnfreezeAssetAsync(UnfreezeAssetContract contract);
        
        Task<TransactionExtention> WithdrawBalanceAsync(WithdrawBalanceContract contract);
        
        Task<TransactionExtention> UpdateAssetAsync(UpdateAssetContract contract);
        
        Task<AssetIssueList> GetAssetIssueByAccountAsync(Account account);
        
        Task<AccountResourceMessage> GetAccountResourceAsync(Account account);

        Task<AssetIssueContract> GetAssetIssueByNameAsync(BytesMessage message);

        Task<AssetIssueContract> GetAssetIssueByIdAsync(BytesMessage message);

        Task<BlockExtention> GetNowBlockAsync();
        
        Task<BlockExtention> GetBlockByNumAsync(NumberMessage message);

        Task<Block> GetBlockByIdAsync(BytesMessage message);
        
        Task<BlockListExtention> GetBlockByLimitNextAsync(BlockLimit blockLimit);
        
        Task<BlockListExtention> GetBlockByLatestNumAsync(NumberMessage message);
        
        Task<Transaction> GetTransactionByIdAsync(BytesMessage message);
        
        Task<AssetIssueList> GetAssetIssueListAsync();
        
        Task<NumberMessage> TotalTransactionAsync();
        
        Task<NumberMessage> GetNextMaintenanceTimeAsync();

        Task<TransactionExtention> ExchangeCreate(ExchangeCreateContract contract);
        Task<TransactionExtention> ExchangeInject(ExchangeInjectContract contract);
        Task<TransactionExtention> ExchangeWithdraw(ExchangeWithdrawContract contract);
        Task<TransactionExtention> ExchangeTransaction(ExchangeTransactionContract contract);
    }
}
