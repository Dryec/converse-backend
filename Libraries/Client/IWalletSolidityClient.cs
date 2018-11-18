using System.Threading;
using System.Threading.Tasks;
using Protocol;

namespace Client
{
    public interface IWalletSolidityClient
    {
        Task<Account> GetAccountAsync(Account account);

        Task<Account> GetAccountByIdAsync(Account account);

        Task<WitnessList> ListWitnessesAsync();

        Task<AssetIssueList> GetAssetIssueListAsync();

        Task<BlockExtention> GetNowBlockAsync();

        Task<BlockExtention> GetBlockByNumAsync(NumberMessage message);

        Task<Transaction> GetTransactionByIdAsync(BytesMessage message);

        Task<TransactionInfo> GetTransactionInfoByIdAsync(BytesMessage message);
    }
}